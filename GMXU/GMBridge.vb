Module GMBridge

    Public Class GMResourceId
        '' constants
        Private Const ID_SIZE As Byte = 32
        Private Const CHARS As String = "0123456789abcdef"
        '' variables
        Private Dim resourceId As String
        '' constructors
        Public Sub New()
            Me.New("00000000-0000-0000-0000-000000000000")
        End Sub
        Public Sub New(Byval id As String)
            Me.Id = id
        End Sub
        '' methods
        Public Shared Function IsValid(Byval Id As String) As Boolean
            Dim anyChar As String = "[" & CHARS & "]"
            Dim mask4b As String = anyChar & anyChar & anyChar & anyChar
            Dim mask8b As String = mask4b & mask4b
            Dim mask12b As String = mask8b & mask4b
            Return Id.ToLower Like (mask8b & "-" & mask4b & "-" & mask4b & "-" & mask4b & "-" & mask12b)
        End Function
        Public Sub AddValue(Byval value As Integer)
            If (value > 0) Then For i As Integer = 1 To value : Increment : Next
            If (value < 0) Then For i As Integer = 1 To -value : Decrement : Next
        End Sub
        Public Sub Increment()
            For pos As Integer = 0 To (resourceId.Length - 1)
                If (resourceId(pos) = "-"c) Then Continue For
                Dim chId As Byte = CHARS.IndexOf(resourceId(pos))
                If (chId < (CHARS.Length - 1)) Then chId += 1 Else chId = 0
                resourceId = resourceId.Remove(pos, 1).Insert(pos, CHARS(chId))
                If (chId <> 0) Then Return
            Next
        End Sub
        Public Sub Decrement()
            For pos As Integer = 0 To (resourceId.Length - 1)
                If (resourceId(pos) = "-"c) Then Continue For
                Dim chId As Byte = CHARS.IndexOf(resourceId(pos))
                If (chId > 0) Then chId -= 1 Else chId = CHARS.Length - 1
                resourceId = resourceId.Remove(pos, 1).Insert(pos, CHARS(chId))
                If (chId <> (CHARS.Length - 1)) Then Return
            Next
        End Sub
        Public Sub Randomise()
            For pos As Integer = 0 To (resourceId.Length - 1)
                If (resourceId(pos) = "-"c) Then Continue For
                Dim ch As Char = CHARS(CInt(Math.Ceiling(Rnd * (CHARS.Length - 1))))
                resourceId = resourceId.Remove(pos, 1).Insert(pos, ch)
            Next
        End Sub
        '' overrides
        Public Overrides Function ToString() As String
            Return Id
        End Function
        Public Overrides Function Equals(Byval obj As Object) As Boolean
            If (obj Is Nothing) Then Return False
            If (Not (Me.GetType Is obj.GetType)) Then Return False
            For chId As Integer = 0 To (resourceId.Length - 1)
                If (resourceId(chId) = "-"c) Then Continue For
                If (resourceId(chId) <> obj.resourceId(chId)) Then Return False
            Next
            Return True
        End Function
        '' properties
        Public Property Id() As String
            Get
                Return resourceId
            End Get
            Set(newId As String)
                If (not IsValid(newId)) Then Throw New ArgumentException("id does not follow specification of " & 
                                                                         "00000000-0000-0000-0000-000000000000, " &
                                                                         "where 0s are any hexadecimal digit.")
                resourceId = newId
            End Set
        End Property
    End Class

End Module