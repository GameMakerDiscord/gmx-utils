﻿Public Class Test
    Inherits Command
    '' methods
    Public Overrides Sub Execute(params() As String)
        Console.WriteLine("Hello!")
    End Sub
    Public Overrides Function GetHelp() As String
        Return "A test command which says hi!"
    End Function
End Class
