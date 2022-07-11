using System;

namespace Server.Model
{
    public class UserModel
    {
        /// <summary>
        /// Ip address of the connected client
        /// </summary>
        public string? Ip { get; set; }

        /// <summary>
        /// Login of the user
        /// </summary>
        public string? Login { get; set; }

        /// <summary>
        /// Password of the user
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Living time of the premium access
        /// </summary>
        public DateTime? PremiumValidUntil { get; set; }

        /// <summary>
        /// Count of the downloads at today
        /// </summary>
        public int CurrentDownloadsCount { get; set; }

        /// <summary>
        /// Max count of the downloads per day
        /// </summary>
        public int MaxDownloadsCount { get; set; }
    }
}
