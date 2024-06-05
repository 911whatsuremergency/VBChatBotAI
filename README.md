# VBChatBotAI

An AI-powered chat application built with Visual Basic featuring user authentication, chat history, password recovery, and intelligent responses using OpenAI's GPT-3.5-turbo model.

## Installation

1. **Clone the repository**

   ```sh
   git clone https://github.com/911WUM/VBChatBotAI.git
   ```

2. **Navigate to the project directory**

   ```sh
   cd VBChatBotAI
   ```

3. **Install the required packages**

   ```sh
   Install-Package MySql.Data
   Install-Package Newtonsoft.Json
   Install-Package Guna.UI2.WinForms
   Install-Package System.Speech
   ```

4. **Set up the database**

   Run the `ChatBotDB.sql` script to create the database and necessary tables.

5. **Configure API key and URL**

   Go to the OpenAI website, generate an API key, and update the following lines in your code:

   ```vb
   Dim apiKey As String = "YOUR_API_KEY"
   Dim apiUrl As String = "https://api.openai.com/v1/chat/completions"
   ```

6. **Email Verification (Optional)**

   Update the following line with your email and key:

   ```vb
   smtpServer.Credentials = New Net.NetworkCredential("YOUR_EMAIL", "YOUR_KEY")
   ```


## License

[MIT](https://github.com/911whatsuremergency/VBChatBotAI/blob/main/LICENSE)


## Documentation

[Documentation](https://www.youtube.com/watch?v=5HNyJ5NREZ4)
