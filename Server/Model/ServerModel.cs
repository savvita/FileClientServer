using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Communication;
using Microsoft.Win32;

namespace Server.Model
{
    public class ServerModel
    {
        private TcpListener? listener;

        private Users users;

        private int today = DateTime.Now.DayOfYear;

        /// <summary>
        /// List of the files uploaded to the server
        /// </summary>
        public ObservableCollection<FileModel>? Files { get; private set; } = new ObservableCollection<FileModel>();


        #region Events
        /// <summary>
        /// Raise when during the connection was an error
        /// </summary>
        public event Action<string>? GotError;

        /// <summary>
        /// Raise when the server is started to listening
        /// </summary>
        public event Action<string>? ServerStarted;

        /// <summary>
        /// Raise when list of files is changed
        /// </summary>
        public event Action? FilesChanged;

        #endregion

        public ServerModel()
        {
            AddToAutoRun();
            users = new Users();

            if (!Directory.Exists(SocketData.UploadedFilesPath))
            {
                Directory.CreateDirectory(SocketData.UploadedFilesPath);
            }

            LoadFiles();
            CheckFiles();

            Thread refreshThread = new Thread(RefreshListOfFiles)
            {
                IsBackground = true
            };

            refreshThread.Start();

            Thread checkDayThread = new Thread(CheckNewDay)
            {
                IsBackground = true
            };

            checkDayThread.Start();
        }

        /// <summary>
        /// Connect to the endpoint
        /// </summary>
        /// <returns>True if connect is successful otherwise false</returns>
        private bool Connect()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, SocketData.ServerPort);
                listener.Start();

                ServerStarted?.Invoke($"[{DateTime.Now}] Server started");

