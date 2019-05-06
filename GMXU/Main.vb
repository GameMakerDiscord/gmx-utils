Module Main

    Sub Main()
        Dim commandList As List(Of Command) = New list(Of Command)
        Dim commandType As Type = GetType(Command)
        For Each type As Type In commandType.Assembly.GetTypes()
            If (type.IsSubclassOf(commandType)) Then
                commandList.Add(Activator.CreateInstance(type))
            End If
        Next
        Dim commands As Command() = commandList.ToArray()
        Try
            Dim args As String() = {"a", "anothertest"} 'Environment.GetCommandLineArgs()
            Command.Parse(args,1)
        Catch e As Exception
            Console.WriteLine("Error: " & e.Message)
        End Try
        Console.ReadKey()
    End Sub

End Module
