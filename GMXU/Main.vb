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
            Command.Parse(Environment.GetCommandLineArgs(),1)
        Catch e As Exception
            Console.WriteLine("Error: " & e.Message & vbCrLf & e.StackTrace)
        End Try
    End Sub

End Module
