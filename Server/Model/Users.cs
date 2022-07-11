using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.Model
{
    public class Users
    {
        private List<UserModel>? users = new List<UserModel>();

        public Users()
        {
            LoadUsers();
        }

        /// <summary>
        /// Load the list of the users
        /// </summary>
        public void LoadUsers()
        {
            if (File.Exists(GlobalSettings.UsersListPath))
            {
                try
                {
                    string _users = File.ReadAllText(GlobalSettings.UsersListPath);
                    users = JsonSerializer.Deserialize<List<UserModel>>(_users);
                }
                catch { }
            }
        }

        /// <summary>
        /// Save the list of the users
        /// </summary>
        public void SaveUsers()
        {
            if (!File.Exists(GlobalSettings.UsersListPath))
            {
                File.Create(GlobalSettings.UsersListPath);
            }

            try
            {
                if (users != null)
                {
                    string _users = JsonSerializer.Serialize<List<UserModel>>(users);
                    File.WriteAllText(GlobalSettings.UsersListPath, _users);
                }
            }
            catch { }
        }

        private UserModel AddToTheList(string? ip, string? login, string? password, TimeSpan? premiumAccess = null)
        {
            UserModel model = new UserModel()
            {
                Ip = ip, 
                Login = login,
                Password = password,
                CurrentDownloadsCount = 0,
                MaxDownloadsCount = GlobalSettings.DefaultDownloadsCount
            };

            if(premiumAccess != null)
            {
                model.PremiumValidUntil = DateTime.Now.Add((TimeSpan)premiumAccess);
                model.MaxDownloadsCount = GlobalSettings.DefaultPremiumDownloadsCount;
            }

            users?.Add(model);

            return model;
        }

        /// <summary>
        /// Check if the user is registered
        /// </summary>
        /// <param name="ip">IP of the user</param>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <returns>True if the user is registered otherwise false</returns>
        public bool IsUserRegistered(string login, string password)
        {
            if (users == null)
            {
                return false;
            }

            return users.Any(x => x.Login == login && x.Password == password);
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="ip">IP of the user</param>
        /// <param name="login">Login of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>True if a new user is registred otherwise false</returns>
        public bool RegisterUser(string? ip, string login, string password)
        {
            if(ip == null)
            {
                return false;
            }

            if(users == null)
            {
                return false;
            }

            if(users.Any(x => x.Login != null && x.Login.Equals(login)))
            {
                return false;
            }

            AddToTheList(ip, login, password);
            return true;
        }

        /// <summary>
        /// Get count of available downloads
        /// </summary>
        /// <param name="ip">IP of the user</param>
        /// <param name="login">Login of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Count of available downloads</returns>
        public int GetAvailableDownloads(string? ip, string? login = null, string? password = null)
        {
            if(ip == null && login == null)
            {
                return 0;
            }

            UserModel? model;

            if (login != null)
            {
                model = users?.Where(x => x.Login != null && x.Login.Equals(login)).FirstOrDefault();

                if (model != null)
                {
                    if (DateTime.Now > model.PremiumValidUntil)
                    {
                        model.MaxDownloadsCount = GlobalSettings.DefaultDownloadsCount;
                    }

                    return model.MaxDownloadsCount - model.CurrentDownloadsCount;
                }
            }

            model = users?.Where(x => x.Login == null && x.Ip != null && x.Ip.Equals(ip)).FirstOrDefault();

            if (model == null)
            {
                model = AddToTheList(ip, login, password);
            }

            return model.MaxDownloadsCount - model.CurrentDownloadsCount;
        }

        /// <summary>
        /// Decrease a number of available downloads
        /// </summary>
        /// <param name="ip">Ip of the connection</param>
        /// <param name="login">Login of the user</param>
        public void DecreaseAvailableDownloads(string? ip, string? login = null)
        {
            if(ip == null && login == null)
            {
                return;
            }

            UserModel? model;

            if (login != null)
            {
                model = users?.Where(x => x.Login != null && x.Login.Equals(login)).FirstOrDefault();

                if (model != null)
                {
                    model.CurrentDownloadsCount++;
                    return;
                }
            }

            model = users?.Where(x => x.Login == null && x.Ip != null && x.Ip.Equals(ip)).FirstOrDefault();

            if (model == null)
            {
                return;
            }

            model.CurrentDownloadsCount++;
        }

        /// <summary>
        /// Reset all counts of the current downloads
        /// </summary>
        public void ResetDownloads()
        {
            for (int i = 0; i < users?.Count; i++)
            {
                users[i].CurrentDownloadsCount = 0;
            }
        }

        /// <summary>
        /// Get deadline of the premium access of the user 
        /// </summary>
        /// <param name="login">Login of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Daedline of the premium access</returns>
        public DateTime GetPremiumValidUntil(string login, string password)
        {
            UserModel? model = users?.Where(x => x.Login == login && x.Password == password).FirstOrDefault();

            if (model != null && model.PremiumValidUntil != null)
            {           
                return (DateTime)model.PremiumValidUntil;
            }

            return new DateTime();
        }

        /// <summary>
        /// Set premium access to the user
        /// </summary>
        /// <param name="login">Login of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>True if premium access is setted otherwise false</returns>
        public bool SetPremium(string login, string password)
        {
            UserModel? model = users?.Where(x => x.Login == login && x.Password == password).FirstOrDefault();

            if(model != null)
            {
                if(model.PremiumValidUntil != null && model.PremiumValidUntil > DateTime.Now)
                {
                    model.PremiumValidUntil = ((DateTime)model.PremiumValidUntil).Add(GlobalSettings.DefaultPremiumLivingTime);
                }
                else
                {
                    model.PremiumValidUntil = DateTime.Now.Add(GlobalSettings.DefaultPremiumLivingTime);
                }

                model.MaxDownloadsCount = GlobalSettings.DefaultPremiumDownloadsCount;
                return true;
            }

            return false;
        }
    }
}
