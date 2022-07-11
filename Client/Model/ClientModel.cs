using Communication;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Client.Model
{
    public class ClientModel : INotifyPropertyChanged
    {
        private Socket? socket;

        /// <summary>
        /// Available downloads
        /// </summary>
        private int availableDownloads;
        public int AvailableDownloads
        {
            get => availableDownloads;
            private set
            {
                availableDownloads = value;
                OnPropertyChanged(nameof(AvailableDownloads));
                OnPropertyChanged(nameof(IsUploadAvailable));
            }
        }

        /// <summary>
        /// True if there is available uploads otherwise false
        /// </summary>
        public bool IsUploadAvailable
        {
            get => AvailableDownloads > 0;
        }

        /// <summary>
        /// Login of the client
        /// </summary>
        private string login = string.Empty;
        public string Login
        {
            get => login;
            set
            {
                login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        /// <summary>
        /// Password of the client
        /// </summary>
        private string password = string.Empty;

        /// <summary>
        /// True if the user is authorized otherwise false
        /// </summary>
        private bool isAuthorized = false;
        public bool IsAuthorized
        {
            get => isAuthorized;

            private set
            {
                isAuthorized = value;
                OnPropertyChanged(nameof(IsAuthorized));
            }
        }

        /// <summary>
        /// Deadlin for the premium access
        /// </summary>

        private DateTime premiumValidUntil;
        public DateTime PremiumValidUntil
        {
            get => premiumValidUntil;
            set
            {
                premiumValidUntil = value;
                OnPropertyChanged(nameof(PremiumValidUntil));
            }
        }

        /// <summary>
        /// True if premium access is active otherwise false
        /// </summary>
        private bool isPremiumValid;
        public bool IsPremiumValid
        {
            get => isPremiumValid;
            private set
            {
                isPremiumValid = value;
                OnPropertyChanged(nameof(IsPremiumValid));
            }
        }

        public ClientModel()
        {
            bool connected = Connect();

            if(connected)
            {
                try
                {
                    SocketData.SendMessage(socket, SocketData.OkCode);
                }
                catch { }
                finally
                {
                    socket?.Shutdown(SocketShutdown.Both);
                    socket?.Close();
                }
            }
            else
            {
                AvailableDownloads = 0;
                PremiumValidUntil = new DateTime();
            }
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        /// <returns>True if connect is successful otherwise false</returns>
        private bool Connect()
        {
            bool result;

            try
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(SocketData.ServerHost), SocketData.ServerPort);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(iPEndPoint);
                SocketData.SendMessage(socket, string.Join(',', Login, password));
                AvailableDownloads = int.Parse(SocketData.ReceiveMessage(socket));
                result = true;
            }
            catch
            {
                result = false;
            }

            IsPremiumValid = PremiumValidUntil > DateTime.Now;

            return result;
        }

        /// <summary>
        /// Get count of available downloads for the user
        /// </summary>
        public void GetAvailableDownloads()
        {
            bool connected = Connect();

            if(!connected)
            {
                AvailableDownloads = 0;
                PremiumValidUntil = new DateTime();
            }
            else
            {
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Close();
            }

        }

        /// <summary>
        /// Authorizate at the server
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <returns>True if authorization is successful otherwise false</returns>
        public bool Authorizate(string login, string password)
        {
            bool connected = Connect();

            if (connected)
            {
                try
                {
                    SocketData.SendMessage(socket, SocketData.AuthorizationCode);

                    return GetAuthorizationResponse(socket, login, password);
                }
                catch { }
                finally
                {
                    socket?.Shutdown(SocketShutdown.Both);
                    socket?.Close();
                }
            }
            return false;
        }

        /// <summary>
        /// Get response for authorization request
        /// </summary>
        /// <param name="socket">Socket of the server</param>
        /// <param name="login">Login of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>True is authorization is successful otherwise false</returns>
        private bool GetAuthorizationResponse(Socket? socket, string login ,string password)
        {
            if(socket == null)
            {
                return false;
            }
            SocketData.ReceiveMessage(socket);
            SocketData.SendMessage(socket, string.Join(',', login, password));

            string response = SocketData.ReceiveMessage(socket);

            if (!response.Equals(SocketData.FailCode))
            {
                this.Login = login;
                this.password = password;
                IsAuthorized = true;
                string[] cols = response.Split("||");

                AvailableDownloads = int.Parse(cols[0]);
                PremiumValidUntil = DateTime.Parse(cols[1]);
                IsPremiumValid = PremiumValidUntil > DateTime.Now;
                return true;
            }
            else
            {
                IsAuthorized = false;
                return false;
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <returns>True if registration is successful otherwise false</returns>
        public bool Register(string login, string password)
        {
            bool connected = Connect();

            if (connected)
            {
                try
                {
                    SocketData.SendMessage(socket, SocketData.RegistrationCode);

                    return GetAuthorizationResponse(socket, login, password);
                }
                catch { }
                finally
                {
                    socket?.Shutdown(SocketShutdown.Both);
                    socket?.Close();
                }
            }
            return false;
        }

        /// <summary>
        /// Log out
        /// </summary>
        public void LogOut()
        {
            Login = string.Empty;
            password = string.Empty;
            GetAvailableDownloads();
            IsAuthorized = false;
            IsPremiumValid = false;
        }

        /// <summary>
        /// Send a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="fileId">Id of the uploaded file</param>
        /// <returns>True if sending is successful otherwise false</returns>
        public bool SendFile(string? filePath, out string fileId)
        {
            bool connected = Connect();

            bool result = false;
            fileId = string.Empty;

            if (connected && filePath != null)
            {
                try
                {
                    SocketData.SendMessage(socket, SocketData.UploadCode);
                    string response = SocketData.ReceiveMessage(socket);

                    if (response.Equals(SocketData.OkCode))
                    {
                        SocketData.SendFile(socket, filePath);
                        result = true;
                        fileId = SocketData.ReceiveMessage(socket);
                        SocketData.SendMessage(socket, SocketData.OkCode);
                        AvailableDownloads = int.Parse(SocketData.ReceiveMessage(socket));
                    }
                }
                catch
                {
                    result = false;
                }
                finally
                {
                    socket?.Shutdown(SocketShutdown.Both);
                    socket?.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Download a file from the server
        /// </summary>
        /// <param name="downloadFileId">Id of the file</param>
        /// <param name="path">Path to which save the file</param>
        /// <returns>True if download is successful otherwise false</returns>
        public bool ReceiveFile(string? downloadFileId, string path)
        {
            bool connected = Connect();

            bool result = false;

            if (connected && downloadFileId != null)
            {
                try
                {
                    SocketData.SendMessage(socket, SocketData.DownloadCode);
                    SocketData.ReceiveMessage(socket);
                    SocketData.SendMessage(socket, downloadFileId);

                    string response = SocketData.ReceiveMessage(socket);

                    if(response.Equals(SocketData.OkCode))
                    {
                        SocketData.SendMessage(socket, SocketData.OkCode);

                        SocketData.ReceiveFile(socket, path);

                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch
                {
                    result = false;
                }
                finally
                {
                    socket?.Shutdown(SocketShutdown.Both);
                    socket?.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Add premium access to the user
        /// </summary>
        /// <returns>True if premium acces is added otherwise false</returns>
        public bool AddPremium()
        {
            bool connected = Connect();

            if (connected)
            {
                try
                {
                    SocketData.SendMessage(socket, SocketData.PremiumCode);
                    SocketData.ReceiveMessage(socket);
                    SocketData.SendMessage(socket, string.Join(',', login, password));

                    string response = SocketData.ReceiveMessage(socket);

                    if (response.Equals(SocketData.FailCode))
                    {
                        return false;
                    }

                    string[] cols = response.Split("||");

                    AvailableDownloads = int.Parse(cols[0]);
                    PremiumValidUntil = DateTime.Parse(cols[1]);
                    IsPremiumValid = PremiumValidUntil > DateTime.Now;

                    return true;
                }
                catch { }
                finally
                {
                    socket?.Shutdown(SocketShutdown.Both);
                    socket?.Close();
                }
            }

            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
