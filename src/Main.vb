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
            Console.WriteLine()
            Command.Parse(Environment.GetCommandLineArgs(),1)
        Catch e As Exception
            Console.ForegroundColor = ConsoleColor.DarkRed
            Console.Write("ERROR")
            Console.ResetColor()
            Console.WriteLine(": " & e.Message)
        End Try
    End Sub
End Module
