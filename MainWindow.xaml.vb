Imports Microsoft.Web.WebView2.Core
Imports System.Text.Json

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
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadAsync()
    End Sub
    Private Async Sub LoadAsync()
        Status.Content = "Loading WebView2..."
        Await MainWeb.EnsureCoreWebView2Async()
        Progress.IsIndeterminate = False
        Progress.Value = 10
        MainWeb.CoreWebView2.Settings.IsStatusBarEnabled = False
        Status.Content = "Loading Web Page..."
        MainWeb.CoreWebView2.Navigate("https://glacier.qinlili.bid")
    End Sub


    Private Sub MainWeb_NavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs) Handles MainWeb.NavigationStarting
        Status.Content = "Loading Web Page..."
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
            Select Case (Message.Action)
                'Action 1
                Case "Install"
                    MsgBox("Pending Install:" + Message.Arg1)
                'Action 3
                Case "Prepare"
                    MsgBox("Pending Install Depending Module:" + Message.Arg1)
                'Action 9
                Case "StartService"
                    'Detect if service died
                    If Service IsNot Nothing Then
                        If Service.HasExited Then
                            Service = Nothing
                        End If
                    End If
                    If Service Is Nothing Then
                        Status.Content = "Starting Service..."
                        Service = Process.Start(Environment.ProcessPath, "--service")
                        If Service IsNot Nothing Then
                            '91 Start Success
                            MainWeb.CoreWebView2.ExecuteScriptAsync("msgStatus(91)")
                        Else
                            MsgBox("Fail to start service. Check if antivirus software stop it.")
                            '90 Start Fail
                            MainWeb.CoreWebView2.ExecuteScriptAsync("msgStatus(90)")
                        End If
                    Else
                        '93 Duplicate Service
                        MainWeb.CoreWebView2.ExecuteScriptAsync("msgStatus(93)")
                    End If

                    Status.Content = "Ready."
                    Progress.IsIndeterminate = False
                    IsBusy = False
            End Select
        Else
            '-1 Busy
            MainWeb.CoreWebView2.ExecuteScriptAsync("msgStatus(-1)")
        End If
    End Sub
End Class
