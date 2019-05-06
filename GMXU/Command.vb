Public MustInherit Class Command
    '' variables
    Private Shared ReadOnly commands As Dictionary(Of String, Command) = New Dictionary(Of String, Command)
    Private name As String
    '' constructor
    Public Sub New()
        name = Me.GetType().Name.ToLower()
        commands.Add(name, Me)
    End Sub
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
        cmd.Execute(params)
    End Sub
    Public Function GetHelp(ByVal verbose As Boolean) As String
        If (Not verbose) Then Return GetHelp()
        Return _
            "Name:" & vbCrLf & vbTab & name & vbCrLf & vbCrLf &
            "Description:" & vbCrLf & vbTab & GetHelp() & vbCrLf & vbCrLf &
            "Syntax:" & vbCrLf & vbTab & name & " " & GetSyntax()
    End Function
    Public Overridable Function GetHelp() As String
        Return ""
    End Function
    Public Overridable Function GetSyntax() As String
        Return ""
    End Function
    '' interface
    Public MustOverride Sub Execute(ByVal params As String())
End Class