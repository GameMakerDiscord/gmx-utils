Module GMExtensionUtils

    Sub Main()
        Dim id As GMResourceId = New GMResourceId
        id.Randomise
        Console.WriteLine(id)
        id.Increment
        Console.WriteLine(id)
        id.AddValue(-1)
        Console.WriteLine(id)
        Console.ReadLine
    End Sub

End Module
