'' extends the string and array types
Imports System.Runtime.CompilerServices

Module TypeExtensions

    #Region "Array Extensions"
    <Extension()>Public Sub Append(Of Type)(ByRef arr As Type(), value As Type)
        '' appends a value to an array
        Dim recordIndex As Integer = arr.Length
        Array.Resize(arr,recordIndex+1)
        arr(recordIndex) = value
    End Sub
    #End Region

    #Region "String Extensions"
    <Extension()>Public Function ReplaceAtPosition(ByRef str As string, ByVal pos As Integer, ByVal newChar As Char) As String
        Dim l As Integer = str.Length
        If((pos<0)Or(pos>=l)) Then Return str
        If(pos=(l-1)) Then Return str.Substring(0,pos) & newChar
        return str.Substring(0,pos) & newChar & str.Substring(pos+1,l-pos-1)
    End Function
    <Extension()>Public Function Separate(Byref str As String, ByRef separators As char(), ByVal clipBegin As Char, ByVal clipEnd As Char) As String()
        '' splits a string into an array, but avoiding any delimiters
        Dim splitArray As String() = {}
        Dim clippingStack As Integer = 0
        Dim currentSegment As String = ""
        For each currentChar As Char In str
            If((clippingStack=0) And separators.Contains(currentChar))
                '' current character is a separator character
                If(currentSegment<>"") Then splitArray.Append(currentSegment)
                currentSegment = ""
            Else
                '' normal char
                currentSegment += currentChar
                If(clipBegin<>clipEnd)
                    If(currentChar=clipBegin)
                        '' increment stack
                        clippingStack += 1
                    ElseIf(currentChar=clipEnd)
                        '' decrement stack
                        clippingStack -= 1
                    End If               
                Else If(currentChar=clipBegin)
                    clippingStack -= 1  '  0 -  1 = -1,  1 -  1 = 0,
                    clippingStack *= -1 ' -1 * -1 =  1,  0 * -1 = 0, => 0 becomes 1 and 1 becomes 0
                End If
            End If
        Next
        '' add final segment
        If(currentSegment<>"") then splitArray.Append(currentSegment)
        Return splitArray
    End Function
    <Extension()>Public Function Separate(Byref str As String, ByRef separator As char, ByVal clipBegin As Char, ByVal clipEnd As Char) As String()
        Return str.Separate({separator},clipBegin,clipEnd) 
    End Function
    <Extension()>Public Function Separate(Byref str As String, ByRef separators As char(), ByVal clip As Char) As String()
        Return str.Separate(separators,clip,clip)
    End Function
    <Extension()>Public Function Separate(Byref str As String, ByRef separator As char, ByVal clip As Char) As String()
        Return str.Separate({separator},clip)
    End Function
    #End Region

End Module