Module GMBridge

    Public Class GMResourceId
        '' constants
        Private Const ID_SIZE As Byte = 32
        Private Const CHARS As String = "0123456789abcdef"
        '' variables
        Private Dim resourceId As String
        '' constructors
        Public Sub New()
            Me.Id = GMResourceId.GenerateRandomId()
        End Sub
        Public Sub New(Byval id As String)
            Me.Id = id
        End Sub
        '' methods
        Public Shared Function GenerateRandomId()

        End Function
        Public Shared Function IsValid(Byval Id As String) As Boolean
            Dim anyChar As String = "[" & CHARS & "]"
            Dim mask4b As String = anyChar & anyChar & anyChar & anyChar
            Dim mask8b As String = mask4b & mask4b
            Dim mask12b As String = mask8b & mask4b
            Return Id.ToLower Like (mask8b & "-" & mask4b & "-" & mask4b & "-" & mask4b & "-" & mask12b)
        End Function
        '' properties
        Public Property Id() As String
            Get
                Return resourceId
            End Get
            Set(newId As String)
                If (not IsValid(newId)) Then Throw New InvalidGMResourceIdException("id does not follow specification of " & _ 
                                                                                    "00000000-0000-0000-0000-000000000000, " & _
                                                                                    "where 0s are any hexadecimal number.")
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