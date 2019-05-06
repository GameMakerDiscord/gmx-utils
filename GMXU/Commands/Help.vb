Public Class Help
    Inherits Command
    '' Methods
    Public Overrides Sub Execute(ByVal params As String())
        If (params.Length = 0) Then
            '' output all possible commands
            Console.WriteLine("Use 'help <command>' for help with a specific command:" & vbCrLf)
            For Each name As String In Command.GetNames()
                Dim cmd As Command = Command.GetCommand(name)
                If (cmd IsNot Nothing) Then
                    Console.WriteLine(vbTab & name & " : " & cmd.GetBrief())
                End If
            Next
        Else
            Dim name As String = params(0)
            Dim cmd As Command = Command.GetCommand(name)
            If (cmd Is Nothing) Then Throw New ArgumentException("Command '" & name & "' does not exist")
            Console.WriteLine(name & " " & cmd.GetSyntax() & vbCrLf & vbCrLf & vbTab & cmd.GetDescription())
        End If
    End Sub
    Public Overrides Function GetBrief() As String
        Return "Displays help information."
    End Function
    Public Overrides Function GetDescription() As String
        Return "Use this command to to display help information about specific commands."
    End Function
    Public Overrides Function GetSyntax() As String
        Return "<command>"
    End Function
End Class
