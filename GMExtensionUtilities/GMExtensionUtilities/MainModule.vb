Module MainModule

    Const HEADER_MESSAGE As String = _
        "GameMaker Extension Utilities, by Nuxii @Tat3xd" & vbCrLf

    Sub Main()
        Console.Title = HEADER_MESSAGE
        Dim messageParser As GMXU.Parser = New GMXU.Parser
        Do
            Console.Write(">")
            Dim message As String = Console.ReadLine()
            Console.WriteLine()
            messageParser.execute(message)
            Console.WriteLine()
        Loop
    End Sub

End Module
