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
        Dim params As List(Of String) = New List(Of String)
        For p As Integer = (i + 1) To (c.Length - 1)
            params.Add(c(p))
        Next
        commands(name).Execute(params.ToArray())
    End Sub
    Public Shared Function GetNames() As String()
        Return commands.Keys.ToArray()
    End Function
    Public Shared Function GetCommand(ByVal name As String) As Command
        If ((name = "") Or (Not commands.ContainsKey(name))) Then Return Nothing
        Return commands(name)
    End Function
    Public Shared Function GetArgs(ByRef params As String(), ByVal token As String) As String()
        Dim args As List(Of String) = New List(Of String)
        If (params.Contains("-" & token)) Then
            For i As Integer = (Array.IndexOf(params, token) + 1) To (params.Length - 1)
                Dim arg As String = params(i)
                If (arg.Length = 0) Then Continue For
                If (arg(0) = "-") Then Exit For
                args.Add(arg)
            Next
        End If
        Return args.ToArray()
    End Function
    Public Shared Function GetArg(ByRef params As String(), ByVal token As String, ByVal [default] As String) As String
        Dim args As String() = Command.GetArgs(params, token)
        Dim arg As String = ""
        For Each item In args
            arg += " " & item
        Next
        If (arg <> "") Then Return arg.Substring(1) Else Return [default]
    End Function
    '' interface
    Public MustOverride Sub Execute(ByVal params As String())
    Public MustOverride Function GetBrief() As String
    Public MustOverride Function GetDescription() As String
    Public MustOverride Function GetSyntax() As String
End Class