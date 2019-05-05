Module CommandParser

    Public Class Parser
        '' variables
        Public Shared ReadOnly commands As Dictionary(Of String, Command) = New Dictionary(Of String, Command)
        '' constructor
        #Disable Warning
        Private Sub New()
            '' Has Nothing
        End Sub
        '' methods

    End Class

    Public MustInherit Class Command
        '' methods
        Public Function GetHelp(Byval verbose As String) As String
            If (Not verbose) Then Return GetHelp()
            Return _
                "Description:" & vbCrLf & vbTab & GetHelp() & vbCrLf &
                "Syntax:" & vbCrLf & vbTab & GetSyntax()
        End Function
        '' interface
        Public MustOverride Function GetHelp() As String
        Public MustOverride Function GetSyntax() As String
        Public MustOverride Function Execute()
    End Class

End Module