                return true;
            }
            catch (Exception ex)
            {
                OnGotError($"[{DateTime.Now}] {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Listen to the port
        /// </summary>
        public void Listen()
        {
            bool connected = Connect();

            if (connected)
            {
                try
                {
                    do
                    {
                        Socket? socket = listener?.AcceptSocket();

                        Thread thread = new Thread((obj) => HandleClient(socket));
                        thread.Start();

                    } while (true);
                }
                catch (Exception ex)
                {
                    OnGotError($"[{DateTime.Now}] {ex.Message}");     
                    Close();
                }
            }
        }

        /// <summary>
        /// Handle client connection
        /// </summary>
        /// <param name="obj">Client socket</param>
        private void HandleClient(object? obj)
        {
            if (obj is Socket socket)
            {
                try
                {
                    string authorization = SocketData.ReceiveMessage(socket);
                    string[] cols = authorization.Split(',');

                    string? login = null;
                    string? password = null;

                    if (users.IsUserRegistered(cols[0], cols[1]))
                    {
                        login = cols[0];
                        password = cols[0];
                    }

                    string? ip = null;

                    if(socket.RemoteEndPoint is IPEndPoint endPoint)
                    {
                        ip = Convert.ToString(endPoint.Address);
                    }

                    int available = users.GetAvailableDownloads(ip, login, password);

                    SocketData.SendMessage(socket, available.ToString());

                    string code = SocketData.ReceiveMessage(socket);

                    if (code.Equals(SocketData.UploadCode))
                    {
                        if (available > 0)
                        {
                            ReceiveFile(socket);
                            users.DecreaseAvailableDownloads(ip, login);
                            SocketData.ReceiveMessage(socket);
                            SocketData.SendMessage(socket, users.GetAvailableDownloads(ip, login, password).ToString());
                        }
                        else
                        {
                            SocketData.SendMessage(socket, SocketData.FailCode);
                        }
                    }
                    else if(code.Equals(SocketData.DownloadCode))
                    {
                        SendFile(socket);
                    }
                    else if(code.Equals(SocketData.RegistrationCode))
                    {
                        RegisterClient(socket);
                    }
                    else if (code.Equals(SocketData.AuthorizationCode))
                    {
                        AuthorizateClient(socket);
                    }
                    else if(code.Equals(SocketData.PremiumCode))
                    {
                        SetPremium(socket);
                    }
                }
                catch (Exception ex)
                {
                    OnGotError($"[{DateTime.Now}] {ex.Message}");
                }
                finally
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
        }

        /// <summary>
        /// Set premium access to the user
        /// </summary>
        /// <param name="socket">Socket of the user</param>
        private void SetPremium(Socket socket)
        {
            SocketData.SendMessage(socket, SocketData.OkCode);

            string[] cols = SocketData.ReceiveMessage(socket).Split(',');

            if (users.IsUserRegistered(cols[0], cols[1]))
            {
                if (users.SetPremium(cols[0], cols[1]))
                {
                    string? ip = null;

                    if (socket.RemoteEndPoint is IPEndPoint endPoint)
                    {
                        ip = Convert.ToString(endPoint.Address);
                    }

                    string msg = $"{users.GetAvailableDownloads(ip, cols[0], cols[1])}||{users.GetPremiumValidUntil(cols[0], cols[1])}";
                    SocketData.SendMessage(socket, msg);
                }
            }
            else
            {
                SocketData.SendMessage(socket, SocketData.FailCode);
            }

        }

        /// <summary>
        /// Authorizate a client
        /// </summary>
        /// <param name="socket">Socket of the client</param>
        private void AuthorizateClient(Socket socket)
        {
            SocketData.SendMessage(socket, SocketData.OkCode);

            string[] cols = SocketData.ReceiveMessage(socket).Split(',');

            if (users.IsUserRegistered(cols[0], cols[1]))
            {
                string? ip = null;

                if (socket.RemoteEndPoint is IPEndPoint endPoint)
                {
                    ip = Convert.ToString(endPoint.Address);
                }
                string msg = $"{users.GetAvailableDownloads(ip, cols[0], cols[1])}||{users.GetPremiumValidUntil(cols[0], cols[1])}";
                SocketData.SendMessage(socket, msg);
            }
            else
            {
                SocketData.SendMessage(socket, SocketData.FailCode);
            }

        }

        /// <summary>
        /// Register a new client
        /// </summary>
        /// <param name="socket">Socket of the client</param>
        private void RegisterClient(Socket socket)
        {
            SocketData.SendMessage(socket, SocketData.OkCode);

            string[] cols = SocketData.ReceiveMessage(socket).Split(',');

            string? ip = null;

            if (socket.RemoteEndPoint is IPEndPoint endPoint)
            {
                ip = Convert.ToString(endPoint.Address);
            }

            if (users.RegisterUser(ip, cols[0], cols[1]))
            {
                string msg = $"{users.GetAvailableDownloads(ip, cols[0], cols[1])}||{users.GetPremiumValidUntil(cols[0], cols[1])}";
                SocketData.SendMessage(socket, msg);
            }
            else
            {
                SocketData.SendMessage(socket, SocketData.FailCode);
            }
        }

        /// <summary>
        /// Receive a file
        /// </summary>
        /// <param name="socket">Socket to receive file from</param>
        private void ReceiveFile(Socket? socket)
        {
            if(socket == null)
            {
                return;
            }

            try
            {
                string path = SocketData.UploadedFilesPath;
                SocketData.SendMessage(socket, SocketData.OkCode);
                FileInfo file = SocketData.ReceiveFile(socket, path);
                SocketData.SendMessage(socket, Crypto.Encrypt(file.Name));

                string? owner = null;

                if(socket.RemoteEndPoint is IPEndPoint endPoint)
                {
                    owner = Convert.ToString(endPoint.Address);
                }

                Files?.Add(new FileModel()
                {
                    FilePath = file.FullName,
                    FileName = file.Name,
                    AddingTime = file.CreationTime,
                    DeadLine = file.CreationTime.Add(GlobalSettings.DefaultLivingTime),
                    DownloadedBy = owner,
                    Size = file.Length
                }); ;

                FilesChanged?.Invoke();
            }
            catch (Exception ex)
            {
                OnGotError($"[{DateTime.Now}] {ex.Message}");
            }
        }

        /// <summary>
        /// Send a file
        /// </summary>
        /// <param name="socket">Scocket to send file to</param>
        /// <returns>True if no error during sending a file otherwise false</returns>
        private bool SendFile(Socket? socket)
        {
            bool success = false;
            try
            {
                SocketData.SendMessage(socket, SocketData.OkCode);

                string fileName = Crypto.Decrypt(SocketData.ReceiveMessage(socket));

                if (File.Exists(Path.Combine(SocketData.UploadedFilesPath, fileName)))
                {
                    SocketData.SendMessage(socket, SocketData.OkCode);
                    SocketData.ReceiveMessage(socket);
                    SocketData.SendFile(socket, Path.Combine(SocketData.UploadedFilesPath, fileName));
                    success = true;
                }
                else
                {
                    SocketData.SendMessage(socket, SocketData.FailCode);
                }
            }
            catch (Exception ex)
            {
                OnGotError($"[{DateTime.Now}] {ex.Message}");
            }

            return success;
        }

        /// <summary>
        /// Check if files at the list still exist
        /// </summary>
        private void CheckFiles()
        {
            for (int i = 0; i < Files?.Count; i++)
            {
                if (!File.Exists(Files[i].FilePath))
                {
                    Files.Remove(Files[i]);
                    i--;
                }
            }
        }

        /// <summary>
        /// Load the list of the uploaded files
        /// </summary>
        private void LoadFiles()
        {
            if (File.Exists(GlobalSettings.FilesListPath))
            {
                try
                {
                    string files = File.ReadAllText(GlobalSettings.FilesListPath);
                    Files = JsonSerializer.Deserialize<ObservableCollection<FileModel>>(files);
                }
                catch { }
            }
        }

        /// <summary>
        /// Save the list of the uploaded files
        /// </summary>
        private void SaveFiles()
        {
            if (!File.Exists(GlobalSettings.FilesListPath))
            {
                File.Create(GlobalSettings.FilesListPath);
            }

            try
            {
                if (Files != null)
                {
                    string files = JsonSerializer.Serialize<ObservableCollection<FileModel>>(Files);
                    File.WriteAllText(GlobalSettings.FilesListPath, files);
                }
            }
            catch { }
        }

        /// <summary>
        /// Refresh list of files at the UploadedFilesPath
        /// </summary>
        private void RefreshListOfFiles()
        {
            bool isChanged = false;
            while (true)
            {
                for (int i = 0; i < Files?.Count; i++)
                {
                    if (Files[i].DeadLine < DateTime.Now)
                    {
                        try
                        {
                            if (Files[i].FilePath != null)
                            {
                                File.Delete(Files[i].FilePath!);
                            }
                            Files.Remove(Files[i]);
                            i--;
                        }
                        catch { }

                        isChanged = true;
                    }
                }

                if(isChanged)
                {
                    FilesChanged?.Invoke();
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Check if day is changed
        /// </summary>
        private void CheckNewDay()
        {
            while (true)
            {
                if (DateTime.Now.DayOfYear != today)
                {
                    today = DateTime.Now.DayOfYear;
                    users.ResetDownloads();
                }
                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="file">File to delete</param>
        public void RemoveFile(FileModel? file)
        {
            if(file != null && file.FilePath != null)
            {
                try
                {
                    File.Delete(file.FilePath);
                    Files?.Remove(file);
                    FilesChanged?.Invoke();
                }
                catch { }
            }
        }

        /// <summary>
        /// Close all connections
        /// </summary>
        public void Close()
        {
            listener?.Stop();
            users.SaveUsers();
            SaveFiles();
        }

        /// <summary>
        /// Raise an event GotError
        /// </summary>
        /// <param name="message">Error message</param>
        public void OnGotError(string message)
        {
            GotError?.Invoke(message);
        }

        /// <summary>
        /// Add program to the autorun
        /// </summary>
        private void AddToAutoRun()
        {
            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            try
            {
                string path = Environment.CurrentDirectory;
                string name = "FTPServer";

                registryKey?.SetValue(name, Path.Combine(path, name));
            }
            catch { }
        }
    }
}
