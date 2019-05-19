Imports System.IO.StreamReader
Imports System.IO.StreamWriter
Imports System.IO

''' <summary>
''' A command which compiles .gml scripts into a single file for easy distribution.
''' </summary>
Public Class Compile
    Inherits Command
    
    ''' <summary>
    ''' <see cref="Command.Execute(String())"/>
    ''' </summary>
    Public Overrides Sub Execute(params() As String)
        If (params.Length < 1) Then Throw New ArgumentException("execute must contain at least one argument <directory>.")
        Dim deep As Boolean = params.Contains("--deep")
        Dim append As Boolean = params.Contains("--append")
        Dim mask As String = Command.GetArg(params,"m","*")
        Dim source As String = params(0)
        Dim dest As String = Command.GetArg(params,"d",source & ".gml")
        '' compile scripts
        If (Not Directory.Exists(source)) Then Throw New ArgumentException("<directory> is not a valid path.")
        Dim searchQueue As Queue(Of String) = New Queue(Of String)
        searchQueue.Enqueue(source)
        Dim output As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(dest, append)
        output.WriteLine("// THIS FILE WAS AUTOMATICALLY GENERATED USING https://github.com/GameMakerDiscord/GMExtensionUtilities BY @Kat3Nuxii")
        While (searchQueue.Count > 0)
            Dim dir As String = searchQueue.Dequeue()
            If (deep) Then
                '' obtain deep directories
                For Each subDir As String In Directory.GetDirectories(dir)
                    Console.WriteLine("Directory: " & Path.GetFileName(subDir))
                    searchQueue.Enqueue(subDir)
                Next
            End If
            For Each file As String In Directory.GetFiles(dir, mask & ".gml")
                Console.WriteLine(vbTab & "File: " & Path.GetFileName(file))
                '' scan the file first to find any occurences of "#define ", if this is the case then we dont need a header
                Dim scanner As StreamReader = My.Computer.FileSystem.OpenTextFileReader(file)
                While(not scanner.EndOfStream)
                    Dim line As String = scanner.ReadLine()
                    If (line.Contains("#define ")) Then GoTo Translate
                End While
                Dim header As String = "#define " & Path.GetFileNameWithoutExtension(file)
                output.WriteLine()
                output.WriteLine(header)
                Translate:
                scanner.Close()
                scanner.Dispose()
                Dim input As StreamReader = My.Computer.FileSystem.OpenTextFileReader(file)
                While(not input.EndOfStream)
                    Dim line As String = input.ReadLine()
                    output.WriteLine(line)
                End While
                input.Close()
                input.Dispose()
            Next
        End While
        output.Close()
        output.Dispose()
        Console.WriteLine("File compiled!")
    End Sub

    ''' <summary>
    ''' <see cref="Command.GetBrief()"/>
    ''' </summary>
    Public Overrides Function GetBrief() As String
        Return "Packs .gml scripts into a single file."
    End Function

    ''' <summary>
    ''' <see cref="Command.GetDescription()"/>
    ''' </summary>
    Public Overrides Function GetDescription() As String
        Return _
            "Use this command to package all .gml files in a project directory into a single extension file." &
            vbCrLf & vbCrLf & vbTab & "<directory>" & vbCrLf & vbTab & " The target directory containing the scripts you wish to compile." &
            vbCrLf & vbCrLf & vbTab & "--deep" & vbCrLf & vbTab & " If this is added as an argument, then the application will perform a deep search for files." &
            vbCrLf & vbCrLf & vbTab & "-m <mask>" & vbCrLf & vbTab & " An optional argument used to define the search mask, where only scripts which satisfy the pattern are compiled." &
            vbCrLf & vbCrLf & vbTab & "-d <destination>" & vbCrLf & vbTab & " An optional argument used to define a destination path. Defaults to one directory above the source directory."
    End Function

    ''' <summary>
    ''' <see cref="Command.GetSyntax()"/>
    ''' </summary>
    Public Overrides Function GetSyntax() As String
        Return "<directory> [--deep] [--append] [-m <mask>] [-d <destination>]"
    End Function
End Class
