Imports System.Security.Cryptography
Imports Enumerable = System.Linq.Enumerable

Namespace Models

    Public Class Encryption
        Private Shared ReadOnly TripleDes As New TripleDESCryptoServiceProvider
        Private Shared _instance As Encryption


        Public Shared ReadOnly Property Instance As Encryption
            Get
                'If there is no instance or it has been destroyed...
                If _instance Is Nothing Then
                    '...create a new one.
                    _instance = New Encryption
                End If

                Return _instance
            End Get
        End Property

        Private Shared Function TruncateHash(key As String, length As Integer) As Byte()

            Dim sha1 As New SHA1CryptoServiceProvider

            ' Hash the key.
            Dim keyBytes() As Byte = Text.Encoding.Unicode.GetBytes(key)
            Dim hash() As Byte = sha1.ComputeHash(keyBytes)

            ' Truncate or pad the hash.
            ReDim Preserve hash(length - 1)
            Return hash
        End Function

        Private Sub New()
            ' Initialize the crypto provider.
            Dim key As String = Environment.UserName & SystemSerialNumber()
            TripleDes.Key = TruncateHash(key, TripleDes.KeySize \ 8)
            TripleDes.IV = TruncateHash("", TripleDes.BlockSize \ 8)
        End Sub

        Public Function EncryptData(plaintext As String) As String
            Try
                ' Convert the plaintext string to a byte array.
                Dim plaintextBytes() As Byte = Text.Encoding.Unicode.GetBytes(plaintext)

                ' Create the stream.
                Dim ms As New IO.MemoryStream
                ' Create the encoder to write to the stream.
                Dim encStream As New CryptoStream(ms, TripleDes.CreateEncryptor(), CryptoStreamMode.Write)

                ' Use the crypto stream to write the byte array to the stream.
                encStream.Write(plaintextBytes, 0, plaintextBytes.Length)
                encStream.FlushFinalBlock()

                ' Convert the encrypted stream to a printable string.
                Return Convert.ToBase64String(ms.ToArray)
            Catch ex As Exception
                Return Nothing
            End Try

        End Function

        Public Function DecryptData(encryptedtext As String) As String
            Try
                ' Convert the encrypted text string to a byte array.
                Dim encryptedBytes() As Byte = Convert.FromBase64String(encryptedtext)

                ' Create the stream.
                Dim ms As New IO.MemoryStream
                ' Create the decoder to write to the stream.
                Dim decStream As New CryptoStream(ms, TripleDes.CreateDecryptor(), CryptoStreamMode.Write)

                ' Use the crypto stream to write the byte array to the stream.
                decStream.Write(encryptedBytes, 0, encryptedBytes.Length)
                decStream.FlushFinalBlock()

                ' Convert the plaintext stream to a string.
                Return Text.Encoding.Unicode.GetString(ms.ToArray)
            Catch ex As Exception
                Return Nothing
            End Try

        End Function

        Private Shared Function SystemSerialNumber() As String
            ' Get the Windows Management Instrumentation object.
            Dim wmi As Object = GetObject("WinMgmts:")

            ' Get the base boards.
            Dim motherBoards As Object = wmi.InstancesOf("Win32_BaseBoard")
            Dim serialNumbers = Enumerable.Cast(Of Object)(motherBoards).Aggregate("", Function(current, board) current & (", " & board.SerialNumber))
            If serialNumbers.Length > 0 Then serialNumbers = serialNumbers.Substring(2)
            Return serialNumbers
        End Function

    End Class
End NameSpace