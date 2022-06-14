Imports System.IO
Imports System.IO.Compression
Imports System.Net.Http
Imports System.Text.Json
Imports System.Windows.Forms

Public Class Plugin
    ReadOnly PluginPath As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\Ephedrine\Plugins"
    Class PluginInfo
        Public Property Name As String
        Public Property Program As String
        Public Property Argument As String
        Public Property Update As Boolean
        Public Property Install As Boolean
    End Class
    Class PluginJson
        Public Property Name As String
        Public Property Files As String()
        Public Property Dependent As String()
        Public Property Version As String
        Public Property Link As String
        Public Property Source As String
    End Class
    Dim CurrentPlugin As PluginInfo
    Public Sub New(PluginName As String, PluginProgram As String, Argument As String, Optional ForceUpdate As Boolean = False, Optional InstallOnly As Boolean = False)

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        Name.Content = PluginName
        CurrentPlugin = New PluginInfo With {
            .Name = PluginName,
            .Program = PluginProgram,
            .Argument = Argument,
            .Update = ForceUpdate,
            .Install = InstallOnly
        }
        Argu.Text = PluginProgram + " " + Argument
    End Sub

    Private Sub Plugin_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not Directory.Exists(PluginPath) Then
            Dim result As DialogResult = MessageBox.Show("Ephedrine plugins can add more functions for installer, such as diff patcher or 7z extractor. However plugins will store files permenantly on your disk which will use your disk space. You can manage plugins by launch any Ephedrine installer with `--plugins`. Click `Yes` if you want to use plugins.",
                              "Ephedrine",
                              MessageBoxButtons.YesNo)
            If Not result Then
                Close()
            End If
            Directory.CreateDirectory(PluginPath)
        End If
        If Not Directory.Exists(PluginPath + "\Installed") Then
            Directory.CreateDirectory(PluginPath + "\Installed")
        End If
        Process()
    End Sub
    Private Async Function Process() As Task
        If Not File.Exists(PluginPath + "\Installed\" + CurrentPlugin.Name + ".json") Or CurrentPlugin.Update Then
            'Install Plugin
            Await Install(CurrentPlugin.Name)
        End If
        If Not CurrentPlugin.Install Then
            Progress.IsIndeterminate = True
            StatusLabel.Content = "Running actions..."
            Environment.SetEnvironmentVariable("Path", PluginPath + ";" + Environment.GetEnvironmentVariable("Path"))
            Dim SI As New ProcessStartInfo(PluginPath + "\" + CurrentPlugin.Program, CurrentPlugin.Argument) With {
                .CreateNoWindow = True,
                .WindowStyle = ProcessWindowStyle.Hidden
                }
            Dim RunProcess = Diagnostics.Process.Start(SI)
            Await RunProcess.WaitForExitAsync()
        End If
        Progress.Value = 100
        StatusLabel.Content = "Actions done. Window close in 3 seconds."
        Progress.IsIndeterminate = False
        Await Task.Delay(3000)
        Close()
    End Function

    Private Async Function Install(Name As String) As Task
        If Not Directory.Exists(PluginPath + "\Pending") Then
            Directory.CreateDirectory(PluginPath + "\Pending")
        End If
        Progress.IsIndeterminate = False
        StatusLabel.Content = "Getting info..."
        Dim client As New HttpClient
        Try
            Dim response As HttpResponseMessage = Await client.GetAsync("https://cdn.cloudflare.ephedrine.qinlili.bid/Plugin/" + Name + "/Install.json")
            response.EnsureSuccessStatusCode()
            Progress.Value = 10
            Dim responseBody As String = Await response.Content.ReadAsStringAsync()
            Progress.Value = 20
            Await File.WriteAllTextAsync(PluginPath + "\Pending\" + Name + ".json", responseBody)
            Dim InstallJsonParsed = JsonSerializer.Deserialize(Of PluginJson)(responseBody)
            Progress.Value = 35
            StatusLabel.Content = "Downloading package..."
            response = Await client.GetAsync(InstallJsonParsed.Link)
            response.EnsureSuccessStatusCode()
            Progress.Value = 50
            Dim zipStream As Stream = Await response.Content.ReadAsStreamAsync()
            Progress.Value = 75
            StatusLabel.Content = "Extracting package..."
            Dim archive = New ZipArchive(zipStream)
            archive.ExtractToDirectory(PluginPath, True)
            Progress.Value = 80
            StatusLabel.Content = "Cleaning up..."
            File.Move(PluginPath + "\Pending\" + Name + ".json", PluginPath + "\Installed\" + Name + ".json", True)
            StatusLabel.Content = "Install Success. Launching..."
            Progress.Value = 100
        Catch ex As Exception
            File.WriteAllText("Error.log", ex.ToString)
            StatusLabel.Content = "Error:" + ex.Message
        End Try
    End Function
End Class
