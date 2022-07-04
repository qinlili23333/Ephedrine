Imports System.ComponentModel
Imports System.Environment
Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text.Json
Imports System.Windows.Forms
Imports Microsoft.Web.WebView2.Core

Class MainWindow
    Class Config
        '"Internal" to use built in index.html, Or a specify web address Like "https://qinlili.bid"
        Public Property StartPage As String
        'Progress will hide if set to true
        Public Property HideProgress As Boolean
        'Right click will show default webview context menu if enabled
        Public Property EnableContextMenu As Boolean
        '"Always" to enable F12 under any circumstances, Or a specify command Like "--debugKey 1145141919810" when launch with this key F12 will be enabled
        Public Property Devtool As String
        'Webview profile saving path, set to "Temp" if need to save in temp folder, same path will share profile in same domain
        Public Property WvPath As String
        'Hide Window Frame
        Public Property HideFrame As Boolean
        'Enable transparent webview if set to true. Must enable HideFrame+DisableResize first.
        Public Property AllowTransparency As Boolean
        'Disable window resize
        Public Property DisableResize As Boolean
        'Pages Not in whitelist will be opened in browser instead of in installer if enabled
        Public Property EnableWhitelist As Boolean
        'Domains that allowed to access in whitelist mode
        Public Property Whitelist As String()
        'Installer title
        Public Property Title As String
    End Class
    Dim InternalConfig As Config
    Class JSONFormat
        Public Property Action As String
        Public Property Arg1 As String
        Public Property Arg2 As String
        Public Property Arg3 As String
        Public Property Arg4 As String
        Public Property Arg5 As String
    End Class
    Class FileList
        Public Property Size As Long
        Public Property Name As String
        Public Property IsFolder As Boolean
    End Class
    Dim IsBusy As Boolean = False
    Dim Service As Process
    Dim webView2Environment As CoreWebView2Environment
    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。

        ReadConfig()
    End Sub

    Private Async Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Status.Content = "Loading WebView2..."
        If Not File.Exists("WebView2Loader.dll") Then
            Dim fs As New FileStream("WebView2Loader.dll", FileMode.Create)
            Assembly.GetExecutingAssembly().GetManifestResourceStream("WebModInstaller.WebView2Loader.dll").CopyTo(fs)
            fs.Close()
            fs.Dispose()
        End If
        If InternalConfig.WvPath = "Temp" Then
            Directory.CreateDirectory(Path.GetTempPath + "QinliliWebview2\")
            SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", Path.GetTempPath + "QinliliWebview2\")
            webView2Environment = Await CoreWebView2Environment.CreateAsync(, Path.GetTempPath + "QinliliWebview2\Cache",)
        Else
            Directory.CreateDirectory(GetFolderPath(SpecialFolder.ApplicationData) + "\QinliliWebview2\" + InternalConfig.WvPath)
            SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", GetFolderPath(SpecialFolder.ApplicationData) + "\QinliliWebview2\" + InternalConfig.WvPath)
            webView2Environment = Await CoreWebView2Environment.CreateAsync(, GetFolderPath(SpecialFolder.ApplicationData) + "\QinliliWebview2\" + InternalConfig.WvPath,)
        End If
        Await MainWeb.EnsureCoreWebView2Async()
        Progress.IsIndeterminate = False
        Progress.Value = 10
        MainWeb.CoreWebView2.Settings.IsStatusBarEnabled = False
        MainWeb.CoreWebView2.Settings.AreDefaultContextMenusEnabled = InternalConfig.EnableContextMenu
        If Not InternalConfig.Devtool = "Always" And Not InternalConfig.Devtool = Command() Then
            MainWeb.CoreWebView2.Settings.AreDevToolsEnabled = False
        End If
        If InternalConfig.StartPage = "Internal" Then
            Dim reader As StreamReader = New StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("WebModInstaller.index.html"))
            Dim htmlString As String = Await reader.ReadToEndAsync()
            MainWeb.CoreWebView2.NavigateToString(htmlString)
        Else
            MainWeb.CoreWebView2.Navigate(InternalConfig.StartPage)
        End If
    End Sub

    Private Sub ReadConfig()
        InternalConfig = JsonSerializer.Deserialize(Of Config)(Assembly.GetExecutingAssembly().GetManifestResourceStream("WebModInstaller.Config.json"))
        Title = InternalConfig.Title
        Status.Content = "Loading Web Page..."
        If InternalConfig.HideProgress Then
            MainWeb.Margin = New Thickness(0, 0, 0, 0)
            Progress.Visibility = Visibility.Hidden
            Status.Visibility = Visibility.Hidden
        End If
        If InternalConfig.AllowTransparency Then
            Dim WChrome = New Shell.WindowChrome
            WChrome.GlassFrameThickness = New Thickness(-1)
            Shell.WindowChrome.SetWindowChrome(Me, WChrome)
            MainWeb.DefaultBackgroundColor = System.Drawing.Color.Transparent
        End If
        If InternalConfig.HideFrame Then
            Me.WindowStyle = WindowStyle.None
        End If
        If InternalConfig.DisableResize Then
            Me.ResizeMode = ResizeMode.NoResize
        End If
    End Sub
    Private Sub MainWeb_NavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs) Handles MainWeb.NavigationStarting
        Status.Content = "Loading Web Page..."
        If InternalConfig.EnableWhitelist And Not ((New Uri(e.Uri).DnsSafeHost) = "") And Not InternalConfig.Whitelist.Contains(New Uri(e.Uri).DnsSafeHost) Then
            e.Cancel = True
            Interaction.Shell("cmd.exe /c start " + e.Uri + " & exit", AppWinStyle.Hide)
        End If
        Progress.Value = 10
    End Sub

    Private Sub MainWeb_ContentLoading(sender As Object, e As CoreWebView2ContentLoadingEventArgs) Handles MainWeb.ContentLoading
        Progress.Value = 25
    End Sub
    Private Sub MainWeb_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles MainWeb.NavigationCompleted
        Status.Content = "Ready."
        Progress.Value = 100
    End Sub

    Private Sub MainWeb_WebMessageReceived(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs) Handles MainWeb.WebMessageReceived
        If Not IsBusy Then
            Dim Message = JsonSerializer.Deserialize(Of JSONFormat)(e.WebMessageAsJson)
            Progress.Value = 0
            IsBusy = True
            Progress.IsIndeterminate = True
            Status.Content = "Processing..."
            Select Case Message.Action
                'Action 1
                Case "Install"
                    Dim Method As String = Message.Arg1
                    Dim Link As String = Message.Arg2
                    Dim Location As String = Message.Arg3
                    Select Case Message.Arg1
                        'Extract only
                        Case "zip"
                            InstallZip(Link, Location)
                        'No exctract and directly run
                        Case "run"
                            DownloadAndRun(Link, Location, Message.Arg4, Message.Arg5)
                        'Only save to local
                        Case "save"
                            DownloadOnly(Link, Location)
                    End Select
                'Action 2
                Case "Plugin"
                    Dim window As New Plugin(Message.Arg1, Message.Arg2, Message.Arg3, Message.Arg4 = "ForceUpdate", Message.Arg5 = "InstallOnly")
                    window.Show()
                    '21 Start Plugin Success
                    Status.Content = "Plugin Started."
                    MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(21)")
                    Progress.Value = 100
                    Progress.IsIndeterminate = False
                    IsBusy = False
                'Action 3
                Case "List"
                    Try
                        Dim dirPath As String
                        If Message.Arg1 = "Here" Or Message.Arg1 = Nothing Then
                            dirPath = ".\"
                        Else
                            dirPath = Message.Arg1
                        End If
                        Dim di As New DirectoryInfo(dirPath)
                        Dim aryFi As FileInfo() = di.GetFiles()
                        Dim aryDr As DirectoryInfo() = di.GetDirectories()
                        Dim fiInfo As Object() = New Object(aryFi.Length + aryDr.Length - 1) {}
                        Dim num As Integer = 0
                        For Each dr As DirectoryInfo In aryDr
                            fiInfo(num) = New FileList With {
                                .Size = 0,
                                .Name = dr.Name,
                                .IsFolder = True
                            }
                            num += 1
                        Next
                        For Each fi As FileInfo In aryFi
                            fiInfo(num) = New FileList With {
                                .Size = fi.Length,
                                .Name = fi.Name,
                                .IsFolder = False
                            }
                            num += 1
                        Next
                        Convert.ToBase64String(Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fiInfo)))
                        '31 List Success
                        Status.Content = "List Folder Success."
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(31)")
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('" + Convert.ToBase64String(Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fiInfo))) + "')")
                    Catch ex As Exception
                        File.WriteAllTextAsync("Error.log", ex.ToString)
                        '30 List Failed
                        Status.Content = "List Folder Failed."
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(30)")
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('')")
                    End Try
                    Progress.Value = 100
                    Progress.IsIndeterminate = False
                    IsBusy = False
                'Action 4
                Case "Select"
                    Select Case Message.Arg1
                        Case "Folder"
                            Dim f As New FolderBrowserDialog With {
                                .Description = "Select the directory of game.",
                                .ShowNewFolderButton = False,
                                .AutoUpgradeEnabled = True
                            }
                            If f.ShowDialog() = Forms.DialogResult.OK Then
                                '41 Select Success
                                Status.Content = "Select Folder Success."
                                MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(41)")
                                MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('" + f.SelectedPath.Replace("\", "\\") + "')")
                            Else
                                '40 Select Canceled
                                Status.Content = "Select Folder Canceled."
                                MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(40)")
                                MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('')")
                            End If
                            Progress.Value = 100
                            Progress.IsIndeterminate = False
                            IsBusy = False
                        Case "File"
                            Dim fd As New OpenFileDialog() With {
                                .CheckFileExists = True,
                                .CheckPathExists = True,
                                .Title = "Select the game file.",
                                .Multiselect = False,
                                .RestoreDirectory = True,
                                .InitialDirectory = CurrentDirectory
                                }
                            If fd.ShowDialog() = Forms.DialogResult.OK Then
                                '41 Select Success
                                Status.Content = "Select File Success."
                                MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(41)")
                                MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('" + fd.FileName.Replace("\", "\\") + "')")
                            Else
                                '40 Select Canceled
                                Status.Content = "Select File Canceled."
                                MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(40)")
                                MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('')")
                            End If
                            Progress.Value = 100
                            Progress.IsIndeterminate = False
                            IsBusy = False
                    End Select
                'Action 5
                Case "Delete"
                    If (File.Exists(Message.Arg1)) Then
                        Try
                            File.Delete(Message.Arg1)
                            Status.Content = "Delete Success."
                            '51 Delete Success
                            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(51)")
                        Catch ex As Exception
                            Status.Content = "Delete Fail."
                            '52 Delete Fail
                            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(52)")
                        End Try
                    Else
                        Status.Content = "File Not Found."
                        '50 File Not Found
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(50)")
                    End If
                    Progress.Value = 100
                    Progress.IsIndeterminate = False
                    IsBusy = False
                'Action 6
                Case "Run"
                    Try
                        Dim Run As Process
                        Select Case Message.Arg3
                            Case "Admin"
                                Dim info As New ProcessStartInfo(Message.Arg1, Message.Arg2) With {
                                            .UseShellExecute = True,
                                            .Verb = "runas"
                                        }
                                Run = Process.Start(info)
                            Case "User"
                                Run = Process.Start(Message.Arg1, Message.Arg2)
                            Case "Intent"
                                Interaction.Shell("cmd.exe /c start " + Message.Arg1 + " & exit", AppWinStyle.Hide)
                        End Select
                        '61 Run Success
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(61)")
                        Status.Content = "Start Process Success."
                    Catch ex As Exception
                        If ex.Message.Contains("The requested operation requires elevation.") Then
                            '62 No Permission
                            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(62)")
                            Status.Content = "Require Elevation."
                        ElseIf ex.Message.Contains("The system cannot find the file specified.") Then
                            '63 No File
                            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(63)")
                            Status.Content = "Program Not Found."
                        Else
                            File.WriteAllTextAsync("Error.log", ex.ToString)
                            '60 Run Fail
                            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(60)")
                            Status.Content = "Start Process Fail."
                        End If
                    End Try
                    Progress.Value = 100
                    Progress.IsIndeterminate = False
                    IsBusy = False
                'Action 7
                Case "KillProcess"
                    Dim Proc = Process.GetProcessesByName(Message.Arg1)
                    If Proc.Length > 0 Then
                        For Each ToKill In Proc
                            ToKill.Kill()
                        Next
                        Status.Content = "Kill Process Success."
                        '71 Kill Success
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(71)")
                    Else
                        Status.Content = "No Process Found."
                        '70 Nothing To Kill
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(70)")
                    End If
                    Progress.Value = 100
                    Progress.IsIndeterminate = False
                    IsBusy = False
                'Acrion 8 
                Case "Verify"
                    Hash(Message.Arg1, Message.Arg2)
                'Action 9
                Case "StartService"
                    MsgBox(Message.Arg1)
                    'Detect if service died
                    If Service IsNot Nothing Then
                        If Service.HasExited Then
                            Service = Nothing
                        End If
                    End If
                    If Service Is Nothing Then
                        Status.Content = "Starting Service..."
                        Select Case Message.Arg1
                            Case "User"
                                Service = Process.Start(ProcessPath, "--service")
                            Case "Admin"
                                Dim info As New ProcessStartInfo(ProcessPath, "--service") With {
                                    .UseShellExecute = True,
                                    .Verb = "runas"
                                }
                                Try
                                    Service = Process.Start(info)
                                Catch ex As Exception
                                    MsgBox("Need administrator permission to run service.",, "Error")
                                    Status.Content = "Administrator Permission Denied."
                                    Progress.Value = 100
                                    Progress.IsIndeterminate = False
                                    IsBusy = False
                                    '90 Start Fail
                                    MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(90)")
                                    Exit Sub
                                End Try
                        End Select
                        If Service IsNot Nothing Then
                            '91 Start Success
                            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(91)")
                        Else
                            MsgBox("Fail to start service. Check if antivirus software stop it.",, "Error")
                            Status.Content = "Fail to start service."
                            Progress.Value = 100
                            Progress.IsIndeterminate = False
                            IsBusy = False
                            '90 Start Fail
                            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(90)")
                        End If
                    Else
                        '93 Duplicate Service
                        MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(93)")
                    End If
                    Status.Content = "Ready."
                    Progress.Value = 100
                    Progress.IsIndeterminate = False
                    IsBusy = False
                'Action 10
                Case "Exit"
                    Close()
            End Select
        Else
            '-1 Busy
            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(-1)")
        End If
    End Sub

    'Download and install zip file
    Private Sub InstallZip(link As String, loc As String)
        Dim Location As String
        If loc = "Here" Then
            Location = ".\"
        Else
            Location = loc
        End If
        Status.Content = "Downloading Patch File..."
        If File.Exists("QinliliPatch.zip") Then
            Try
                File.Delete("QinliliPatch.zip")
            Catch ex As Exception
                File.WriteAllTextAsync("Error.log", ex.ToString)
                MsgBox("Failed to delete unused patch file. Please try to delete 'QinliliPatch.zip' manually and continue.",, "Error")
            End Try
        End If
        Dim DownloadClient As New WebClient
        AddHandler DownloadClient.DownloadProgressChanged, (Sub(ByVal sender As Object, ByVal e As DownloadProgressChangedEventArgs)
                                                                Progress.Value = e.ProgressPercentage * 0.7
                                                            End Sub)
        AddHandler DownloadClient.DownloadFileCompleted, (Sub(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs)
                                                              Status.Content = "Extracting Patch File..."
                                                              Dim archive As ZipArchive
                                                              Try
                                                                  archive = ZipFile.OpenRead("QinliliPatch.zip")
                                                              Catch
                                                                  MsgBox("Failed to open patch file. Check your internet connection or contact with patch creator.",, "Error")
                                                                  '10 Corrupt Patch
                                                                  MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(10)")
                                                                  Progress.Value = 100
                                                                  Status.Content = "Installation Failed : Corrupt Patch."
                                                                  IsBusy = False
                                                                  Exit Sub
                                                              End Try
                                                              Try
                                                                  archive.ExtractToDirectory(Location, True)
                                                                  Progress.Value = 90
                                                                  Status.Content = "Cleaning..."
                                                                  archive.Dispose()
                                                                  If File.Exists("QinliliPatch.zip") Then
                                                                      Try
                                                                          File.Delete("QinliliPatch.zip")
                                                                      Catch ex As Exception
                                                                          File.WriteAllTextAsync("Error.log", ex.ToString)
                                                                          MsgBox("Failed to delete unused patch file. Please try to delete 'QinliliPatch.zip' manually.",, "Error")
                                                                      End Try
                                                                  End If
                                                                  Progress.Value = 100
                                                                  Status.Content = "Success Installed Patch."
                                                                  '11 Success Zip Patch
                                                                  MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(11)")
                                                                  IsBusy = False
                                                              Catch ex As Exception
                                                                  If ex.Message.Contains("denied.") Then
                                                                      archive.Dispose()
                                                                      File.WriteAllText("InstallLocation", Location)
                                                                      Dim info As New ProcessStartInfo(ProcessPath, "--unzip") With {
                    .UseShellExecute = True,
                    .Verb = "runas"
                }
                                                                      Try
                                                                          Dim ZipProgress = Process.Start(info)
                                                                          ZipProgress.EnableRaisingEvents = True
                                                                          AddHandler ZipProgress.Exited, Sub()
                                                                                                             Dispatcher.Invoke(Sub()
                                                                                                                                   If File.Exists("QinliliPatch.zip") Then
                                                                                                                                       Try
                                                                                                                                           File.Delete("QinliliPatch.zip")
                                                                                                                                       Catch ex2 As Exception
                                                                                                                                           File.WriteAllTextAsync("Error.log", ex.ToString)
                                                                                                                                           MsgBox("Failed to delete unused patch file. Please try to delete 'QinliliPatch.zip' manually.",, "Error")
                                                                                                                                       End Try
                                                                                                                                   End If
                                                                                                                                   If File.Exists("InstallLocation") Then
                                                                                                                                       Try
                                                                                                                                           File.Delete("InstallLocation")
                                                                                                                                       Catch ex2 As Exception
                                                                                                                                           File.WriteAllTextAsync("Error.log", ex.ToString)
                                                                                                                                           MsgBox("Failed to delete unused patch file. Please try to delete 'InstallLocation' manually.",, "Error")
                                                                                                                                       End Try
                                                                                                                                   End If
                                                                                                                                   Select Case ZipProgress.ExitCode
                                                                                                                                       Case -1
                                                                                                                                           Status.Content = "Failed To Installed Patch."
                                                                                                                                           '10 Zip Patch Fail
                                                                                                                                           MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(10)")
                                                                                                                                       Case 1
                                                                                                                                           Status.Content = "Success Installed Patch."
                                                                                                                                           '11 Success Zip Patch
                                                                                                                                           MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(11)")
                                                                                                                                   End Select
                                                                                                                                   Progress.Value = 100
                                                                                                                                   IsBusy = False
                                                                                                                               End Sub)
                                                                                                         End Sub
                                                                      Catch ex2 As Exception
                                                                          MsgBox("Permission denied for temporary elevating. Please run installer as administrator and try again.",, "Error")
                                                                          If File.Exists("QinliliPatch.zip") Then
                                                                              Try
                                                                                  File.Delete("QinliliPatch.zip")
                                                                              Catch ex3 As Exception
                                                                                  File.WriteAllTextAsync("Error.log", ex.ToString)
                                                                                  MsgBox("Failed to delete unused patch file. Please try to delete 'QinliliPatch.zip' manually.",, "Error")
                                                                              End Try
                                                                          End If
                                                                          If File.Exists("InstallLocation") Then
                                                                              Try
                                                                                  File.Delete("InstallLocation")
                                                                              Catch ex3 As Exception
                                                                                  File.WriteAllTextAsync("Error.log", ex.ToString)
                                                                                  MsgBox("Failed to delete unused patch file. Please try to delete 'InstallLocation' manually.",, "Error")
                                                                              End Try
                                                                          End If
                                                                          Progress.Value = 100
                                                                          Status.Content = "Permission Denied."
                                                                          '13 Zip Patch Permission Denied
                                                                          MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(13)")
                                                                          IsBusy = False
                                                                      End Try
                                                                  End If
                                                              End Try
                                                          End Sub)
        DownloadClient.DownloadFileAsync(New Uri(link), "QinliliPatch.zip")
        Progress.IsIndeterminate = False
    End Sub
    Private Sub DownloadOnly(link As String, name As String)
        Progress.IsIndeterminate = False
        Status.Content = "Downloading Patch File..."
        Dim DownloadClient As New WebClient
        AddHandler DownloadClient.DownloadProgressChanged, (Sub(ByVal sender As Object, ByVal e As DownloadProgressChangedEventArgs)
                                                                Progress.Value = e.ProgressPercentage
                                                            End Sub)
        AddHandler DownloadClient.DownloadFileCompleted, (Sub()
                                                              '15 Download Success Without Verification
                                                              MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(15)")
                                                              Status.Content = "Download Patch Success."
                                                              Progress.Value = 100
                                                              IsBusy = False
                                                          End Sub)
        DownloadClient.DownloadFileAsync(New Uri(link), name)
    End Sub
    Private Sub DownloadAndRun(link As String, name As String, user As String, argu As String)
        Progress.IsIndeterminate = False
        Status.Content = "Downloading Patch File..."
        Dim DownloadClient As New WebClient
        AddHandler DownloadClient.DownloadProgressChanged, (Sub(ByVal sender As Object, ByVal e As DownloadProgressChangedEventArgs)
                                                                Progress.Value = e.ProgressPercentage
                                                            End Sub)
        AddHandler DownloadClient.DownloadFileCompleted, (Sub()
                                                              Dim RunProc As Process
                                                              Select Case user
                                                                  Case "User"
                                                                      Try
                                                                          RunProc = Process.Start(name, argu)
                                                                          '16 Download And Run Success 
                                                                          MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(16)")
                                                                          Status.Content = "Run Patch Success."
                                                                          Progress.Value = 100
                                                                          IsBusy = False
                                                                      Catch ex As Exception
                                                                          '18 Run Fail
                                                                          MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(18)")
                                                                          Status.Content = "Failed To Run Patch."
                                                                          Progress.Value = 100
                                                                          IsBusy = False
                                                                      End Try
                                                                  Case "Admin"
                                                                      Dim info As New ProcessStartInfo(name, argu) With {
                                            .UseShellExecute = True,
                                            .Verb = "runas"
                                        }
                                                                      Try
                                                                          RunProc = Process.Start(info)
                                                                      Catch ex As Exception
                                                                          MsgBox("Need administrator permission to run service.",, "Error")
                                                                          Status.Content = "Administrator Permission Denied."
                                                                          Progress.Value = 100
                                                                          Progress.IsIndeterminate = False
                                                                          IsBusy = False
                                                                          '17 No Permission Start
                                                                          MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(17)")
                                                                          Exit Sub
                                                                      End Try
                                                              End Select
                                                          End Sub)
        DownloadClient.DownloadFileAsync(New Uri(link), name)
    End Sub

    Async Sub Hash(method As String, file_name As String)
        Status.Content = "Verify Hash..."
        If File.Exists(file_name) Then
            Dim hash As HashAlgorithm
            Select Case method
                Case "MD5"
                    hash = MD5.Create()
                Case "SHA1"
                    hash = SHA1.Create()
                Case "SHA256"
                    hash = SHA256.Create()
                Case "SHA384"
                    hash = SHA384.Create()
                Case "SHA512"
                    hash = SHA512.Create()
                Case Else
                    '82 Unsupport Method
                    Status.Content = "Unsupported Verify Method"
                    Await MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(82)")
                    Await MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('')")
                    Progress.IsIndeterminate = False
                    Progress.Value = 100
                    IsBusy = False
                    Exit Sub
            End Select
            Dim hashValue() As Byte
            Dim fileStream As FileStream = File.OpenRead(file_name)
            fileStream.Position = 0
            hashValue = Await hash.ComputeHashAsync(fileStream)
            fileStream.Close()
            Dim result = BitConverter.ToString(hashValue).Replace("-", "").ToUpperInvariant()
            'MsgBox(result)
            Status.Content = "Verify Success."
            '81 Verify Finish
            Await MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(81)")
            Await MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('" + result + "')")
        Else
            '80 No File
            Status.Content = "Cannot Found File To Verify."
            Await MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(80)")
            Await MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgResult('')")
        End If
        Progress.IsIndeterminate = False
        Progress.Value = 100
        IsBusy = False
    End Sub


    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        MainWeb.Dispose()
        Try
            File.Delete("WebView2Loader.dll")
        Catch ex As Exception

        End Try
    End Sub
End Class
