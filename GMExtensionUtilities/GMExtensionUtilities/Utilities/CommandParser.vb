Imports GMExtensionUtilities.TypeExtensions

Module CommandParser

    '' create parser
    Public Class Parser
        
        '' create table property for storing command functions
        Private Shared commandTable As Dictionary(Of String, command) = New Dictionary(Of String, command)
        Public Shared ReadOnly Property commands() As Dictionary(Of String, command)
            Get
                Return commandTable
            End Get
        End Property

        '' create singleton
        Private Shared this As Parser = New Parser()
        Public Shared ReadOnly Property obtain As Parser
            Get
                Return this
            End Get
        End Property
        Private Sub New()
            '' nothing
        End Sub

        '' create a subclass for throwing errors
        Public class InvalidCommandException
            Inherits Exception
        End Class

        '' create public methods
        Public Shared Sub add(ByVal name As String, ByRef cmd As Command)
            '' adds a new command
            commandTable.Add(name,cmd)
        End Sub
        Public Shared Function find(ByVal name As String) As Command
            If(commandTable.ContainsKey(name))
                return commandTable(name)
            End If
            '' command doesnt exist
            Return Nothing
        End Function
        Public Shared Sub decode(Byval str As String)
            '' decodes and executes the supplied command string
            Dim cmdData As String() = str.Split({" "c},2)
            If(cmdData.Length>0)
                If(commandTable.ContainsKey(cmdData(0)))
                    Dim cmd As command = commandTable(cmdData(0))
                    If(cmdData.Length=1)
                        '' execute command with no args
                        cmd.execute("")
                    Else
                        '' execute command with args
                        cmd.execute(cmdData(1))
                    End If
                    Exit Sub
                End If
            End If
            '' throw error
            Throw New InvalidCommandException
        End Sub
        
    End Class

    '' create command class
    Public Class Command

        '' create argument data
        Private arguments As Tuple(Of String, String())() = {}
        Public ReadOnly Property count As Integer
            Get '' gets the argument count
                Return arguments.Length
            End Get
        End Property

        '' create flag delimiter
        Private Const flagToken As Char = "-"

        '' setup description
        Public brief As String = ""
        Public description As String = ""

        '' setup threshold for where arguments become optional
        Public threshold As Integer = -1 ' -1 => all arguments are required

        '' create delegate function class
        Public Delegate Sub functPtr(ByRef arguments As arg())
        Private addressPtr As functPtr

        '' create constructor
        Public Sub New(ByRef address As functPtr)
            addressPtr = address
        End Sub

        '' create a subclass for parsing argument data
        Public Class arg
            
            '' setup properties
            Private _value_ As String
            Private _flags_ As String()
            Public ReadOnly Property value As String
                Get 
                    Return _value_
                End Get
            End Property

            '' create constructor
            Public Sub New(ByVal value As String, ByVal flags As String())
                _value_ = value
                _flags_ = flags
            End Sub
            
            '' create methods
            Public Function containsFlag(ByVal flag As string) As Boolean
                Return _flags_.Contains(flag)
            End Function
            Public Overloads Function ToString() As String
                Return value
            End Function

        End Class

        '' create a subclass for throwing errors
        Public class InvalidSyntaxException
            Inherits Exception
        End Class
            
        '' define private methods
        Private Function argIsOptional(ByVal id As Integer) As Boolean
            Return (threshold>=0)And(id>=threshold)
        End Function

        '' define public methods
        Public Sub add(ByVal argumentName As String, ByVal optionalFlags As String())
            '' adds a new argument
            arguments.Append(New Tuple(Of String, String())(argumentName,optionalFlags))
        End Sub
        Public Sub add(ByVal argumentName As String)
            '' for when no additional flags are supplied
            add(argumentName,{})
        End Sub
        Public Function encode()
            '' convert the command to a string
            Dim syntax As String = ""
            For i As Integer = 0 To (count - 1)
                '' iterate through arguments
                Dim argument As Tuple(Of String, String()) = arguments(i)
                For each additionalFlag As String In argument.Item2
                    '' compile additional flags
                    syntax += "[" & flagToken & additionalFlag & "] "
                Next
                '' add argument name
                Dim argName As String = "<" & argument.Item1 & ">"
                If(argIsOptional(i))
                    argName = "[" & argName & "]"
                End If
                syntax += argName & " "
            Next
            Return syntax
        End Function
        Public Sub execute(ByVal str As String)
            '' parses and executes the supplied command string
            Dim strItems As String() = str.Separate(" "c,""""c) '' separate command string, ignoring any spaces contained in quotes
            Dim cmdArgs As arg() = {}
            Dim cmdFlags As String() = {}
            For each strItem As String In strItems
                If(strItem.Length>0)
                    if(strItem(0)=flagToken)
                        '' add new flag
                        Dim cmdFlag As String = strItem.Remove(0,1) '' remove starting item from flag
                        cmdFlags.Append(cmdFlag)
                    Else
                        '' create a new argument
                        cmdArgs.Append(
                            New arg(
                                strItem.Trim(""""c),
                                cmdFlags
                            )
                        )
                        cmdFlags = {} '' clear previous flags
                    End If
                End If
            Next
            If(threshold>=0)
                If(not argIsOptional(cmdArgs.Length))
                    '' missing a required command; abort
                    Throw New invalidSyntaxException
                    Exit Sub
                End If
            Else If(count<>cmdArgs.Length)
                Throw New invalidSyntaxException
                Exit Sub
            End If
            '' execute
            addressPtr(cmdArgs)
        End Sub

    End Class

End Module