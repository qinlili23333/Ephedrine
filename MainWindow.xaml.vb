Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Reflection
Imports System.Text.Json
Imports Microsoft.Web.WebView2.Core
Imports System.Security.Cryptography

Class MainWindow
    Class JSONFormat
        Public Property Action As String
        Public Property Arg1 As String
        Public Property Arg2 As String
        Public Property Arg3 As String
        Public Property Arg4 As String
        Public Property Arg5 As String
    End Class
    Dim IsBusy As Boolean = False
    Dim Service As Process
    Private Async Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Status.Content = "Loading WebView2..."
        If Not File.Exists("WebView2Loader.dll") Then
            Dim fs As New FileStream("WebView2Loader.dll", FileMode.Create)
            Assembly.GetExecutingAssembly().GetManifestResourceStream("WebModInstaller.WebView2Loader.dll").CopyTo(fs)
            fs.Close()
            fs.Dispose()
        End If
        Directory.CreateDirectory(Path.GetTempPath + "QinliliWebview2\")
        Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", Path.GetTempPath + "QinliliWebview2\")
        Dim webView2Environment = Await CoreWebView2Environment.CreateAsync(, Path.GetTempPath + "QinliliWebview2\Cache",)
        Await MainWeb.EnsureCoreWebView2Async()
        Progress.IsIndeterminate = False
        Progress.Value = 10
        MainWeb.CoreWebView2.Settings.IsStatusBarEnabled = False
        Status.Content = "Loading Web Page..."
        MainWeb.CoreWebView2.Navigate("http://127.0.0.1:8082")
    End Sub


    Private Sub MainWeb_NavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs) Handles MainWeb.NavigationStarting
        Status.Content = "Loading Web Page..."
        'MsgBox(e.Uri)
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

                        'Only save to local
                        Case "save"
                            DownloadOnly(Link, Location)
                    End Select
                'Action 3
                Case "Prepare"
                    MsgBox("Pending Install Depending Module:" + Message.Arg1)
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
                                        Service = Process.Start(Environment.ProcessPath, "--service")
                                    Case "Admin"
                                        Dim info As New ProcessStartInfo(Environment.ProcessPath, "--service") With {
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
                    End Select
                    Else
            '-1 Busy
            MainWeb.CoreWebView2.ExecuteScriptAsync("Ephedrine.msgStatus(-1)")
        End If
    End Sub

    'Download and install zip file
    Private Sub InstallZip(link As String, loc As String)
        Dim Location As String
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
                                                                      Dim info As New ProcessStartInfo(Environment.ProcessPath, "--unzip") With {
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
        If loc = "Here" Then
            Location = ".\"
        Else
            Location = loc
        End If
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
            hashValue = Await Hash.ComputeHashAsync(fileStream)
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
End Class
