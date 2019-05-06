Imports GMXU.CommandParser

Module GMExtensionUtils

    Sub Main()
        Try
            Command.Parse(Environment.GetCommandLineArgs(),1)
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub

End Module
