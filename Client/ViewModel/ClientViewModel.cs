using Client.Model;
using Client.View;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModel
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        public ClientModel Client { get; } = new ClientModel();

        /// <summary>
        /// Path to the file to upload
        /// </summary>
        private string? filePath;
        public string? FilePath
        {
            get => filePath;
            private set
            {
                filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        /// <summary>
        /// Id of the file to upload to the server
        /// </summary>
        private string? fileId;
        public string? FileID
        {
            get => fileId;
            set
            {
                fileId = value;
                OnPropertyChanged(nameof(FileID));
            }
        }

        /// <summary>
        /// Path to the folder to save a file
        /// </summary>
        private string? downloadFolder;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Commands
        private readonly RelayCommand? openCommand;

        public RelayCommand OpenCommand
        {
            get => openCommand ?? new RelayCommand(() => OpenFile());
        }

        private readonly RelayCommand? sendCommand;

        public RelayCommand SendCommand
        {
            get => sendCommand ?? new RelayCommand(() => Task.Factory.StartNew(SendFile));
        }

        private readonly RelayCommand<object>? downloadCommand;

        public RelayCommand<object> DownloadCommand
        {
            get => downloadCommand ?? new RelayCommand<object>((obj) =>
            {
                if (ChoseFolder() && downloadFolder != null)
                {
                    Task.Factory.StartNew(ReceiveFile, obj);
                }
            });
        }

        private readonly RelayCommand? copyCommand;

        public RelayCommand CopyCommand
        {
            get => copyCommand ?? new RelayCommand(() =>
            {
                if (FileID != null)
                {
                    Clipboard.SetData(DataFormats.Text, (Object?)FileID);
                }
            });
        }

        private readonly RelayCommand? authorizationCommand;

        public RelayCommand AuthorizationCommand
        {
            get => authorizationCommand ?? new RelayCommand(() =>
            {
                AuthorizationView view = new AuthorizationView(Client);

                view.ShowDialog();

            });
        }

        private readonly RelayCommand? buyPremiumCommand;

        public RelayCommand BuyPremiumCommand
        {
            get => buyPremiumCommand ?? new RelayCommand(() =>
            {
                BuyPremiumView view = new BuyPremiumView(Client);

                view.ShowDialog();

            });
        }

        private readonly RelayCommand? logOutCommand;

        public RelayCommand LogOutCommand
        {
            get => logOutCommand ?? new RelayCommand(() =>
            {
                Client.LogOut();
            });
        }

        #endregion


        /// <summary>
        /// Show open file dialog
        /// </summary>
        private void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }

        /// <summary>
        /// Upload file to the server
        /// </summary>
        /// <returns>True if upload is successful otherwise false</returns>
        private bool SendFile()
        {
            bool successed = Client.SendFile(FilePath, out string id);

            if (successed)
            {
                FileID = id;
            }

            return successed;
        }

        /// <summary>
        /// Download a file from the server
        /// </summary>
        /// <param name="obj">Id of file to download</param>
        /// <returns>True if file exists on the server and is downloaded otherwise false</returns>
        private bool ReceiveFile(object? obj)
        {
            bool result = false;

            if (obj is string id)
            {
                result = Client.ReceiveFile(id, downloadFolder!);
            }

            if(result)
            {
                MessageBox.Show("Download complete!", "Download complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Cannot download the file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return result;
        }

        /// <summary>
        /// Open chose folder dialog
        /// </summary>
        /// <returns>True if dialog is accepter otherwise false</returns>
        private bool ChoseFolder()
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Save file",
                IsFolderPicker = true,
                InitialDirectory = Environment.CurrentDirectory,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = Environment.CurrentDirectory,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                downloadFolder = dlg.FileName;
                return true;
            }
            else
            {
                downloadFolder = null;
                return false;
            }
        }
    }
}
