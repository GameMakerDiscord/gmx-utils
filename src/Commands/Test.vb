Public Class Test
    Inherits Command
    '' methods
    Public Overrides Sub Execute(ByVal params As String())
        Console.WriteLine("Hello!")
    End Sub
    Public Overrides Function GetBrief() As String
        Return "A test command which says hi!"
    End Function
    Public Overrides Function GetDescription() As String
        Return GetBrief()
    End Function
    Public Overrides Function GetSyntax() As String
        Return ""
    End Function
End Class
