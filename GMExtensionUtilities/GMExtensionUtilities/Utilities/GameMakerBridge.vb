

Module GameMakerBridge

    Public Class GamemakerResourceID
        
        '' define base
        Private Const baseChars As String = "0123456789abcdef"

        '' define properties
        Private _id_ As String
        Public Property id() As String
            Get
                '' Format: 00000000-0000-0000-0000-000000000000
                Return _
                    _id_.Substring(0,8) & "-" & _
                    _id_.Substring(8,4) & "-" & _
                    _id_.Substring(12,4) & "-" & _
                    _id_.Substring(16,4) & "-" & _
                    _id_.Substring(20,12)
            End Get
            Set(ByVal GMID As String)
                '' remove dashes from string
                GMID = GMID.Replace("-"c,"")
                If(GMID.Length=32)
                    Dim newID As String = ""
                    For each IDChar As Char In GMID.ToLower
                        If(baseChars.Contains(IDChar))
                            '' character is valid
                            newID += IDChar
                        Else
                            '' invalid character; abort
                            Exit Property
                        End If
                    Next
                    '' update id
                    _id_ = newID
                End If
            End Set
        End Property

        '' create constructor
        Public Sub New(Optional ByVal GMID As String = "00000000000000000000000000000000")
            id = GMID
        End Sub
       
        '' define methods
        Public Sub increment(ByVal count As Integer)
            '' increment id by count
            Dim currentIndex As Integer = 0
            Dim valueOverflow As Integer = count
            Dim valueMax As Integer = baseChars.Length
            Dim valueMin As Integer = 0
            Dim valueDiff As Integer = valueMax-valueMin
            Do
                '' get the value of the current character
                Dim currentChar As Char = _id_(currentIndex)
                Dim currentValue As Integer = baseChars.IndexOf(currentChar) + valueOverflow
                '' control overflow/underflow
                valueOverflow = 0
                If(currentValue>=valueMax)
                    While(currentValue>=valueMax)
                        currentValue -= valueDiff
                        valueOverflow += 1
                    End While
                Else If(currentValue<valueMin)
                    While(currentValue<valueMin)
                        currentValue += valueDiff
                        valueOverflow -= 1
                    End While
                End If
                '' update current character
                Dim nextChar As Char = baseChars(currentValue)
                _id_ = _id_.ReplaceAtPosition(currentIndex,nextChar)
                currentIndex += 1
            Loop Until((valueOverflow=0) Or (currentIndex>=_id_.length))
        End Sub
        Public Sub randomize()
            '' randomise id
            Dim currentIndex As Integer = 0
            Dim valueMax As Integer = baseChars.Length
            Dim valueMin As Integer = 0
            Do
                '' update current character
                Dim nextChar As Char = baseChars(CInt(Math.Ceiling(Rnd() * (valueMax-1))) + valueMin)
                _id_ = _id_.ReplaceAtPosition(currentIndex,nextChar)
                currentIndex += 1
            Loop Until(currentIndex>=_id_.length)
        End Sub

    End Class

End Module

