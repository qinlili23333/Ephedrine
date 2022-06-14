Imports System.IO

Public Class PluginManage
    Dim PluginPath As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\Ephedrine\Plugins"
    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        MsgBox("This function is in development. Do not use it if you are not a developer.",, "Ephedrine Plugins")
    End Sub

    Private Sub PluginManage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not Directory.Exists(PluginPath) Then
            MsgBox("You have not used plugins. Why are you here?",, "Ephedrine Plugins")
            Close()
        End If
    End Sub
End Class
