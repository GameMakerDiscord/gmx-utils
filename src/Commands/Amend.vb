Imports System.IO.StreamReader
Imports System.IO.StreamWriter
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' A command which updates JSDoc help information.
''' </summary>
Public Class Amend
    Inherits Command
    
    ''' <summary>
    ''' <see cref="Command.Execute(String())"/>
    ''' </summary>
    Public Overrides Sub Execute(params() As String)
        If (params.Length < 1) Then Throw New ArgumentException("amend must contain at least one argument <filepath>.")
        Dim extensionPath As String = params(0)
        If (Not My.Computer.FileSystem.FileExists(extensionPath)) Then Throw New ArgumentException("file '" & extensionPath & "' does not exist.")
        '' obtain the json
        Dim jsonObj As String = ""
        Dim jsonInput As StreamReader = My.Computer.FileSystem.OpenTextFileReader(extensionPath)
        While(not jsonInput.EndOfStream)
            jsonObj += jsonInput.ReadLine() & vbCrLf
        End While
        jsonInput.Close()
        jsonInput.Dispose()
        Dim json As JObject = JsonConvert.DeserializeObject(jsonObj)
        '' inject help into json
        If (json.ContainsKey("files")) Then
            For Each record As JObject In json("files")
                If (record.ContainsKey("functions") AndAlso record.ContainsKey("filename")) Then
                    Dim file As String = Path.GetDirectoryName(extensionPath) & "\" & record("filename").Value(Of String)
                    If (My.Computer.FileSystem.FileExists(file)) Then
                        Dim input As StreamReader = My.Computer.FileSystem.OpenTextFileReader(file)
                        Dim parameterTable As Dictionary(Of String, List(Of String)) = New Dictionary(Of String, List(Of String))
                        '' for each line of the source file:
                        '' 1) check whether it is a "#define " token, then
                        '' 2) find its parameters.
                        Dim scriptName As String = ""
                        While (Not input.EndOfStream)
                            Dim line As String = input.ReadLine().TrimStart(" "c)
                            If (line.Length = 0) Then Continue While
                            If (line(0) = "#") Then
                                Dim signature As String() = line.Split(New Char() {" "c}, 2)
                                If ((signature.Length = 2) AndAlso (signature(0) = "#define")) Then
                                    scriptName = signature(1)
                                    parameterTable.Add(scriptName, New List(Of String))
                                    Console.WriteLine("Script: " & scriptName)
                                End If
                            ElseIf (parameterTable.ContainsKey(scriptName))
                                If (line Like "///*") Then
                                    Dim param As String() = line.Remove(0,3).TrimStart(" "c).Split(New Char() {" "c}, 2)
                                    If (param.Length <> 2) Then Continue While
                                    Select Case (param(0).ToLower())
                                        Case "@param", "@parameter", "@arg", "@argument"
                                            parameterTable(scriptName).Add(param(1))
                                            Console.WriteLine(vbTab & "Parameter: " & param(1))
                                    End Select
                                End If
                            End If
                        End While
                        input.Close()
                        input.Dispose()
                        '' map function names to parameter lists
                        For Each script As JObject In record("functions")
                            If (script.ContainsKey("externalName") AndAlso script.ContainsKey("help")) Then
                                Dim externalName As String = script("externalName").Value(Of String)
                                If (parameterTable.ContainsKey(externalName)) Then
                                    Dim helpInfo As String = ""
                                    For Each param As String In parameterTable(externalName)
                                        If (helpInfo <> "") Then helpInfo += ","
                                        helpInfo += param
                                    Next
                                    helpInfo = "(" & helpInfo & ")"
                                    Dim helpFile As JValue = script("help")
                                    helpFile.Value = helpInfo
                                    Console.WriteLine("Updated help file: " & externalName & helpInfo)
                                End If
                            End If
                        Next
                    End If
                End If
            Next
        End If
        '' serialise json back to original file
        Dim jsonOutput As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(extensionPath, False)
        jsonOutput.WriteLine(JsonConvert.SerializeObject(json, Formatting.Indented))
        jsonOutput.Close()
        jsonOutput.Dispose()
    End Sub

    ''' <summary>
    ''' <see cref="Command.GetBrief()"/>
    ''' </summary>
    Public Overrides Function GetBrief() As String
        Return "Updates help information of an extension."
    End Function

    ''' <summary>
    ''' <see cref="Command.GetDescription()"/>
    ''' </summary>
    Public Overrides Function GetDescription() As String
        Return "Iterates through all the defined functions in a supplied .yy extension file and updates the help text of each script such that it corresponds to the JSDoc parameter information." &
            vbCrLf & vbCrLf & vbTab & "<filepath>" & vbCrLf & vbTab & " The filepath of the .yy extension you wish to amend."
    End Function

    ''' <summary>
    ''' <see cref="Command.GetSyntax()"/>
    ''' </summary>
    Public Overrides Function GetSyntax() As String
        Return "<filepath>"
    End Function
End Class
