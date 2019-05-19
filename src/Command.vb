''' <summary>
''' A class used to collect and parse commands easily.
''' </summary>
Public MustInherit Class Command
    
    Private Shared ReadOnly commands As Dictionary(Of String, Command) = New Dictionary(Of String, Command)
    Private name As String

    ''' <summary>
    ''' Constructs a new <code>Command</code> and adds it to the <code>Dictionary</code> of available commands.
    ''' </summary>
    ''' <remarks>
    ''' Uses reflection to find the name of the inheriting class and uses that to automatically fill in the name record of this <code>Command</code>.
    ''' </remarks>
    Public Sub New()
        name = Me.GetType().Name.ToLower()
        commands.Add(name, Me)
    End Sub

    ''' <summary>
    ''' Parses an array of arguments.
    ''' </summary>
    ''' <param name="args">An array of command arguments.</param>
    ''' <exception cref="ArgumentException"><see cref="Parse(ByRef String(), Integer)"/></exception>
    Public Shared Sub Parse(ByRef args As String())
        Parse(args, 0)
    End Sub

    ''' <summary>
    ''' Parses an array of arguments beyond a certain threshold index <paramref name="i"/>.
    ''' </summary>
    ''' <param name="args">An array of command arguments.</param>
    ''' <param name="i">A threshold index, before which arguments of <paramref name="args"/> are ignored.</param>
    ''' <exception cref="ArgumentException">Thrown when <paramref name="args"/> is empty or a command does not exist.</exception>
    Public Shared Sub Parse(ByRef args As String(), ByVal i As Integer)
        If (args.Length <= i) Then Throw New ArgumentException("Params must be non-empty.")
        Dim name As String = args(i)
        If (Not commands.ContainsKey(name)) Then Throw New ArgumentException("Command '" & name & "' does not exist.")
        Dim params As List(Of String) = New List(Of String)
        For p As Integer = (i + 1) To (args.Length - 1)
            params.Add(args(p))
        Next
        commands(name).Execute(params.ToArray())
    End Sub

    ''' <summary>
    ''' Gets command names.
    ''' </summary>
    ''' <returns>An array of command names.</returns>
    Public Shared Function GetNames() As String()
        Return commands.Keys.ToArray()
    End Function

    ''' <summary>
    ''' Finds a specific command with a given name.
    ''' </summary>
    ''' <param name="name">The name of the command.</param>
    ''' <returns>A <c>Command</c> object with the name <paramref name="name"/>, or <c>Nothing</c> if no command with that name exists.</returns>
    Public Shared Function GetCommand(ByVal name As String) As Command
        If ((name = "") Or (Not commands.ContainsKey(name))) Then Return Nothing
        Return commands(name)
    End Function

    ''' <summary>
    ''' Gets the parameters of a command argument <paramref name="token"/>.
    ''' </summary>
    ''' <remarks>
    ''' Command arguments are delimitered by the token <c>-</c>.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim args As String() = Command.GetArgs({"-a", "foo", "false", "-k", "-d"}, "a")
    ''' For arg In args
    '''     Console.WriteLine(arg)
    ''' Next
    ''' ' Displays:
    ''' '   foo
    ''' '   false
    ''' </code>
    ''' </example>
    ''' <param name="params">An array of command arguments.</param>
    ''' <param name="token">A commandline argument to find the parameters of.</param>
    ''' <returns>An array of parameters.</returns>
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

    ''' <summary>
    ''' Gets the parameter of a command argument <paramref name="token"/>.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Command.GetArgs(ByRef String(), String)"/>
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim arg As String = Command.GetArgs({"-a", "foo", "false", "-k", "-d"}, "a")
    ''' Console.WriteLine(arg)
    ''' ' Displays:
    ''' '   foo false
    ''' </code>
    ''' </example>
    ''' <param name="params">An array of command arguments.</param>
    ''' <param name="token">A commandline argument to find the parameter of.</param>
    ''' <returns>A single parameter.</returns>
    Public Shared Function GetArg(ByRef params As String(), ByVal token As String, ByVal [default] As String) As String
        Dim args As String() = Command.GetArgs(params, token)
        Dim arg As String = ""
        For Each item In args
            arg += " " & item
        Next
        If (arg <> "") Then Return arg.Substring(1) Else Return [default]
    End Function
    
    ''' <summary>
    ''' Calls the command.
    ''' </summary>
    ''' <param name="params">An array of command arguments.</param>
    Public MustOverride Sub Execute(ByVal params As String())

    ''' <summary>
    ''' Gets a brief explaination of the <c>Command</c>.
    ''' </summary>
    ''' <returns>A brief explaination.</returns>
    Public MustOverride Function GetBrief() As String

    ''' <summary>
    ''' Gets a full description of the <c>Command</c>.
    ''' </summary>
    ''' <returns>A description.</returns>
    Public MustOverride Function GetDescription() As String

    ''' <summary>
    ''' Gets the argument syntax for this <c>Command</c>.
    ''' </summary>
    ''' <returns>A syntax description.</returns>
    Public MustOverride Function GetSyntax() As String
End Class