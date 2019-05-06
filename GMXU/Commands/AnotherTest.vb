Public Class AnotherTest
    Inherits Command
    '' methods
    Public Overrides Sub Execute(params() As String)
        Console.WriteLine("Got damned")
    End Sub
    Public Overrides Function GetHelp() As String
        Return "Another test command which says a rude word."
    End Function
End Class
