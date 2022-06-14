Imports System.IO
Imports System.Net.Http
Imports System.Text.Json

Public Class PluginManage
    ReadOnly PluginPath As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\Ephedrine\Plugins"
    Class PluginJson
        Public Property Name As String
        Public Property Files As String()
        Public Property Dependent As String()
        Public Property Version As String
        Public Property Link As String
        Public Property Source As String
    End Class
    Class JsonCheckBox
        Inherits CheckBox
        Public Property Current As PluginJson
        Public Property Cloud As PluginJson
    End Class
    Dim Checks As New ArrayList()
    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
    End Sub

    Private Sub PluginManage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not Directory.Exists(PluginPath) Then
            MsgBox("You have not used any plugin.",, "Ephedrine Plugins")
            Close()
        End If
        InitializeList()
    End Sub

    Private Async Sub InitializeList()
        Dim InstalledFiles As New DirectoryInfo(PluginPath)
        Dim InstalledJsons As New DirectoryInfo(PluginPath + "\Installed")
        Dim aryJson As FileInfo() = InstalledJsons.GetFiles("*.json")
        For Each Fi In aryJson
            Dim JsonText = Await File.ReadAllTextAsync(Fi.FullName)
            Dim JsonParsed = JsonSerializer.Deserialize(Of PluginJson)(JsonText)
            Dim check = New JsonCheckBox()
            Dim size As Long = 0
            For Each Program In JsonParsed.Files
                size += InstalledFiles.GetFiles(Program)(0).Length
            Next
            check.Content = JsonParsed.Name + "(" + FormatBytes(size) + ")"
            InstallList.Children.Add(check)
            check.Current = JsonParsed
            CheckUpdate(JsonParsed, check)
            Checks.Add(check)
        Next
    End Sub

    Private Async Sub CheckUpdate(jsonParsed As PluginJson, checkBox As JsonCheckBox)
        Dim client As New HttpClient
        Try
            Dim response As HttpResponseMessage = Await client.GetAsync("https://cdn.cloudflare.ephedrine.qinlili.bid/Plugin/" + jsonParsed.Name + "/Install.json")
            response.EnsureSuccessStatusCode()
            Dim responseBody As String = Await response.Content.ReadAsStringAsync()
            Dim InstallJsonParsed = JsonSerializer.Deserialize(Of PluginJson)(responseBody)
            checkBox.Cloud = InstallJsonParsed
            If jsonParsed.Version = InstallJsonParsed.Version Then
                checkBox.Content += " Already Latest:" + InstallJsonParsed.Version
                checkBox.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF44DD20"))
            Else
                checkBox.Content += " Can Update:" + jsonParsed.Version + "->" + InstallJsonParsed.Version
                checkBox.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FFDD4420"))
            End If
        Catch ex As Exception
            File.WriteAllText("Error.log", ex.ToString)
            checkBox.Content += " Fail To Update"
            checkBox.Cloud = jsonParsed
        End Try
    End Sub


    Dim DoubleBytes As Double
    Public Function FormatBytes(BytesCaller As ULong) As String
        Try
            Select Case BytesCaller
                Case Is >= 1099511627776
                    DoubleBytes = BytesCaller / 1099511627776 'TB
                    Return FormatNumber(DoubleBytes, 2) & " TB"
                Case 1073741824 To 1099511627775
                    DoubleBytes = BytesCaller / 1073741824 'GB
                    Return FormatNumber(DoubleBytes, 2) & " GB"
                Case 1048576 To 1073741823
                    DoubleBytes = BytesCaller / 1048576 'MB
                    Return FormatNumber(DoubleBytes, 2) & " MB"
                Case 1024 To 1048575
                    DoubleBytes = BytesCaller / 1024 'KB
                    Return FormatNumber(DoubleBytes, 2) & " KB"
                Case 0 To 1023
                    DoubleBytes = BytesCaller ' bytes
                    Return FormatNumber(DoubleBytes, 2) & " bytes"
                Case Else
                    Return ""
            End Select
        Catch
            Return ""
        End Try
    End Function

    Private Sub Uninstall_Click(sender As Object, e As RoutedEventArgs) Handles Uninstall.Click
        For Each Check In Checks.ToArray()
            If Check.IsChecked Then
                If MsgBox("Do you really want to uninstall `" + Check.Current.Name + "`?", MsgBoxStyle.YesNo, "Ephedrine Plugins") Then
                    For Each FileName In Check.Current.Files
                        File.Delete(PluginPath + "\" + FileName)
                    Next
                    File.Delete(PluginPath + "\Installed\" + Check.Current.Name + ".json")
                End If
            End If
        Next
        Dim window As New PluginManage()
        window.Show()
        Close()
    End Sub

    Private Sub Update_Click(sender As Object, e As RoutedEventArgs) Handles Update.Click
        For Each Check In Checks.ToArray()
            If Check.IsChecked And (Not Check.Current.Version = Check.Cloud.Version Or ForceReinstall.IsChecked) Then
                Dim window As New Plugin(Check.Current.Name, "Update to " + Check.Cloud.Version, Nothing, True, True)
                window.Show()
            End If
        Next
    End Sub
End Class
