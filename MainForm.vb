Imports MySql.Data.MySqlClient
Imports System.Net.Http
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Text
Imports System.Drawing
Imports System.Net.Mail
Imports System.Speech.Synthesis

Public Class MainForm
    Private _username As String
    Private synthesizer As SpeechSynthesizer

    Public Sub New()
        InitializeComponent()
        synthesizer = New SpeechSynthesizer()
    End Sub

    Public Sub New(username As String)
        Me.New()
        _username = username
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ContentPanel.Visible = False
        RegisterPanel.Visible = False
        RecoveryPanel.Visible = False
        PanelEmailVerify.Visible = False
        PanelMyAccount.Visible = False
        PanelChangePassword.Visible = False
        LoginPanel.Visible = True

        AddHandler MainChatSend.KeyDown, AddressOf MainChatSend_KeyDown

        MainAI.ReadOnly = True
        MainAI.TabStop = False
        MainChatSend.Focus()

        LoadRememberedUser()

        If CheckUserExists() Then
            LoginCreateAccount.Visible = False
        End If
    End Sub

    Private Sub LogoutButton_Click(sender As Object, e As EventArgs) Handles LogoutButton.Click
        PerformLogout()
    End Sub

    Private Sub LogoutButton2_Click(sender As Object, e As EventArgs) Handles LogoutButton2.Click
        PerformLogout()
    End Sub

    Private Sub PerformLogout()
        ResetChatBot()
        ContentPanel.Visible = False
        PanelMyAccount.Visible = False
        PanelChangePassword.Visible = False
        LoginPanel.Visible = True

        If Not LoginSwitch.Checked Then
            LoginUsername.Text = ""
            LoginPassword.Text = ""
        End If
    End Sub

    Private Sub ResetChatBot()
        MainAI.Clear()
    End Sub

    Private Sub RegisterBack_Click(sender As Object, e As EventArgs) Handles RegisterBack.Click
        RegisterPanel.Visible = False
        LoginPanel.Visible = True
    End Sub

    Private Sub VerifyBack_Click(sender As Object, e As EventArgs) Handles VerifyBack.Click
        PanelEmailVerify.Visible = False
        LoginPanel.Visible = True
    End Sub

    Private Sub VerifyBack2_Click(sender As Object, e As EventArgs) Handles VerifyBack2.Click
        RecoveryPanel.Visible = False
        PanelEmailVerify.Visible = True
    End Sub

    Private Sub MyAccountBack_Click(sender As Object, e As EventArgs) Handles MyAccountBack.Click
        PanelMyAccount.Visible = False
        ContentPanel.Visible = True
    End Sub

    Private Sub AccountButton_Click(sender As Object, e As EventArgs) Handles AccountButton.Click
        ContentPanel.Visible = False
        PanelMyAccount.Visible = True
        LoadAccountDetails()
    End Sub

    Private Sub LoadAccountDetails()
        Dim connectionString = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query = "SELECT * FROM Users WHERE Username=@Username"
                Using cmd As New MySqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@Username", _username)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim username = reader("Username").ToString()
                            Dim email = reader("Email").ToString()
                            AccountInformation.Text = $"Username: {username}{Environment.NewLine}Email: {email}"
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred while loading account details: " & ex.Message)
        End Try
    End Sub

    Private Sub LoginButton_Click(sender As Object, e As EventArgs) Handles LoginButton.Click
        Dim connectionString = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query = "SELECT COUNT(*) FROM Users WHERE Username=@Username AND Password=@Password"
                Using cmd As New MySqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@Username", LoginUsername.Text)
                    cmd.Parameters.AddWithValue("@Password", LoginPassword.Text)
                    Dim result As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    If result > 0 Then
                        _username = LoginUsername.Text
                        ResetChatBot()
                        LoginPanel.Visible = False
                        ContentPanel.Visible = True

                        If LoginSwitch.Checked Then
                            RememberUser(LoginUsername.Text, LoginPassword.Text)
                        End If
                    Else
                        MessageBox.Show("Invalid username or password")
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
    End Sub

    Private Sub LoginCreateAccount_Click(sender As Object, e As EventArgs) Handles LoginCreateAccount.Click
        LoginPanel.Visible = False
        RegisterPanel.Visible = True
    End Sub

    Private Sub LoginForgot_Click(sender As Object, e As EventArgs) Handles LoginForgot.Click
        LoginPanel.Visible = False
        PanelEmailVerify.Visible = True
    End Sub

    Private Sub ButtonEmailVerify_Click(sender As Object, e As EventArgs) Handles ButtonEmailVerify.Click
        If Not IsValidEmail(GetEmailVerify.Text) Then
            MessageBox.Show("Invalid email format.")
            Return
        End If

        Dim recoveryCode As String = GenerateRecoveryCode()
        Dim connectionString = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim updateQuery = "UPDATE Users SET RecoveryCode=@RecoveryCode WHERE Email=@Email"
                Using updateCmd As New MySqlCommand(updateQuery, connection)
                    updateCmd.Parameters.AddWithValue("@RecoveryCode", recoveryCode)
                    updateCmd.Parameters.AddWithValue("@Email", GetEmailVerify.Text)
                    updateCmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
            Return
        End Try

        SendRecoveryEmail(GetEmailVerify.Text, recoveryCode)
        Dim result As DialogResult = MessageBox.Show("Recovery code has been sent to your email.", "Info", MessageBoxButtons.OK)
        If result = DialogResult.OK Then
            PanelEmailVerify.Visible = False
            RecoveryPanel.Visible = True
        End If
    End Sub

    Private Sub RememberUser(username As String, password As String)
        Dim connectionString As String = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query As String = "UPDATE Users SET RememberMe=1, Password=@Password WHERE Username=@Username"
                Using cmd As New MySqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@Username", username)
                    cmd.Parameters.AddWithValue("@Password", password)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred while saving user: " & ex.Message)
        End Try
    End Sub

    Private Sub LoadRememberedUser()
        Dim connectionString As String = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query As String = "SELECT Username, Password FROM Users WHERE RememberMe=1 LIMIT 1"
                Using cmd As New MySqlCommand(query, connection)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            LoginUsername.Text = reader("Username").ToString()
                            LoginPassword.Text = reader("Password").ToString()
                            LoginSwitch.Checked = True
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred while loading remembered user: " & ex.Message)
        End Try
    End Sub

    Private Sub ButtonCodes_Click(sender As Object, e As EventArgs) Handles ButtonCodes.Click
        Dim enteredCode = InputCodes.Text.Trim

        If VerifyRecoveryCode(enteredCode) Then
            RecoveryPanel.Visible = False
            ContentPanel.Visible = True
        Else
            MessageBox.Show("The recovery code you entered is incorrect. Please try again.")
        End If
    End Sub

    Private Sub RegisterButton_Click(sender As Object, e As EventArgs) Handles RegisterButton.Click
        If String.IsNullOrWhiteSpace(RegisterUsername.Text) Then
            MessageBox.Show("Username cannot be empty.")
            Return
        End If

        If String.IsNullOrWhiteSpace(RegisterEmail.Text) Then
            MessageBox.Show("Email cannot be empty.")
            Return
        End If

        If String.IsNullOrWhiteSpace(RegisterPassword.Text) Then
            MessageBox.Show("Password cannot be empty.")
            Return
        End If

        If Not IsValidEmail(RegisterEmail.Text) Then
            MessageBox.Show("Invalid email format.")
            Return
        End If

        If CheckUserExists() Then
            MessageBox.Show("An account already exists. You cannot create another account.")
            RegisterPanel.Visible = False
            LoginPanel.Visible = True
            Return
        End If

        Dim connectionString = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query = "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)"
                Using cmd As New MySqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@Username", RegisterUsername.Text)
                    cmd.Parameters.AddWithValue("@Email", RegisterEmail.Text)
                    cmd.Parameters.AddWithValue("@Password", RegisterPassword.Text)
                    cmd.ExecuteNonQuery()
                    MessageBox.Show("Registration successful")
                    RegisterPanel.Visible = False
                    LoginPanel.Visible = True
                    LoginCreateAccount.Visible = False
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
    End Sub

    Private Function IsValidEmail(email As String) As Boolean
        Try
            Dim addr = New System.Net.Mail.MailAddress(email)
            Return addr.Address = email
        Catch
            Return False
        End Try
    End Function

    Private Sub SendRecoveryEmail(toEmail As String, recoveryCode As String)
        Try
            Dim mail As New MailMessage()
            mail.From = New MailAddress("no-reply@babaturanaI.com", "BabaturanAI Support")
            mail.To.Add(toEmail)
            mail.Subject = "BabaturanAI - Recovery Code"

            mail.Body = $"Dear User," & Environment.NewLine & Environment.NewLine &
                    $"We have received a request to reset your password for your BabaturanAI account." & Environment.NewLine &
                    $"Please use the following recovery code to reset your password:" & Environment.NewLine & Environment.NewLine &
                    $"Recovery Code: {recoveryCode}" & Environment.NewLine & Environment.NewLine &
                    $"If you did not request this change, please ignore this email or contact our support team for assistance." & Environment.NewLine & Environment.NewLine &
                    $"Best regards," & Environment.NewLine &
                    $"The BabaturanAI Team" & Environment.NewLine & Environment.NewLine &
                    $"Note: This is an automated message, please do not reply to this email."

            Dim smtpServer As New SmtpClient("smtp.gmail.com")
            smtpServer.Port = 587
            smtpServer.Credentials = New Net.NetworkCredential("YOUR_EMAIL", "YOUR_KEY")
            smtpServer.EnableSsl = True

            smtpServer.Send(mail)
        Catch ex As Exception
            MessageBox.Show("An error occurred while sending the email: " & ex.Message)
        End Try
    End Sub

    Private Function GenerateRecoveryCode() As String
        Dim random As New Random()
        Return random.Next(100000, 999999).ToString()
    End Function

    Private Function VerifyRecoveryCode(enteredCode As String) As Boolean
        Dim connectionString As String = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"

        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query As String = "SELECT COUNT(1) FROM Users WHERE RecoveryCode=@RecoveryCode"
                Using cmd As New MySqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@RecoveryCode", enteredCode)
                    Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    Return count = 1
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred while verifying the recovery code: " & ex.Message)
            Return False
        End Try
    End Function

    Private Async Sub MainChatSend_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            Await SendMessage()
            e.Handled = True
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Async Sub MainChatSend_IconRightClick(sender As Object, e As EventArgs) Handles MainChatSend.IconRightClick
        Await SendMessage()
    End Sub

    Private Async Function SendMessage() As Task
        Dim message As String = MainChatSend.Text.Trim()

        If String.IsNullOrWhiteSpace(message) Then
            MessageBox.Show("Message cannot be empty.")
            Return
        End If

        Dim response As String = Await GetAIResponse(message)
        AppendFormattedText(MainAI, "You", message, False)
        AppendFormattedText(MainAI, "Bot", response, True)

        If SpeechSwitch.Checked Then
            Speak(response)
        End If

        SaveChat(message, response)
        MainChatSend.Clear()
    End Function

    Private Sub AppendFormattedText(rtb As RichTextBox, user As String, text As String, isBot As Boolean)
        Dim timestamp As String = DateTime.Now.ToString("HH:mm:ss")
        Dim header As String = $"{user} [{timestamp}]:{Environment.NewLine}"

        If isBot Then
            AppendTextWithStyle(rtb, header, Color.Green, New Font("Segoe UI", 9, FontStyle.Bold))
            AppendTextWithStyle(rtb, text & Environment.NewLine & Environment.NewLine, Color.Black, New Font("Segoe UI", 9))
        Else
            AppendTextWithStyle(rtb, header, Color.Blue, New Font("Segoe UI", 9, FontStyle.Bold))
            AppendTextWithStyle(rtb, text & Environment.NewLine & Environment.NewLine, Color.Black, New Font("Segoe UI", 9))
        End If
    End Sub

    Private Sub AppendTextWithStyle(rtb As RichTextBox, text As String, color As Color, font As Font)
        rtb.SelectionStart = rtb.TextLength
        rtb.SelectionLength = 0
        rtb.SelectionColor = color
        rtb.SelectionFont = font
        rtb.AppendText(text)
        rtb.SelectionColor = rtb.ForeColor
        rtb.SelectionFont = rtb.Font
    End Sub

    Private Sub Speak(text As String)
        synthesizer.SpeakAsync(text)
    End Sub

    Private Async Function GetAIResponse(ByVal input As String) As Task(Of String)
        Dim apiKey As String = "API_KEY"
        Dim apiUrl As String = "API_URL"
        Dim model As String = "gpt-3.5-turbo"

        Using client As New HttpClient()
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}")
            Dim requestBody As New Dictionary(Of String, Object) From {
                {"model", model},
                {"messages", New List(Of Object) From {
                    New With {Key .role = "user", Key .content = input}
                }}
            }
            Dim json As String = JsonConvert.SerializeObject(requestBody)
            Dim content As New StringContent(json, Encoding.UTF8, "application/json")
            Dim response As HttpResponseMessage = Await client.PostAsync(apiUrl, content)
            Dim responseString As String = Await response.Content.ReadAsStringAsync()

            Dim jsonResponse As JObject = JObject.Parse(responseString)

            If jsonResponse.ContainsKey("error") Then
                Dim errorMessage As String = jsonResponse("error")("message").ToString()
                Return "Error: " & errorMessage
            End If

            If jsonResponse.ContainsKey("choices") Then
                Dim choices = jsonResponse("choices")
                Dim firstChoice = choices(0)("message")("content").ToString().Trim()
                Return firstChoice
            Else
                Return "Error: Invalid API response"
            End If
        End Using
    End Function

    Private Sub SaveChat(message As String, response As String)
        Dim connectionString As String = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query As String = "INSERT INTO Chats (Username, Message, Response, Timestamp) VALUES (@Username, @Message, @Response, @Timestamp)"
                Using cmd As New MySqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@Username", _username)
                    cmd.Parameters.AddWithValue("@Message", message)
                    cmd.Parameters.AddWithValue("@Response", response)
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
    End Sub

    Private Function CheckUserExists() As Boolean
        Dim connectionString As String = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query As String = "SELECT COUNT(*) FROM Users"
                Using cmd As New MySqlCommand(query, connection)
                    Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    Return count > 0
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred while checking user existence: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub ButtonChangePassword_Click(sender As Object, e As EventArgs) Handles ButtonChangePassword.Click
        PanelMyAccount.Visible = False
        PanelChangePassword.Visible = True
    End Sub

    Private Sub ChangePasswordBack_Click(sender As Object, e As EventArgs) Handles ChangePasswordBack.Click
        PanelChangePassword.Visible = False
        PanelMyAccount.Visible = True
    End Sub

    Private Sub ChangePassButton_Click(sender As Object, e As EventArgs) Handles ChangePassButton.Click
        If String.IsNullOrWhiteSpace(KolomNewPassword.Text) Or String.IsNullOrWhiteSpace(KolomVerifyPassword.Text) Then
            MessageBox.Show("Password fields cannot be empty.")
            Return
        End If

        If KolomNewPassword.Text <> KolomVerifyPassword.Text Then
            MessageBox.Show("Passwords do not match.")
            Return
        End If

        Dim connectionString = "Server=127.0.0.1;Database=chatbotdb;Uid=root;Pwd=;"
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()
                Dim query = "UPDATE Users SET Password=@Password WHERE Username=@Username"
                Using cmd As New MySqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@Username", _username)
                    cmd.Parameters.AddWithValue("@Password", KolomNewPassword.Text)
                    cmd.ExecuteNonQuery()
                    MessageBox.Show("Password changed successfully")

                    If LoginSwitch.Checked Then
                        LoginPassword.Text = KolomNewPassword.Text
                    End If

                    ResetChatBot()
                    KolomNewPassword.Text = ""
                    KolomVerifyPassword.Text = ""
                    PanelChangePassword.Visible = False
                    LoginPanel.Visible = True
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
    End Sub
End Class
