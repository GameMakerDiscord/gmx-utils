Imports System.IO.StreamReader
Imports System.IO.StreamWriter
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module GMXU
    
    '' extensions for arrays
    <Extension()>Private Sub Add(Of T)(ByRef arr As T(), item As T)
        Array.Resize(arr, arr.Length + 1)
        arr(arr.Length - 1) = item
    End Sub

    '' extensions for strings
    <Extension()>Private Function split2(ByRef str As string, separator As char(), ByVal clip As Char) As String()
        return str.split2(separator,clip,clip)
    End Function
    <Extension()>Private Function split2(ByRef str As string, separator As char(), ByVal clipBegin As Char, ByVal clipEnd As Char) As String()
        '' split a string, ignoring any delimiters
        Dim outputArray As String() = {}
        Dim currentSegment As String = ""
        Dim clipStack As Integer = 0
        For Each currentCharacter As Char In str
            If((clipStack=0) And separator.Contains(currentCharacter))
                '' current character is a delimiter, add current segment to array
                If(currentSegment<>"") then outputArray.Add(currentSegment)
                currentSegment = ""
            Else
                '' normal char, add to the current segment
                currentSegment += currentCharacter
                If(currentCharacter=clipBegin)
                    '' increment stack
                    clipStack += 1
                ElseIf(currentCharacter=clipEnd)
                    '' decrement stack
                    clipStack -= 1
                End If               
            End If
        Next
        '' add final segment
        If(currentSegment<>"") then outputArray.Add(currentSegment)
        Return outputArray
    End Function

    Public Class Parser
        '' create class for parsing 
        Public Sub new()
            newCommand( '' displays help
                "help",AddressOf cmdHelp,
                "<commandName>",
                "Displays the help information for a specified command... Hey, wait a second."
            )
            newCommand( '' compiles scripts in a folder path
                "compile",AddressOf cmdCompile,
                "<directory> [destination]",
                "Compiles all the *.gml script files within the supplied directory into a single *.gml file. Destination is an optional argument if you already have a file you want to write to, otherwise the result will be written to a new file located outside the supplied directory."
            )
            newCommand( '' fills the help files with argument information
                "amend",AddressOf cmdFillHelpFiles,
                "<filepath>",
                "Iterates through all the scripts within the *.yy extension file, then updates the help text of each such that it corresponds to the parameters defined in the external *.gml files"
            )
        End Sub

        '' declare delegate function ptr
        Private Delegate Sub cmdPtr(ByRef args As String())
        Private cmdTable As Dictionary(Of String, cmdPtr) = New Dictionary(Of String, cmdPtr)
        Private cmdTableSyntax As Dictionary(Of String, String) = New Dictionary(Of String, String)
        Private cmdTableDesc As Dictionary(Of String, String) = New Dictionary(Of String, String)
        Private sub newCommand(ByVal name As String, ByRef address As cmdPtr, ByVal syntax As String, ByVal description As string)
            cmdTable.Add(name,address)
            cmdTableSyntax.Add(name,syntax)
            cmdTableDesc.Add(name,description)
        End sub

        '' decalre exception
        Private class cmdInvalidSyntaxException
            Inherits Exception
        End Class

        '' declare commands
        Private Sub cmdHelp(ByRef args As String())
            If(args.Length=0)
                '' display list of commands
                showMessage("The following commands can be used to perform actions:",True)
                for each cmdData As KeyValuePair(Of String, cmdPtr) In cmdTable
                    showMessage(" - " & cmdData.Key,True,messageType.successful)
                Next
                showMessage("For help with a specific command type 'help <commandName>'",True)
            Else If(args.Length=1)
                '' display help for a single command
                Dim cmdName As String = args(0)
                If(cmdTable.ContainsKey(cmdName))
                    If(cmdTableSyntax.ContainsKey(cmdName))
                        showMessage("Syntax:",True,messageType.problem)
                        showMessage(cmdName & " " & cmdTableSyntax(cmdName),True)
                    End If
                    If(cmdTableDesc.ContainsKey(cmdName))
                        showMessage("Description:",True,messageType.problem)
                        showMessage(cmdTableDesc(cmdName),True)
                    End If
                Else
                    showMessage("Command """ & cmdName & """ does not exist",True)
                End If
            Else
                Throw New cmdInvalidSyntaxException
            End If
        End Sub
        Private Sub cmdCompile(ByRef args As String())
            Dim argCount As Integer = args.Length
            If((argCount=1)Or(argCount=2))
                Dim sourceDirectory As String = args(0).Trim("""")
                If(Directory.Exists(sourceDirectory))
                    Dim destinationFile As String
                    If(argCount=1)
                        '' write to current directory
                        destinationFile = sourceDirectory & ".gml"
                    Else
                        destinationFile = args(1).Trim("""")
                    End If
                    '' create a queue for storing file sub directories
                    Dim directoryQueue As Queue(Of String) = New Queue(Of String)()
                    '' enqueue top directory
                    directoryQueue.Enqueue(sourceDirectory)
                    '' open destination file and compile data
                    Dim outputFileStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(destinationFile,false)
                    outputFileStream.WriteLine("THIS FILE WAS AUTOMATICALLY GENERATED")
                    showMessage("Compiling sub directories:", False)
                    While(directoryQueue.Count>0)
                        Dim fileDirectory As String = directoryQueue.Dequeue()
                        showMessage("New Directory: " & fileDirectory, False, messageType.successful)
                        For Each subDirectory As String In Directory.GetDirectories(fileDirectory)
                            '' enqueue all sub directories
                            directoryQueue.Enqueue(subDirectory)
                        Next
                        For each gmlFile As String In Directory.GetFiles(fileDirectory,"*.gml")
                            '' decode file
                            showMessage("Decoding File: " & gmlFile, False, messageType.successful)
                            Dim inputFileStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(gmlFile)
                            '' compile script data
                            Dim scriptToken As String = "#define " & Path.GetFileNameWithoutExtension(gmlFile)
                            outputFileStream.WriteLine(scriptToken)
                            While(not inputFileStream.EndOfStream)
                                Dim scriptLine As String = inputFileStream.ReadLine()
                                outputFileStream.WriteLine(scriptLine)
                            End While
                            '' close file
                            inputFileStream.Close()
                            inputFileStream.Dispose() ' dispose of dynamic resources
                        Next
                    End While
                    outputFileStream.Close() ' submit changes
                    outputFileStream.Dispose() ' dispose of dynamic resources
                Else
                    showMessage("Invalid Directory!", False, messageType.problem)
                End If
            Else
                Throw New cmdInvalidSyntaxException
            End If
        End Sub
        Private Sub cmdFillHelpFiles(ByRef args As String())
            Dim argCount As Integer = args.Length
            If(argCount=1)
                Dim extensionFile As String = args(0).Trim("""")
                If(My.Computer.FileSystem.FileExists(extensionFile))
                    '' set indentation style
                    
                    '' decode file
                    Dim jsonStr As String = ""
                    Dim jsonInputStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(extensionFile)
                    While(not jsonInputStream.EndOfStream)
                        jsonStr += jsonInputStream.ReadLine() & vbCrLf
                    End While
                    jsonInputStream.Close()
                    jsonInputStream.Dispose() ' dispose of dynamic resources
                    '' decode json
                    Dim json As JObject = JsonConvert.DeserializeObject(jsonStr)
                    '' parse extension file
                    If(json.ContainsKey("files"))
                        For each gmFile As JObject In json.GetValue("files")
                            '' iterate through files
                            If(gmFile.ContainsKey("functions") And gmFile.ContainsKey("filename"))
                                '' get the file path of the current source file
                                Dim gmScriptFilepath As String = gmFile.GetValue("filename").Value(Of String)
                                '' use the filepath to compile the parameters for each script, delimited by "#define"
                                
                                '/ TBA /'

                                For each gmFunction As JObject In gmFile.GetValue("functions")
                                    '' iterate through functions (scripts)
                                    If(gmFunction.ContainsKey("externalName"))
                                        '' get the external name of the script within the source file
                                        Dim gmScriptName as String = gmFunction.GetValue("externalName").Value(Of String)
                                        '' find parameters from source file

                                        '/ TBA /'

                                        '' update help
                                        If(gmFunction.ContainsKey("help"))
                                            Dim gmHelpFile As JValue = gmFunction.GetValue("help")
                                            gmHelpFile.Value = "testing 1 2 3"
                                        End If
                                    End If
                                Next
                            End If
                        Next
                    End If
                    '' encode json again
                    jsonStr = JsonConvert.SerializeObject(json,Formatting.Indented)
                    '' write json to original extension file                    
                    Dim jsonOutputStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(extensionFile,false)
                    jsonOutputStream.WriteLine(jsonStr)
                    jsonOutputStream.Close()
                    jsonOutputStream.Dispose() ' dispose of dynamic resources
                End If
            Else
                Throw New cmdInvalidSyntaxException
            End If
        End Sub

        '' debug output
        Private enum messageType
            problem
            neutral
            successful
        End enum
        Private Sub showMessage(ByVal message As String, Byval wrap As Boolean, Optional ByVal type As messageType = messageType.neutral)
        #Const CONSOLE_OUTPUT = true
        #If CONSOLE_OUTPUT then
            Dim defaultConsoleForeground As ConsoleColor = Console.ForegroundColor
            Dim defaultConsoleBackground As ConsoleColor = Console.BackgroundColor
            Select(type)
                Case messageType.problem:
                    Console.ForegroundColor = ConsoleColor.Red
                Case messageType.neutral:
                    Console.ForegroundColor = ConsoleColor.Gray
                Case messageType.successful:
                    Console.ForegroundColor = ConsoleColor.DarkGreen
                Case Else:
                    Console.ForegroundColor = ConsoleColor.DarkGray
                    Console.BackgroundColor = ConsoleColor.Gray
            End Select
            If(wrap)
                '' declare lambda for rich text formatting
                Dim richTextWrap = Function(ByVal txt As String) As String
                                        Dim maxCharsPerLine As Integer = Console.BufferWidth-1
                                        Dim sb As New System.Text.StringBuilder()
                                        Dim line As String = String.Empty
                                        While txt.Length > maxCharsPerLine
                                            line = txt.Substring(0, maxCharsPerLine)
                                            For i As Integer = line.Length - 1 To 1 Step -1
                                                If line(i) <> Chr(32) Then
                                                    line = line.Substring(0, i)
                                                Else
                                                    Exit For
                                                End If
                                            Next
                                            sb.AppendLine(line)
                                            txt = txt.Substring(line.Length)
                                        End While
                                        sb.Append(txt)
                                        Return sb.ToString
                                   End Function
                Dim richTextLines As String() = message.Split(vbCrLf)
                For Each line In richTextLines
                    Console.Write(richTextWrap(line) & vbCrLf)
                Next
            Else
                Console.Write(message & vbCrLf)
            End If
            Console.ForegroundColor = defaultConsoleForeground
            Console.BackgroundColor = defaultConsoleBackground
        #end if  
        End Sub

        '' define methods
        Public Sub execute(ByVal cmd As String)
            Dim cmdData As String() = cmd.Split({" "c},2) '' separate cmd into an array of length 2
            Dim cmdName As String = cmdData(0).ToLower
            Dim cmdArgs As String()
            If(cmdData.Length<2)
                '' no commands
                cmdArgs = {}
            Else
                '' split available commands
                cmdArgs = cmdData(1).split2({" "c},""""c) '' trim arguments, ignoring any delimiters inside quotation marks
            End If
            '' get command
            If(cmdTable.ContainsKey(cmdName))
                '' execute command
                Dim ƒ As cmdPtr = cmdTable(cmdName)
                Try
                    Call ƒ(cmdArgs)
                Catch ex As cmdInvalidSyntaxException
                    '' invalid syntax
                    Dim correctSyntax As String = ""
                    if(cmdTableSyntax.ContainsKey(cmdName)) Then correctSyntax = cmdTableSyntax(cmdName)
                    showMessage("Invalid command syntax, expecting: " & cmdName & " " & correctSyntax,True,messageType.problem)
                End Try
            Else
                '' command is invalid
                showMessage("Unknown command """ & cmdName & """:",True)
                showMessage(" - Type 'help' for a list of commands",True)
            End If
        End Sub

    End Class

End Module