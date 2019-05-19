''' <summary>
''' A class used to simulate the behaviour of yy resource ids.
''' </summary>
''' <remarks>
''' These ids are 128-bit UUIDs and are used to stitch the project resources together.
''' </remarks>
Public Class GMResourceId
    
    Private Const ID_SIZE As Byte = 32
    Private Const CHARS As String = "0123456789abcdef"
    Private Dim resourceId As String
    
    ''' <summary>
    ''' Constructs a default <c>GMResourceId</c>.
    ''' </summary>
    Public Sub New()
        Me.New("00000000-0000-0000-0000-000000000000")
    End Sub

    ''' <summary>
    ''' Constructs a new <c>GMResourceId</c> from an existing UUID.
    ''' </summary>
    ''' <param name="id">A 128-bit UUID.</param>
    ''' <exception cref="ArgumentException"><see cref="Id"/></exception>
    Public Sub New(Byval id As String)
        Me.Id = id
    End Sub
    
    ''' <summary>
    ''' Determines whether the input UUID matches the specification.
    ''' </summary>
    ''' <remarks>
    ''' Can be used to check whether a UUID is valid before constructing a new <c>GMResourceId</c> or setting the UUID of an existing <c>GMResourceId</c>.
    ''' </remarks>
    ''' <param name="id">A 128-bit UUID.</param>
    ''' <returns><code>True</code> or <code>False</code> depending on whether the UUID is valid.</returns>
    Public Shared Function IsValid(Byval id As String) As Boolean
        Dim anyChar As String = "[" & CHARS & "]"
        Dim mask4b As String = anyChar & anyChar & anyChar & anyChar
        Dim mask8b As String = mask4b & mask4b
        Dim mask12b As String = mask8b & mask4b
        Return id.ToLower Like (mask8b & "-" & mask4b & "-" & mask4b & "-" & mask4b & "-" & mask12b)
    End Function

    ''' <summary>
    ''' Adds an arbitrary integer to the UUID of this <c>GMResourceId</c>.
    ''' </summary>
    ''' <remarks>
    ''' The value will overflow and underflow without throwing exceptions.
    ''' </remarks>
    ''' <example>
    ''' Adding 1 to the UUID:
    ''' <code>
    ''' ffffffff-ffff-ffff-ffff-ffffffffffff
    ''' </code>
    ''' Will overflow to the UUID:
    ''' <code>
    ''' 00000000-0000-0000-0000-000000000000
    ''' </code>
    ''' </example>
    ''' <param name="value">A value to increment by.</param>
    Public Sub AddValue(Byval value As Integer)
        If (value > 0) Then For i As Integer = 1 To value : Increment : Next
        If (value < 0) Then For i As Integer = 1 To -value : Decrement : Next
    End Sub

    ''' <summary>
    ''' Nudges the value of the UUID of this <c>GMResourceId</c> up by one.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="AddValue(Integer)"/>
    ''' </remarks>
    Public Sub Increment()
        For pos As Integer = 0 To (resourceId.Length - 1)
            If (resourceId(pos) = "-"c) Then Continue For
            Dim chId As Byte = CHARS.IndexOf(resourceId(pos))
            If (chId < (CHARS.Length - 1)) Then chId += 1 Else chId = 0
            resourceId = resourceId.Remove(pos, 1).Insert(pos, CHARS(chId))
            If (chId <> 0) Then Return
        Next
    End Sub

    ''' <summary>
    ''' Nudges the value of the UUID of this <c>GMResourceId</c> down by one.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="AddValue(Integer)"/>
    ''' </remarks>
    Public Sub Decrement()
        For pos As Integer = 0 To (resourceId.Length - 1)
            If (resourceId(pos) = "-"c) Then Continue For
            Dim chId As Byte = CHARS.IndexOf(resourceId(pos))
            If (chId > 0) Then chId -= 1 Else chId = CHARS.Length - 1
            resourceId = resourceId.Remove(pos, 1).Insert(pos, CHARS(chId))
            If (chId <> (CHARS.Length - 1)) Then Return
        Next
    End Sub

    ''' <summary>
    ''' Randomises the UUID of this <c>GMResourceId</c>.
    ''' </summary>
    Public Sub Randomise()
        For pos As Integer = 0 To (resourceId.Length - 1)
            If (resourceId(pos) = "-"c) Then Continue For
            Dim ch As Char = CHARS(CInt(Math.Ceiling(Rnd * (CHARS.Length - 1))))
            resourceId = resourceId.Remove(pos, 1).Insert(pos, ch)
        Next
    End Sub
    
    ''' <summary>
    ''' Returns the string representation for the UUID of this <c>GMResourceId</c>.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Return Id
    End Function

    ''' <summary>
    ''' Checks equality between two <c>GMResourceId</c>s.
    ''' </summary>
    ''' <param name="obj">Any <code>Object</code>.</param>
    ''' <returns><c>True</c> or <c>False</c> depending on whether the <paramref name="obj"/> is equal to this <c>GMResourceId</c>.</returns>
    Public Overrides Function Equals(Byval obj As Object) As Boolean
        If (obj Is Nothing) Then Return False
        If (Not (Me.GetType Is obj.GetType)) Then Return False
        For chId As Integer = 0 To (resourceId.Length - 1)
            If (resourceId(chId) = "-"c) Then Continue For
            If (resourceId(chId) <> obj.resourceId(chId)) Then Return False
        Next
        Return True
    End Function
    
    ''' <summary>
    ''' Gets and/or sets the UUID of this <c>GMResourceId</c>.
    ''' </summary>
    ''' <value>The UUID of this <c>GMResourceId</c>.</value>
    ''' <exception cref="ArgumentException">Thrown when the new UUID does not match the specification. <see cref="GMResourceId.IsValid(String)"/></exception>
    Public Property Id() As String
        Get
            Return resourceId
        End Get
        Set(newId As String)
            If (not IsValid(newId)) Then Throw New ArgumentException( _
                "id does not follow specification of " & 
                "00000000-0000-0000-0000-000000000000, " &
                "where 0s are any hexadecimal digit.")
            resourceId = newId
        End Set
    End Property
End Class