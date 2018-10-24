Imports System.Collections.Specialized
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports System.Xml
Imports System.Xml.Serialization

Namespace Models
    Public Class Functions

        Public Shared Sub CheckSettings()
            If Not Directory.Exists(My.Settings.serverPath) Then
                My.Settings.serverPath = String.Empty
            End If

            If Not Directory.Exists(My.Settings.steamCMDPath) Then
                My.Settings.steamCMDPath = String.Empty
            End If
        End Sub


        Public Shared Function GetLinesCollectionFromTextBox(textBox As Controls.TextBox) As StringCollection
            Dim lines = New StringCollection()
            Dim lineCount As Integer = textBox.LineCount

            For line = 0 To lineCount - 1
                lines.Add(textBox.GetLineText(line))
            Next

            Return lines
        End Function

        Public Shared Function StringFromRichTextBox(rtb As Controls.RichTextBox) As String
            Dim textRange As New TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)
            Return textRange.Text
        End Function

        'Retrieves mods from an XML file
        Public Shared Function GetModsFromXml(filename As String) As List(Of SteamMod)
            Dim xml = File.ReadAllText(filename)
            Return Deserialize(Of List(Of SteamMod))(xml)
        End Function

        Public Shared Sub ExportModsToXml(filename As String, mods As List(Of SteamMod))
            File.WriteAllText(filename, Serialize(mods))
        End Sub

        'Serialise a class
        Private Shared Function Serialize(Of T)(value As T) As String
            If value Is Nothing Then
                Return Nothing
            End If

            Dim serializer = New XmlSerializer(GetType(T))
            Dim settings = New XmlWriterSettings() With {
                    .Encoding = New UnicodeEncoding(False, False),
                    .Indent = True,
                    .OmitXmlDeclaration = False
                    }

            Using textWriter = New StringWriter()

                Using xmlWriter As XmlWriter = XmlWriter.Create(textWriter, settings)
                    serializer.Serialize(xmlWriter, value)
                End Using

                Return textWriter.ToString()
            End Using
        End Function

        'Deserialise a class
        Private Shared Function Deserialize(Of T)(xml As String) As T
            If String.IsNullOrEmpty(xml) Then
                Return Nothing
            End If

            Dim serializer = New XmlSerializer(GetType(T))
            Dim settings = New XmlReaderSettings()

            Using textReader = New StringReader(xml)

                Using xmlReader As XmlReader = XmlReader.Create(textReader, settings)
                    Return serializer.Deserialize(xmlReader)
                End Using
            End Using
        End Function

        Public Shared Function SelectFile(filter As String) As String
            Dim dialog As New Microsoft.Win32.OpenFileDialog With {
                    .Filter = filter
                    }

            If dialog.ShowDialog() <> DialogResult.OK Then
                Return dialog.FileName
            Else 
                Return Nothing
            End If
        End Function

        'Takes any string and removes illegal characters
        Public Shared Function SafeName(input As String, Optional ignoreWhiteSpace As Boolean = False, Optional replacement As Char = "_") As String
            If ignoreWhiteSpace Then
                'input = Regex.Replace(input, "[^a-zA-Z0-9\-_\s]", replacement) >> "-" is allowed
                input = Regex.Replace(input, "[^a-zA-Z0-9_\s]", replacement)
                input = Replace(input, replacement & replacement, replacement)
                Return input
            Else
                'input = Regex.Replace(input, "[^a-zA-Z0-9\-_]", replacement) >> "-" is allowed
                input = Regex.Replace(input, "[^a-zA-Z0-9_]", replacement)
                input = Replace(input, replacement & replacement, replacement)
                Return input
            End If
        End Function
    End Class
End NameSpace