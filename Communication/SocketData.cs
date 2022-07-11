using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Communication
{
    public static class SocketData
    {
        /// <summary>
        /// Host of the server
        /// </summary>
        public static string ServerHost { get; } = "127.0.0.1";

        /// <summary>
        /// The port that server is listening
        /// </summary>
        public static int ServerPort { get; } = 8008;

        /// <summary>
        /// Size of buffer array
        /// </summary>
        public static int BufferSize { get; set; } = 256;

        /// <summary>
        /// Default encoding of messages
        /// </summary>
        public static Encoding Encoding { get; set; } = Encoding.Unicode;

        /// <summary>
        /// Ok code
        /// </summary>
        public static string OkCode { get; set; } = "/ok";

        /// <summary>
        /// Fail code
        /// </summary>
        public static string FailCode { get; set; } = "/fail";

        /// <summary>
        /// Code to upload file
        /// </summary>
        public static string UploadCode { get; set; } = "/upload";

        /// <summary>
        /// Code to download file
        /// </summary>
        public static string DownloadCode { get; set; } = "/download";

        /// <summary>
        /// Authorization code
        /// </summary>
        public static string AuthorizationCode { get; set; } = "/login";

        /// <summary>
        /// Registration code
        /// </summary>
        public static string RegistrationCode { get; set; } = "/register";

        /// <summary>
        /// Code to receive premium access
        /// </summary>
        public static string PremiumCode { get; set; } = "/premium";

        /// <summary>
        /// Path to files that were uploaded to the server
        /// </summary>
        public static string UploadedFilesPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "ServerFiles");

        /// <summary>
        /// Receive a message from a socket
        /// </summary>
        /// <param name="socket">Socket to receive a message from</param>
        /// <returns>Received message</returns>
        public static string ReceiveMessage(Socket socket)
        {
            byte[] buffer = new byte[BufferSize];
            int count;
            StringBuilder sb = new StringBuilder();

            do
            {
                count = socket.Receive(buffer);
                sb.Append(Encoding.GetString(buffer, 0, count));
            } while (socket.Available > 0);

            return sb.ToString();
        }

        /// <summary>
        /// Send message to a socket
        /// </summary>
        /// <param name="socket">Socket to send a message to</param>
        /// <param name="message">Message to send</param>
        public static void SendMessage(Socket socket, string message)
        {
            socket.Send(Encoding.GetBytes(message));
        }

        /// <summary>
        /// Send a file to a socket
        /// </summary>
        /// <param name="socket">Socket to send a file to</param>
        /// <param name="path">Path to the file</param>
        public static void SendFile(Socket socket, string path)
        {
            SendMessage(socket, Path.GetExtension(path));
            ReceiveMessage(socket);
            socket.Send(File.ReadAllBytes(path));
        }

        /// <summary>
        /// Receive a file from a socket
        /// </summary>
        /// <param name="socket">Socket to receive file from</param>
        /// /// <param name="path">Path to save a file</param>
        /// <returns>Information about a received file</returns>
        public static FileInfo ReceiveFile(Socket socket, string path)
        {
            string ext = ReceiveMessage(socket);

            SendMessage(socket, OkCode);

            byte[] buffer = new byte[BufferSize];
            int count;

            string fileFullName = Path.Combine(path,
                $"{(socket?.LocalEndPoint as IPEndPoint)?.Address} - {DateTime.Now.ToString().Replace(':', '.')}{ext}");

            using (FileStream file = new FileStream(fileFullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                do
                {
                    count = socket.Receive(buffer);
                    file.Write(buffer, 0, count);
                } while (socket.Available > 0);
            }

            return new FileInfo(fileFullName);
        }
    }
}
