Option Strict Off
Option Explicit On
Imports System.Drawing
Imports System.Reflection
Imports System.Windows.Forms

Public Class Application
    Inherits Windows.Application
    Dim WithEvents Tray As New NotifyIcon
    Public Sub InitializeComponent()
        If Command() = "--service" Then
            MsgBox("Start Service")
            Tray.Icon = New Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("WebModInstaller.drivers.ico"))
            Tray.Text = "Qinlili Mod Installer Service"
            Tray.Visible = True
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
End Class