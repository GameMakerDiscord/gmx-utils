Module Main

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
            Console.WriteLine("Error: " & e.Message & vbCrLf & e.StackTrace)
        End Try
    End Sub

End Module
