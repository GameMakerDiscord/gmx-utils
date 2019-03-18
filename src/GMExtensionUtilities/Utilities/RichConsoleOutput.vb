Module RichConsoleOutput

    Public Sub Write(ByVal text As String)
        Dim left As Integer = Console.WindowLeft
        Dim maxWidth As Integer = left+Console.WindowWidth
        Console.Write("[" & maxWidth.ToString & "]")
        For each character As Char In text
            Dim x As Integer = Console.CursorLeft
            Dim y As Integer = Console.CursorTop
            Console.Write(character)
            If(x>=maxWidth)
                Console.CursorTop = y+1
                Console.CursorLeft = 0
            End If
        Next
        Console.WindowLeft = left
    End Sub

End Module