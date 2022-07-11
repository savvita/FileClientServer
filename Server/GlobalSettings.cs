using System;

namespace Server
{
    internal static class GlobalSettings
    {
        /// <summary>
        /// Path to the file with info about files at the server
        /// </summary>
        internal static string FilesListPath { get; set; } = "files.json";

        /// <summary>
        /// Path to the file with info about registered users
        /// </summary>
        internal static string UsersListPath { get; set; } = "users.json";

        /// <summary>
        /// Default living time for each downloaded file
        /// </summary>
        internal static TimeSpan DefaultLivingTime { get; set; } = new TimeSpan(7, 0, 0, 0);

        /// <summary>
        /// Default living time for premium access
        /// </summary>
        internal static TimeSpan DefaultPremiumLivingTime { get; set; } = new TimeSpan(30, 0, 0, 0);

        /// <summary>
        /// Default count of the downloads
        /// </summary>
        internal static int DefaultDownloadsCount { get; set; } = 5;

        /// <summary>
        /// Default count of the downloads for the premium users
        /// </summary>
        internal static int DefaultPremiumDownloadsCount { get; set; } = 50;
    }
}
