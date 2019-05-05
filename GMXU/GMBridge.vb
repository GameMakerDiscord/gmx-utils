Module GMBridge

    Public Class GMResourceId
        '' constants
        Private Const ID_SIZE As Byte = 32
        Private Const CHARS As String = "0123456789abcdef"
        '' variables
        Private Dim resourceId As String
        '' constructors
        Public Sub New()
            Id = GenerateRandomId()
        End Sub
        Public Sub New(Byval Id As String)

        End Sub
        '' methods
        Public Shared Function GenerateRandomId()

        End Function
        Public Shared Function IsValid(Byval Id As String) As Boolean
            Dim anyChar As String = "[" & CHARS & "]"
            Return Id Like ((anyChar*8) & "-" & (anyChar*4) & "-" & (anyChar*4) & "-" & (anyChar*4) & "-" & (anyChar*12))
        End Function
        '' properties
        Public Property Id() As String
            Get
                '' format 00000000-0000-0000-0000-000000000000
                Return String.Format(
                    "{0}-{1}-{2}-{3}-{4}",
                    resourceId.Substring(0,8),
                    resourceId.Substring(8,4),
                    resourceId.Substring(12,4),
                    resourceId.Substring(16,4),
                    resourceId.Substring(20))
            End Get
            Set(newId As String)
                If (not IsValid(newId)) Then Throw New InvalidGMResourceIdException("id does not follow specification of " & _ 
                                                                                    "00000000-0000-0000-0000-000000000000, " & _
                                                                                    "where 0s are any hexadecimal number")
                resourceId = newId
            End Set
        End Property
    End Class

    Public Class InvalidGMResourceIdException
        Inherits Exception
        '' constructor
        Public Sub New(message As String)
            MyBase.New("Malformed resource Id: " & message)
        End Sub
    End Class

End Module