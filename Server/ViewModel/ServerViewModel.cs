using Communication;
using GalaSoft.MvvmLight.Command;
using Server.Model;
using Server.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Server.ViewModel
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        public ServerModel Server { get; } = new ServerModel();

        private readonly SynchronizationContext? context;

        /// <summary>
        /// True if server is working otherwise false
        /// </summary>
        private bool isServerStared;

        public bool IsServerStared
        {
            get => isServerStared;
        }

        /// <summary>
        /// True if server is NOT working otherwise false
        /// </summary>
        public bool IsServerStopped
        {
            get => !isServerStared;
        }

        /// <summary>
        /// List of uploaded files
        /// </summary>
        private ObservableCollection<FileModel>? files;
        public ObservableCollection<FileModel>? Files
        {
            get => files;

            set
            {
                files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        /// <summary>
        /// Currently selected file at the list
        /// </summary>
        public FileModel? SelectedFile { get; set; }

        public ServerViewModel()
        {
            context = SynchronizationContext.Current;

            RefreshListOfFiles(null);

            Server.FilesChanged += () => context?.Send(RefreshListOfFiles, null);

            StartCommand.Execute(null);
        }

        /// <summary>
        /// Refresh list of files at the UploadedFilesPath
        /// </summary>
        private void RefreshListOfFiles(object? obj)
        {
            Files?.Clear();

            if (Server.Files != null)
            {
                Files = new ObservableCollection<FileModel>(Server.Files);
            }
        }

        /// <summary>
        /// Set the living time of the file
        /// </summary>
        /// <param name="file">File to which is setting the living time</param>
        /// <param name="time">Living time to set</param>
        private void SetTime(FileModel? file, DateTime? date)
        {
            if(file == null)
            {
                return;
            }

            if(file != null && date != null && Server.Files != null)
            {
                int idx = Server.Files.IndexOf(file);

                if(idx != -1)
                {
                    Server.Files[idx].DeadLine = (DateTime) date;
                }

                RefreshListOfFiles(null);
            }
        }

        #region Commands

        private readonly RelayCommand? stopCommand;

        public RelayCommand StopCommand
        {
            get => stopCommand ?? new RelayCommand(() =>
            {
                isServerStared = false;
                NotifyServerWorkingChanged();
                Server.Close();
            });
        }

        private readonly RelayCommand? startCommand;

        public RelayCommand StartCommand
        {
            get => startCommand ?? new RelayCommand(() =>
            {
                isServerStared = true;
                NotifyServerWorkingChanged();
                Task.Factory.StartNew(Server.Listen);
            });
        }

        private readonly RelayCommand? setTimeCommand;

        public RelayCommand SetTimeCommand
        {
            get => setTimeCommand ?? new RelayCommand(() =>
            {
                SetDateView view = new SetDateView();

                view.ShowDialog();

                if(view.LiveTo != null)
                {
                    SetTime(SelectedFile, view.LiveTo);
                }
            });
        }

        private readonly RelayCommand? setAllTimeCommand;

        public RelayCommand SetAllTimeCommand
        {
            get => setAllTimeCommand ?? new RelayCommand(() =>
            {
                SetDateView view = new SetDateView();

                view.ShowDialog();

                if (view.LiveTo != null)
                {
                    for (int i = 0; i < Files?.Count; i++)
                    {
                        SetTime(Files[i], view.LiveTo);
                    }
                }
            });
        }

        private readonly RelayCommand? removeCommand;

        public RelayCommand RemoveCommand
        {
            get => removeCommand ?? new RelayCommand(() =>
            {
                if (SelectedFile != null)
                {
                    if(MessageBox.Show($"Delete file {SelectedFile.FileName}?", 
                        "Delete",MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Server.RemoveFile(SelectedFile);
                    }
                }
            });
        }

        #endregion

        private void NotifyServerWorkingChanged()
        {
            OnPropertyChanged(nameof(IsServerStared));
            OnPropertyChanged(nameof(IsServerStopped));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
