Imports System.Collections.Specialized

Namespace Models
    Public Class Functions
        Public Shared Function GetLinesCollectionFromTextBox(textBox As Controls.TextBox) As StringCollection
            Dim lines = New StringCollection()
            Dim lineCount As Integer = textBox.LineCount

            For line = 0 To lineCount - 1
                lines.Add(textBox.GetLineText(line))
            Next

            Return lines
        End Function

        Public Shared Function StringFromRichTextBox(rtb As RichTextBox) As String
            Dim textRange As New TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)

            Return textRange.Text
        End Function
    End Class
End NameSpace