Imports System.Text.RegularExpressions
Imports System.IO.StreamReader
Imports System.IO.StreamWriter
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' A command which extracts macro data from .gml files and inserts them into their corresponding .yy extension file.
''' </summary>
Public Class ExMacros
    Inherits Command
    
    ''' <summary>
    ''' <see cref="Command.Execute(String())"/>
    ''' </summary>
    Public Overrides Sub Execute(params() As String)
        If (params.Length < 1) Then Throw New ArgumentException("exmacros must contain at least one argument <filepath>.")
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
                If (record.ContainsKey("constants") AndAlso record.ContainsKey("filename")) Then
                    Dim file As String = Path.GetDirectoryName(extensionPath) & "\" & record("filename").Value(Of String)
                    If (My.Computer.FileSystem.FileExists(file)) Then
                        Dim input As StreamReader = My.Computer.FileSystem.OpenTextFileReader(file)
                        Dim constants As JArray = record("constants")
                        Dim sterilisedScript As String = ""
                        constants.Clear()
                        While (Not input.EndOfStream)
                            Dim line As String = input.ReadLine().TrimStart(" "c)
                            If ((line.Length <> 0) AndAlso (line(0) = "#")) Then
                                Dim signature As String() = line.Split(New Char() {" "c}, 3)
                                If ((signature.Length > 0) AndAlso (signature(0) = "#macro")) Then
                                    If (signature.Length = 3) Then
                                        Dim name As String = signature(1)
                                        Dim value As String = signature(2)
                                        Console.WriteLine("Macro: " & name & " " & value)
                                        '' construct new macro element
                                        Dim macro As JObject = New JObject()
                                        Dim macroId As GMResourceId = New GMResourceId()
                                        macroID.Randomise()
                                        macro.Add("id",macroId.Id)
                                        macro.Add("modelName","GMExtensionConstant")
                                        macro.Add("mvc","1.0")
                                        macro.Add("constantName",name)
                                        macro.Add("hidden",false)
                                        macro.Add("value",value)
                                        constants.Add(macro)
                                    End If
                                    Continue While
                                End If
                            End If
                            sterilisedScript += line & vbCrLf
                        End While
                        input.Close()
                        input.Dispose()
                        '' update source file with macros removed
                        Dim output As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(file, False)
                        output.WriteLine(sterilisedScript)
                        output.Close()
                        output.Dispose()
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
        Return "Extracts and inserts macro information."
    End Function

    ''' <summary>
    ''' <see cref="Command.GetDescription()"/>
    ''' </summary>
    Public Overrides Function GetDescription() As String
        Return "Iterates through all the defined functions in a supplied .yy extension file and searches for any #macro definitions. It will then automatically insert those into the macro definitions of the extension." &
            vbCrLf & vbCrLf & vbTab & "<filepath>" & vbCrLf & vbTab & " The filepath of the .yy extension you wish to update the macros of."
    End Function

    ''' <summary>
    ''' <see cref="Command.GetSyntax()"/>
    ''' </summary>
    Public Overrides Function GetSyntax() As String
        Return "<filepath>"
    End Function
End Class