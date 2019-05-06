Public Class Compile
    Inherits Command
    '' methods
    Public Overrides Sub Execute(params() As String)
        Throw New NotImplementedException()
    End Sub
    Public Overrides Function GetBrief() As String
        Return "Packs .gml scripts into a single file."
    End Function
    Public Overrides Function GetDescription() As String
        Return _
            "Use this command to package all .gml files in a project directory into a single extension file." &
            vbCrLf & vbCrLf & vbTab & "--deep" & vbCrLf & vbTab & " If this is added as an argument, then the application will perform a deep search for files." &
            vbCrLf & vbCrLf & vbTab & "-m <mask>" & vbCrLf & vbTab & " An optional argument used to define the search mask, where only scripts which satisfy the pattern are compiled." &
            vbCrLf & vbCrLf & vbTab & "-d <destination>" & vbCrLf & vbTab & " An optional argument used to define a destination path. Defaults to the call path."
    End Function
    Public Overrides Function GetSyntax() As String
        Return "<directory> [--deep] [-m <mask>] [-d <destination>]"
    End Function
End Class
