Option Strict Off
Option Explicit On
Imports System.Drawing
Imports System.Reflection
Imports System.Windows.Forms

Public Class Application
    Inherits Windows.Application
    Dim WithEvents Tray As New NotifyIcon
    Dim WithEvents MainMenu As New ContextMenuStrip
    Dim WithEvents mnuExit As New ToolStripMenuItem("Exit")

    Public Sub InitializeComponent()
        If Command() = "--service" Then
            MsgBox("Start Service")
            Tray.Icon = New Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("WebModInstaller.drivers.ico"))
            Tray.Text = "Qinlili Mod Installer Service"
            Tray.ContextMenuStrip = MainMenu
            MainMenu.Items.AddRange(New ToolStripItem() {mnuExit})
            Tray.Visible = True
        ElseIf Command() = "--unzip" Then
            StartupUri = New System.Uri("Unzip.xaml", System.UriKind.Relative)
        Else
            StartupUri = New System.Uri("MainWindow.xaml", System.UriKind.Relative)
        End If
    End Sub
    Public Shared Sub Main()
        'MsgBox(String.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames().ToArray()))
        'MsgBox(Environment.ProcessPath)
        Dim app As New Application()
        app.InitializeComponent()
        app.Run()
    End Sub

    Private Sub mnuExit_Click(sender As Object, e As EventArgs) Handles mnuExit.Click
        End
    End Sub
End Class