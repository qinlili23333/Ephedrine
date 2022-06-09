Imports System.IO
Imports System.IO.Compression

Public Class Unzip
    Private ExtractPath As String

    Private Async Sub Unzip_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If File.Exists("QinliliPatch.zip") And File.Exists("InstallLocation") Then
            Using sr As New StreamReader("InstallLocation")
                ExtractPath = Await sr.ReadToEndAsync()
            End Using
            Dim t As Task = Task.Run(Sub()
                                         Dim archive As ZipArchive = ZipFile.OpenRead("QinliliPatch.zip")
                                         archive.ExtractToDirectory(ExtractPath, True)
                                     End Sub)
            t.GetAwaiter().OnCompleted(Sub()
                                           Environment.Exit(1)
                                       End Sub)
        Else
            MsgBox("Installation file not found. Check your antivirus software.",, "Error")
            Environment.Exit(-1)
        End If
    End Sub
End Class
