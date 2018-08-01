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
    <Extension()>Private Function split2(ByRef str As string, ByVal separator As char(), ByVal clip As Char) As String()
        return str.split2(separator,clip,clip)
    End Function
    <Extension()>Private Function split2(ByRef str As string, ByVal separator As char(), ByVal clipBegin As Char, ByVal clipEnd As Char) As String()
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
                If(clipBegin<>clipEnd)
                    If(currentCharacter=clipBegin)
                        '' increment stack
                        clipStack += 1
                    ElseIf(currentCharacter=clipEnd)
                        '' decrement stack
                        clipStack -= 1
                    End If               
                Else If(currentCharacter=clipBegin)
                    clipStack -= 1  '  0 -  1 = -1,  1 -  1 = 0,
                    clipStack *= -1 ' -1 * -1 =  1,  0 * -1 = 0, => 0 becomes 1 and 1 becomes 0
                End If
            End If
        Next
        '' add final segment
        If(currentSegment<>"") then outputArray.Add(currentSegment)
        Return outputArray
    End Function
    <Extension()>Private Function replaceAt(ByRef str As string, ByVal pos As Integer, ByVal newChar As Char) As String
        Dim l As Integer = str.Length
        If((pos<0)Or(pos>=l)) Then Return str
        If(pos=(l-1)) Then Return str.Substring(0,pos) & newChar
        return str.Substring(0,pos) & newChar & str.Substring(pos+1,l-pos-1)
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
                "Iterates through all the scripts within the *.yy extension file, then updates the help text of each such that it corresponds to the parameters defined in the external *.gml files."
            )
            newCommand( '' extracts macro information and adds them as separate constants
                "exmacros",AddressOf cmdCreateMacros,
                "<filepath>",
                "Iterates through all the source files within the *.yy extension file, and extracts any #macro tokens contained and inserts them into their own constant section in the extension."
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

        '' declare subclasses
        Private Class GamemakerResourceID
            '' create class for manipulating gamemaker IDs
            Public Sub New()
                ID = "00000000000000000000000000000000"
            End Sub
            Private Const id_length As Byte = 32
            Private id_raw As String

            '' define properties
            Public Property ID() As String
                Get
                    '' Format: 00000000-0000-0000-0000-000000000000
                    Return _
                        id_raw.Substring(0,8) & "-" & _
                        id_raw.Substring(8,4) & "-" & _
                        id_raw.Substring(12,4) & "-" & _
                        id_raw.Substring(16,4) & "-" & _
                        id_raw.Substring(20,12)
                End Get
                Set(ByVal GMID As String)
                    '' remove dashes from string
                    GMID = GMID.Replace("-"c,"")
                    If(GMID.Length=id_length)
                        id_raw = GMID.ToLower
                    End If
                End Set
            End Property

            '' define methods
            Public Sub increment()
                '' increment id by count
                Dim currentCharIndex As Integer = 0
                Do
                    Dim currentChar As Char = id_raw(currentCharIndex)
                    Dim nextChar As Char
                    select(currentChar)
                        Case "0": nextChar = "1"
                        Case "1": nextChar = "2"
                        Case "2": nextChar = "3"
                        Case "3": nextChar = "4"
                        Case "4": nextChar = "5"
                        Case "5": nextChar = "6"
                        Case "6": nextChar = "7"
                        Case "7": nextChar = "8"
                        Case "8": nextChar = "9"
                        Case "9": nextChar = "a"
                        Case "a": nextChar = "b"
                        Case "b": nextChar = "c"
                        Case "c": nextChar = "d"
                        Case "d": nextChar = "e"
                        Case "e": nextChar = "f"
                        Case Else: nextChar = "0"
                    End Select
                    '' replace current character
                    id_raw = id_raw.replaceAt(currentCharIndex,nextChar)
                    If(currentChar="f")
                        '' go to the next character because of an overflow
                        currentCharIndex += 1
                    Else
                        Exit Do
                    End If
                Loop
            End Sub
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
                    showMessage("Complete!", True)
                    showMessage("Destination file created at path: " & destinationFile, False)
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
                    '' store current directory
                    Dim extensionDirectory As String = Path.GetDirectoryName(extensionFile)
                    '' decode file
                    showMessage("Reading extension Json", True)
                    Dim jsonStr As String = ""
                    Dim jsonInputStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(extensionFile)
                    While(not jsonInputStream.EndOfStream)
                        jsonStr += jsonInputStream.ReadLine() & vbCrLf
                    End While
                    jsonInputStream.Close()
                    jsonInputStream.Dispose() ' dispose of dynamic resources
                    '' decode json
                    showMessage("Deserialising Json", True)
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
                                                    showMessage("New Script: " & gmScriptName, False,messageType.successful)
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
                                                                    gmScriptParameterDictionary(gmScriptName).Add(gmParam)
                                                                    showMessage("New Parameter: " & gmParam, False,messageType.successful)
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
                                                    showMessage("Updated help file for '" & gmScriptExternalName & "': " & gmScriptExternalName & gmScriptHelp, False,messageType.successful)
                                                End If
                                            Else
                                                showMessage("ExternalName '" & gmScriptExternalName & "' is not defined", True,messageType.problem)
                                            End If
                                        Else
                                            showMessage("Function does not contain an 'externalName' key!", True,messageType.problem)
                                        End If
                                    Next
                                Else 
                                    showMessage("File does not exist at filepath: " & gmScriptFilepath, False,messageType.problem)
                                End If
                            Else
                                showMessage("Item in 'files' does not contain a 'functions' or 'filepath' key!", True,messageType.problem)
                            End If
                        Next
                    Else
                        showMessage("Extension file does not contain 'files' key!", True,messageType.problem)
                    End If
                    '' encode json again
                    showMessage("Serialising Json", True)
                    jsonStr = JsonConvert.SerializeObject(json,Formatting.Indented)
                    '' write json to original extension file                    
                    Dim jsonOutputStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(extensionFile,false)
                    jsonOutputStream.WriteLine(jsonStr)
                    jsonOutputStream.Close()
                    jsonOutputStream.Dispose() ' dispose of dynamic resources
                    showMessage("Complete!", True)
                Else
                    showMessage("Invalid Extension File!", False, messageType.problem)
                End If
            Else
                Throw New cmdInvalidSyntaxException
            End If
        End Sub
        Private Sub cmdCreateMacros(ByRef args As String())
            Dim myId As GamemakerResourceID = New GamemakerResourceID
            Dim argCount As Integer = args.Length
            If(argCount=1)
                Dim extensionFile As String = args(0).Trim("""")
                If(My.Computer.FileSystem.FileExists(extensionFile))
                    '' declare delegate function for getting a valid macro id
                    Dim gmMacroGetValidID = Function(ByVal macroArray As JArray) As GamemakerResourceID
                                                Dim macroID As GamemakerResourceID = new GamemakerResourceID()
                                                '' iterate through all current macros and correct for any collisions
                                                For each macroRecord As JObject In macroArray
                                                    If(macroRecord.ContainsKey("id"))
                                                        If(macroRecord.GetValue("id")=macroID.ID)
                                                            '' id collision, increment macroID
                                                            macroID.increment()
                                                        End If
                                                    End If
                                                Next
                                                '' return final macro id
                                                Return macroID
                                            End Function
                    '' store current directory
                    Dim extensionDirectory As String = Path.GetDirectoryName(extensionFile)
                    '' decode file
                    showMessage("Reading extension Json", True)
                    Dim jsonStr As String = ""
                    Dim jsonInputStream As StreamReader = My.Computer.FileSystem.OpenTextFileReader(extensionFile)
                    While(not jsonInputStream.EndOfStream)
                        jsonStr += jsonInputStream.ReadLine() & vbCrLf
                    End While
                    jsonInputStream.Close()
                    jsonInputStream.Dispose() ' dispose of dynamic resources
                    '' decode json
                    showMessage("Deserialising Json", True)
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
                                                        Dim gmMacroItems As String() = gmSourceToken(2).split2({" "c},""""c)
                                                        Do ' preparation for multi-line macros
                                                            For each gmMacroItem As String In gmMacroItems
                                                                gmMacroValue += gmMacroItem & " "
                                                            Next
                                                            Exit Do
                                                        Loop
                                                        '' construct new macro element
                                                        Dim gmMacroRecord As JObject = New JObject()
                                                        gmMacroRecord.Add("id",gmMacroGetValidID(gmConstants).ID)
                                                        gmMacroRecord.Add("modelName","GMExtensionConstant")
                                                        gmMacroRecord.Add("mvc","1.0")
                                                        gmMacroRecord.Add("constantName",gmMacroName)
                                                        gmMacroRecord.Add("hidden",false)
                                                        gmMacroRecord.Add("value",gmMacroValue)
                                                        '' add macro to the list of constants
                                                        gmConstants.Add(gmMacroRecord)
                                                        showMessage("New Macro: " & gmMacroName,false,messageType.successful)
                                                        showMessage(" " & gmMacroValue,false)
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
                                    showMessage("File does not exist at filepath: " & gmScriptFilepath, True,messageType.problem)
                                End If
                            Else
                                showMessage("Item in 'files' does not contain a 'constants' or 'filepath' key!", True,messageType.problem)
                            End If
                        Next
                    Else
                        showMessage("Extension file does not contain 'files' key!", True,messageType.problem)
                    End If
                    '' encode json again
                    showMessage("Serialising Json", True)
                    jsonStr = JsonConvert.SerializeObject(json,Formatting.Indented)
                    '' write json to original extension file                    
                    Dim jsonOutputStream As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(extensionFile,false)
                    jsonOutputStream.WriteLine(jsonStr)
                    jsonOutputStream.Close()
                    jsonOutputStream.Dispose() ' dispose of dynamic resources
                    showMessage("Complete!", True)
                Else
                    showMessage("Invalid Extension File!", False, messageType.problem)
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