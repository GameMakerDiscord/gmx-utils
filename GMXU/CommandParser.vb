Module CommandParser

    Public MustInherit Class Command
        '' variables
        Protected Shared ReadOnly commands As Dictionary(Of String, Command) = New Dictionary(Of String, Command)
        '' methods
        Public Shared Sub Parse(ByRef c As String())
            Parse(c, 0)
        End Sub
        Public Shared Sub Parse(ByRef c As String(), ByVal i As Integer)
            If (c.Length <= i) Then Throw New ArgumentException("Params must be non-empty.")
            Dim name As String = c(i)
            If (Not commands.ContainsKey(name)) Then Throw New ArgumentException("Command '" & name & "' does not exist.")
            Dim cmd As Command = commands(name)
            Dim paramCount As Integer = c.Length - (i + 1)
            Dim params(paramCount) As String
            Array.Copy(c, i + 1, params, 0, paramCount)
            cmd.Execute(name,params)
        End Sub
        Public Function GetHelp(ByVal verbose As Boolean) As String
            If (Not verbose) Then Return GetHelp()
            Dim names As String = ""
            For Each name In GetNames()
                names += ", " & name
            Next
            if (names <> "") Then names = names.Remove(0,2)
            Return _
                "Name(s):" & vbCrLf & vbTab & names & vbCrLf & vbCrLf &
                "Description:" & vbCrLf & vbTab & GetHelp() & vbCrLf & vbCrLf &
                "Syntax:" & vbCrLf & vbTab & GetSyntax()
        End Function
        Public Function GetNames() As String()
            Dim names As List(Of String) = New List(Of String)
            For Each name In commands.Keys
                If (commands(name) Is Me) Then names.Add(name)
            Next
            Return names.ToArray()
        End Function
        '' interface
        Public MustOverride Function GetHelp() As String
        Public MustOverride Function GetSyntax() As String
        Public MustOverride Function Execute(ByVal name As String, ByVal params As String())
    End Class

End Module
