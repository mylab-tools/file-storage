namespace MyLab.FileStorage.Cleaner
{
    public class CleanerOptions
    {
        /// <summary>
        /// The base directory for files
        /// </summary>
        /// <remarks>
        /// '/var/fs/data' by default
        /// </remarks>
        public string Directory { get; set; } = "/var/fs/data";

        /// <summary>
        /// Defines time to live for unconfirmed files in hours
        /// </summary>
        /// <remarks>
        /// 12 hours by default
        /// </remarks>
        public int LostFileTtlHours { get; set; } = 12;

        /// <summary>
        /// Defines time to live for stored files in hours
        /// </summary>
        /// <remarks>
        /// Unlimited for default
        /// </remarks>
        public int? StoredFileTtlHours { get; set; }
    }
}
