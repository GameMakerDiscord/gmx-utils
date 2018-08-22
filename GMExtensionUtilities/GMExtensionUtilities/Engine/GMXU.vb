Imports GMExtensionUtilities.TypeExtensions
Imports GMExtensionUtilities.CommandParser
Imports GMExtensionUtilities.GameMakerBridge
Imports System.IO.StreamReader
Imports System.IO.StreamWriter
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module GMXU

    '' create title text
    Const HEADER_MESSAGE As String = "GameMaker Extension Utilities (c) Nuxii @Tat3xd"
    Const VERSION As String = "v1.1.0"
    Const INDENT As String = "  "

    '' initialise gameloop
    Private Sub gameLoop()
        displayMessage("Package Version: " & VERSION)
        displayNewLine()
        displayMessage("Type ""help"" to display a list of commands.")
        Do
            displayNewLine(2)
            displayMessage("> ")
            Dim userInput As String = getUserInput()
            Try
                Parser.decode(userInput)
            Catch ex As Exception
                If(TypeOf ex Is Parser.InvalidCommandException)
                    displayMessage("Command not found")
                End If
                If(TypeOf ex Is Command.InvalidSyntaxException)
                    displayMessage("Invalid command syntax: See 'help -s <command>' for help with syntax")
                End If
            End Try
        Loop
    End Sub

    '' add command information
    Private Sub initCommands()
        Dim cmd As Command
        '' help
        cmd = New Command(AddressOf cmdHelp) With {
            .threshold = 0,
            .brief = "Displays help information",
            .description = "Use this command to to display help information about the tool and specific commands. 'help -s <command>' and 'help -d <command>' display syntax and description data about the supplied command respectively."
        }
        cmd.add("command",{"s","d"})
        Parser.add("help",cmd)
        '' compile
        cmd = New Command(AddressOf cmdCompile) With {
            .threshold = 1,
            .brief = "Packs *.gml scripts into a single file",
            .description = "Compiles all the *.gml script files within the supplied directory into a single *.gml file. 'compile <directory>' Alone will output the result to a new file located above the supplied directory."
        }
        cmd.add("directory")
        cmd.add("destination",{"d"})
        cmd.add("mask",{"m"})
        Parser.add("compile",cmd)
        '' amend
        cmd = New Command(AddressOf cmdFillHelpRecords) With {
            .threshold = 1,
            .brief = "Fills help information automatically",
            .description = "Iterates through all the defined functions in a supplied *.yy extension file and updates the help text of each such that it corresponds to the JSDoc parameter information."
        }
        cmd.add("filepath")
        Parser.add("amend",cmd)
        '' extract macros
        cmd = New Command(AddressOf cmdCreateMacros) With {
            .threshold = 1,
            .brief = "Extracts and applies macro information",
            .description = "Iterates through all the source files in a supplied *.yy extension file and extracts any #macro tokens contained, then inserts those definitions into the files 'constant' section."
        }
        cmd.add("filepath")
        Parser.add("exmacros",cmd)
        '' obfuscate
        cmd = New Command(AddressOf cmdObfuscateScripts) With {
            .threshold = 1,
            .brief = "Obfuscates script display names",
            .description = "Iterates through all the source files in a supplied *.yy extension file and obfuscates any scripts so they become uninteligible from within Game Maker."
        }
        cmd.add("filepath",{"I"})
        cmd.add("mask",{"m"})
        Parser.add("obfuscate",cmd)
    End Sub

    '' create procedures
    Private Sub displayMessage(ByVal message As String)
        Console.Write(message)
    End Sub
    Private Sub displayNewLine(ByVal count As Integer)
        For i As Integer = 1 To count
            Console.WriteLine()
        Next
    End Sub
    Private Sub displayNewLine()
        displayNewLine(1)
    End Sub
    Private Function getUserInput() As String
        Return Console.ReadLine()
    End Function
    
    '' create commands
    Private Sub cmdHelp(ByRef arguments As Command.arg())
        If(arguments.Length=0)
            '' display a list of commands
            displayMessage("These are common commands you can use:")
            displayNewLine(2)
            For each record As KeyValuePair(Of String,Command) In Parser.commands
                '' write command name and brief description
                Dim cmdName As String = record.Key
                Dim cmdData As Command = record.Value
                displayMessage(INDENT & cmdName & New String(" "c,10-cmdName.Length) & cmdData.brief)
                displayNewLine()
            Next
            displayNewLine()
            displayMessage("Use 'help <command>' for a description and syntax for a specific command.")
        Else
            '' display syntax and description data
            Dim argument1 as Command.arg = arguments(0)
            Dim commandName As String = argument1.value
            Dim helpCommand As Command = Parser.find(commandName)
            If(helpCommand IsNot Nothing)
                Dim displaySyntax As Boolean = argument1.containsFlag("s")
                Dim displayDescription As Boolean = argument1.containsFlag("d")
                Dim displayBrief As Boolean = True
                If(displaySyntax Xor displayDescription)
                    '' disable brief
                    displayBrief = False
                Else
                    '' set both flags to true (having no flags means syntax and description will be printed)
                    displaySyntax = True
                    displayDescription = True
                End If
                Dim newLine As Boolean = False
                If(displayBrief)
                    displayMessage(commandName & " - " & helpCommand.brief)
                    newLine = True
                End If
                If(displaySyntax)
                    If(newLine) Then displayNewLine(2)
                    displayMessage("Syntax: " & commandName & " " & helpCommand.encode())
                End If
                If(displayDescription)
                    If(newLine) Then displayNewLine(2)
                    displayMessage("Description:")
                    displayNewLine()
                    displayMessage(helpCommand.description)
                End If
            Else
                displayMessage("No help found for command '" & commandName & "'")
            End If
        End If
    End Sub
    Private Sub cmdCompile(ByRef arguments As Command.arg())
        Dim sourceDirectory As String = arguments(0).value
        Dim destinationFile As String = sourceDirectory & ".gml"
        Dim searchMask As String = "*"
        If(arguments.Length>1)
            For Each arg In arguments
                If(arg IsNot arguments(0))
                    If(arg.containsFlag("d"))
                        destinationFile = arg.value
                    Else If(arg.containsFlag("m"))
                        searchMask = arg.value
                    End If
                End If
            Next
        End If
        '' decode
        If(Directory.Exists(sourceDirectory))
            '' create a queue for storing sub directories
            Dim directoryQueue As Queue(Of String) = New Queue(Of String)
            '' enqueue top directory
            directoryQueue.Enqueue(sourceDirectory)
            '' open destination file and compile script data
            Dim outputFileStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(destinationFile,false)
            outputFileStream.WriteLine("THIS FILE WAS AUTOMATICALLY GENERATED")
            displayMessage("Compiling sub directories:")
            displayNewLine()
            While(directoryQueue.Count>0)
                Dim fileDirectory As String = directoryQueue.Dequeue()
                displayNewLine()
                displayMessage(INDENT & "New Directory: '" & fileDirectory & "'")
                For Each subDirectory As String In Directory.GetDirectories(fileDirectory,searchMask)
                    '' enqueue all sub directories
                    directoryQueue.Enqueue(subDirectory)
                Next
                For each gmlFile As String In Directory.GetFiles(fileDirectory,searchMask & ".gml")
                    '' decode file
                    displayNewLine()
                    displayMessage(New String(INDENT,2) & "New File: '" & gmlFile & "'")
                    Dim inputFileStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(gmlFile)
                    '' compile script data
                    Dim scriptToken As String = "#define " & Path.GetFileNameWithoutExtension(gmlFile)
                    outputFileStream.WriteLine("") ' write line before compiling new script
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
            displayNewLine(2)
            displayMessage("Destination file created at path: '" & destinationFile & "'")
        Else
            displayMessage("Invalid Directory")
        End If
    End Sub
    Private Sub cmdFillHelpRecords(ByRef arguments As Command.arg())
        Dim extensionFile As String = arguments(0).value
        If(My.Computer.FileSystem.FileExists(extensionFile))
            '' store current directory
            Dim extensionDirectory As String = Path.GetDirectoryName(extensionFile)
            '' decode file
            displayMessage("Reading extension Json")
            Dim jsonStr As String = ""
            Dim jsonInputStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(extensionFile)
            While(not jsonInputStream.EndOfStream)
                jsonStr += jsonInputStream.ReadLine() & vbCrLf
            End While
            jsonInputStream.Close()
            jsonInputStream.Dispose() ' dispose of dynamic resources
            '' decode json
            displayNewLine()
            displayMessage("Deserialising Json")
            displayNewLine()
            Dim json As JObject = JsonConvert.DeserializeObject(jsonStr)
            '' parse extension file
            If(json.ContainsKey("files"))
                For each gmFile As JObject In json.GetValue("files")
                    '' iterate through files
                    If(gmFile.ContainsKey("functions") And gmFile.ContainsKey("filename"))
                        '' get the file path of the current source file
                        Dim gmScriptFilepath As String = extensionDirectory & "\" & gmFile.GetValue("filename").Value(Of String)
                        If(My.Computer.FileSystem.FileExists(gmScriptFilepath))
                            '' use the filepath to compile the parameters for each script, delimited by "#define"
                            Dim gmInputStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(gmScriptFilepath)
                            Dim gmScriptParameterDictionary As Dictionary(Of String, String()) = New Dictionary(Of String, String())
                            '' For each line of the source file, check whether the current line is a token, and then find its parameters.
                            Dim gmScriptNamePrevious As String = ""
                            Dim gmScriptName As String = "" ' vbNullString could be used, but I want to protect invalid tokens from being parsed
                            While (not gmInputStream.EndOfStream)
                                Dim gmSourceLine As String = gmInputStream.ReadLine().TrimStart(" "c)
                                '' parse token
                                If(gmSourceLine.Length>1)
                                    If(gmSourceLine(0)="#")
                                        Dim gmSourceToken As String() = gmSourceLine.Split(" "c)
                                        if(gmSourceToken.Length>1)
                                            If(gmSourceToken(0)="#define")
                                                '' update name
                                                gmScriptName = gmSourceToken(1)
                                            End If
                                        End If
                                    End If
                                    '' get parameters
                                    If(gmScriptName<>"")
                                        If(gmScriptNamePrevious<>gmScriptName)
                                            '' create a new parameter array and add reference to dictionary
                                            gmScriptParameterDictionary.Add(gmScriptName,{})
                                            displayNewLine()
                                            displayMessage(INDENT & "New Script: " & gmScriptName)
                                        End If
                                        If(gmScriptParameterDictionary.ContainsKey(gmScriptName))
                                            '' if we are inside a script body, look for parameters for that script
                                            If(gmSourceLine.length>3)
                                                If(gmSourceLine.Substring(0,3)="///")
                                                    If(gmSourceLine(3)=" "c)
                                                        '' remove space from between /// and @param
                                                        gmSourceLine = gmSourceLine.Remove(3,1)
                                                    End If
                                                    Dim gmParameterTokens As String() = gmSourceLine.Split(" "c)
                                                    if(gmParameterTokens.Length>1)
                                                        If(gmParameterTokens(0)="///@param")
                                                            '' add new parameter
                                                            Dim gmParam As String = gmParameterTokens(1)
                                                            gmScriptParameterDictionary(gmScriptName).Append(gmParam)
                                                            displayNewLine()
                                                            displayMessage(New String(INDENT,2) & "New Parameter: " & gmParam)
                                                        End If
                                                    End If
                                                End If
                                            End IF
                                        End If
                                    End If
                                End If
                                '' update previous
                                gmScriptNamePrevious = gmScriptName
                            End While
                            gmInputStream.Close()
                            gmInputStream.Dispose() ' dispose of dynamic resources
                            For each gmFunction As JObject In gmFile.GetValue("functions")
                                '' iterate through functions (scripts)
                                If(gmFunction.ContainsKey("externalName"))
                                    '' get the external name of the script within the source file
                                    Dim gmScriptExternalName as String = gmFunction.GetValue("externalName").Value(Of String)
                                    '' get parameters using external script name
                                    If(gmScriptParameterDictionary.ContainsKey(gmScriptExternalName))
                                        Dim gmScriptHelp As String = ""
                                        For Each gmParam As String In gmScriptParameterDictionary(gmScriptExternalName)
                                            If(gmScriptHelp<>"")
                                                '' add a comma between arguments
                                                gmScriptHelp += ","
                                            End If
                                            gmScriptHelp += gmParam
                                        Next
                                        gmScriptHelp = "(" & gmScriptHelp & ")"
                                        '' update help
                                        If(gmFunction.ContainsKey("help"))
                                            Dim gmHelpFile As JValue = gmFunction.GetValue("help")
                                            gmHelpFile.Value = gmScriptHelp
                                            displayNewLine()
                                            displayMessage("Updated help file for '" & gmScriptExternalName & "': " & gmScriptExternalName & gmScriptHelp)
                                        End If
                                    Else
                                        displayNewLine()
                                        displayMessage("ExternalName '" & gmScriptExternalName & "' is not defined")
                                    End If
                                Else
                                    displayNewLine()
                                    displayMessage("Function does not contain an 'externalName' key")
                                End If
                            Next
                        Else 
                            displayNewLine()
                            displayMessage("File does not exist at filepath: '" & gmScriptFilepath & "'")
                        End If
                    Else
                        displayNewLine()
                        displayMessage("Item in 'files' does not contain a 'functions' or 'filepath' key!")
                    End If
                Next
            Else
                displayNewLine()
                displayMessage("Extension file does not contain 'files' key")
            End If
            '' encode json again
            displayNewLine(2)
            displayMessage("Serialising Json")
            jsonStr = JsonConvert.SerializeObject(json,Formatting.Indented)
            '' write json to original extension file                    
            Dim jsonOutputStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(extensionFile,false)
            jsonOutputStream.WriteLine(jsonStr)
            jsonOutputStream.Close()
            jsonOutputStream.Dispose() ' dispose of dynamic resources
            displayNewLine()
            displayMessage("Complete")
        Else
            displayMessage("Invalid Extension File")
        End If
    End Sub
    Private Sub cmdCreateMacros(ByRef arguments As Command.arg())
        Dim extensionFile As String = arguments(0).value
        If(My.Computer.FileSystem.FileExists(extensionFile))
            '' store current directory
            Dim extensionDirectory As String = Path.GetDirectoryName(extensionFile)
            '' decode file
            displayMessage("Reading extension Json")
            Dim jsonStr As String = ""
            Dim jsonInputStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(extensionFile)
            While(not jsonInputStream.EndOfStream)
                jsonStr += jsonInputStream.ReadLine() & vbCrLf
            End While
            jsonInputStream.Close()
            jsonInputStream.Dispose() ' dispose of dynamic resources
            '' decode json
            displayNewLine()
            displayMessage("Deserialising Json")
            displayNewLine()
            Dim json As JObject = JsonConvert.DeserializeObject(jsonStr)
            '' parse extension file
            If(json.ContainsKey("files"))
                For each gmFile As JObject In json.GetValue("files")
                    '' iterate through files
                    If(gmFile.ContainsKey("constants") And gmFile.ContainsKey("filename"))
                        Dim gmConstants As JArray = gmFile.GetValue("constants")
                        '' remove all previous constants from the array
                        gmConstants.Clear()
                        '' get the file path of the current source file
                        Dim gmScriptFilepath As String = extensionDirectory & "\" & gmFile.GetValue("filename").Value(Of String)
                        If(My.Computer.FileSystem.FileExists(gmScriptFilepath))
                            '' use the filepath to compile the macro names and parameters "#macro" token
                            Dim gmInputStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(gmScriptFilepath)
                            Dim gmFileText As String = ""
                            While (not gmInputStream.EndOfStream)
                                Dim gmSourceLineRaw As String = gmInputStream.ReadLine()
                                Dim gmSourceLine As String = gmSourceLineRaw.TrimStart(" "c)
                                '' parse token
                                If(gmSourceLine.Length>1)
                                    If(gmSourceLine(0)="#")
                                        Dim gmSourceToken As String() = gmSourceLine.Split({" "c},3)
                                        if(gmSourceToken.Length>2)
                                            '' format: #macro <macroName> <macroData[]>
                                            If(gmSourceToken(0)="#macro")
                                                '' decode macro
                                                Dim gmMacroName As String = gmSourceToken(1)
                                                Dim gmMacroValue As String = ""
                                                Dim gmMacroItems As String() = gmSourceToken(2).Separate(" "c,""""c)
                                                Do ' preparation for multi-line macros
                                                    For each gmMacroItem As String In gmMacroItems
                                                        gmMacroValue += gmMacroItem & " "
                                                    Next
                                                    Exit Do
                                                Loop
                                                '' construct new macro element
                                                Dim gmMacroRecord As JObject = New JObject()
                                                Dim gmMacroID As GamemakerResourceID = New GamemakerResourceID
                                                gmMacroID.randomize() ' randomise id so there its highly unlikely for a collisio to occur
                                                gmMacroRecord.Add("id",gmMacroID.ID)
                                                gmMacroRecord.Add("modelName","GMExtensionConstant")
                                                gmMacroRecord.Add("mvc","1.0")
                                                gmMacroRecord.Add("constantName",gmMacroName)
                                                gmMacroRecord.Add("hidden",false)
                                                gmMacroRecord.Add("value",gmMacroValue)
                                                '' add macro to the list of constants
                                                gmConstants.Add(gmMacroRecord)
                                                displayNewLine()
                                                displayMessage(INDENT & "New Macro: " & gmMacroName & " : " & gmMacroValue)
                                                Continue While
                                            End If
                                        End If
                                    End If
                                End If
                                gmFileText += gmSourceLineRaw & vbCrLf ' add source line to file text
                            End While
                            gmInputStream.Close()
                            gmInputStream.Dispose() ' dispose of dynamic resources
                            '' update the source file with no macro data
                            Dim gmOutputStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(gmScriptFilepath,False)
                            gmOutputStream.WriteLine(gmFileText)
                            gmOutputStream.Close()
                            gmOutputStream.Dispose() ' dispose of dynamic resources
                        Else 
                            displayNewLine()
                            displayMessage("File does not exist at filepath: '" & gmScriptFilepath & "'")
                        End If
                    Else
                        displayNewLine()
                        displayMessage("Item in 'files' does not contain a 'constants' or 'filepath' key")
                    End If
                Next
            Else
                displayNewLine()
                displayMessage("Extension file does not contain 'files' key")
            End If
            '' encode json again
            displayNewLine(2)
            displayMessage("Serialising Json")
            jsonStr = JsonConvert.SerializeObject(json,Formatting.Indented)
            '' write json to original extension file                    
            Dim jsonOutputStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(extensionFile,false)
            jsonOutputStream.WriteLine(jsonStr)
            jsonOutputStream.Close()
            jsonOutputStream.Dispose() ' dispose of dynamic resources
            displayNewLine()
            displayMessage("Complete!")
        Else
            displayMessage("Invalid Extension File")
        End If
    End Sub
    Private Sub cmdObfuscateScripts(ByRef arguments As Command.arg())
        Dim extensionFile As String = arguments(0).value
        If(My.Computer.FileSystem.FileExists(extensionFile))
            '' store current directory
            Dim extensionDirectory As String = Path.GetDirectoryName(extensionFile)
            '' get whether to reverse the obfuscation
            Dim reverseObfuscation As Boolean = False
            If(arguments(0).containsFlag("I"))
                reverseObfuscation = True
            End If
            '' get the optional search mask
            Dim searchMask As String = "*"
            If(arguments.Length>1)
                Dim arg As Command.arg = arguments(1)
                If(arg.containsFlag("m"))
                    searchMask = arg.value
                End If
            End If
            '' decode file
            displayMessage("Reading extension Json")
            Dim jsonStr As String = ""
            Dim jsonInputStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(extensionFile)
            While(not jsonInputStream.EndOfStream)
                jsonStr += jsonInputStream.ReadLine() & vbCrLf
            End While
            jsonInputStream.Close()
            jsonInputStream.Dispose() ' dispose of dynamic resources
            '' decode json
            displayNewLine()
            displayMessage("Deserialising Json")
            displayNewLine()
            Dim json As JObject = JsonConvert.DeserializeObject(jsonStr)
            '' parse extension file
            If(json.ContainsKey("files"))
                For each gmFile As JObject In json.GetValue("files")
                    '' iterate through files
                    If(gmFile.ContainsKey("functions"))
                        For each gmFunction As JObject in gmFile.GetValue("functions")
                            If(gmFunction.ContainsKey("id") And gmFunction.ContainsKey("externalName") And gmFunction.ContainsKey("name"))
                                Dim gmScriptId As String = gmFunction.GetValue("id").Value(Of String)
                                Dim gmScriptName As JValue = gmFunction.GetValue("externalName")
                                Dim gmScriptDisplayName As JValue = gmFunction.GetValue("name")
                                If(GamemakerResourceID.isValid(gmScriptId) And (gmScriptName.Value Like searchMask))
                                    If (reverseObfuscation)
                                        gmScriptDisplayName.Value = gmScriptName.Value
                                    Else
                                        gmScriptDisplayName.Value = "Z5CR1P7_" & gmScriptId.Replace("-","").ToUpper
                                    End If
                                    displayNewLine()
                                    displayMessage("Updated display name for script '" & gmScriptName.Value & "' (" & gmScriptId & ")")
                                End If
                            Else
                                displayNewLine()
                                displayMessage("Item in 'functions' does not contain an 'id', 'externalName', or 'name' key")
                            End If
                        Next
                    Else
                        displayNewLine()
                        displayMessage("Item in 'files' does not contain a 'functions' key")
                    End If
                Next
            Else
                displayNewLine()
                displayMessage("Extension file does not contain 'files' key")
            End If
            '' encode json again
            displayNewLine(2)
            displayMessage("Serialising Json")
            jsonStr = JsonConvert.SerializeObject(json,Formatting.Indented)
            '' write json to original extension file                    
            Dim jsonOutputStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(extensionFile,false)
            jsonOutputStream.WriteLine(jsonStr)
            jsonOutputStream.Close()
            jsonOutputStream.Dispose() ' dispose of dynamic resources
            displayNewLine()
            displayMessage("Complete!")
        Else
            displayMessage("Invalid Extension File")
        End If
    End Sub

    '' init
    Sub Main()
        '' randomise engine
        Randomize()
        '' update title
        Console.Title = HEADER_MESSAGE
        '' update console buffer
        Console.SetWindowPosition(0,0)
        Console.SetBufferSize(Console.LargestWindowWidth,Console.LargestWindowHeight)
        '' initialise commands
        initCommands()
        '' enter gameloop
        gameLoop()
    End Sub

End Module
