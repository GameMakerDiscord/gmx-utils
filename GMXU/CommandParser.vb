Module CommandParser

    Public Class Parser
        '' variables
        Public Shared ReadOnly commands As Dictionary(Of String, Command) = New Dictionary(Of String, Command)
        '' constructor
        #Disable Warning
        Private Sub New()
            '' Has Nothing
        End Sub
        '' methods
        Public Shared Sub Execute(ByRef c As String())
            Execute(c, 0)
        End Sub
        Public Shared Sub Execute(ByRef c As String(), ByVal i As Integer)
            If (c.Length <= i) Then Throw New ArgumentException("Params must be non-empty.")
            Dim name As String = c(i)
            If (Not commands.ContainsKey(name)) Then Throw New ArgumentException("Command " & name & " does not exist.")
            Dim cmd As Command = commands(name)
            Dim paramCount As Integer = c.Length - (i + 1)
            Dim params(paramCount) As String
            Array.Copy(c, i + 1, params, 0, paramCount)
            cmd.Execute(name,params)
        End Sub
    End Class

    Public MustInherit Class Command
        '' methods
        Public Function GetHelp(ByVal verbose As String) As String
            If (Not verbose) Then Return GetHelp()
            Return _
                "Description:" & vbCrLf & vbTab & GetHelp() & vbCrLf &
                "Syntax:" & vbCrLf & vbTab & GetSyntax()
        End Function
        '' interface
        Public MustOverride Function GetHelp() As String
        Public MustOverride Function GetSyntax() As String
        Public MustOverride Function Execute(ByVal name As String, ByVal params As String())
    End Class

End Module
