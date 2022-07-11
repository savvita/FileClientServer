using System;

namespace Server.Model
{
    public class FileModel
    {
        /// <summary>
        /// Name of the file
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Path to the file
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Date and time of adding this file to the server
        /// </summary>
        public DateTime AddingTime { get; set; }

        /// <summary>
        /// Date and time when this file will be deleted from the server
        /// </summary>
        public DateTime DeadLine { get; set; }

        /// <summary>
        /// User that downloaded the file
        /// </summary>
        public string? DownloadedBy { get; set; }

        /// <summary>
        /// Size of the file
        /// </summary>
        public long Size { get; set; }
    }
}
