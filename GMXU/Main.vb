Module Main
    '' main
    Sub Main()
        Dim cmdType As Type = GetType(Command)
        For Each type As Type In cmdType.Assembly.GetTypes()
            If (type.IsSubclassOf(cmdType)) Then
                Activator.CreateInstance(type)
            End If
        Next
        Try
            Command.Parse(Environment.GetCommandLineArgs(),1)
        Catch e As Exception
            Console.WriteLine("Error: " & e.Message)
        End Try
        Console.ReadKey
    End Sub
End Module
