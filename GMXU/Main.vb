Module Main

    Sub Main()
        Try
            Dim args As String() = {"a","test"}'Environment.GetCommandLineArgs()
            Command.Parse(args,1)
        Catch e As Exception
            Console.WriteLine("Error: " & e.Message)
        End Try
    End Sub

End Module
