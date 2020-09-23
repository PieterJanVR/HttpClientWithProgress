using System;

namespace HttpClientWithProgress.Models
{
    public class HttpProgress
    {
        /// <summary>
        /// The number of bytes that have already been downloaded
        /// </summary>
        public long BytesDownloaded { get; }
        
        /// <summary>
        /// The number of bytes to download in total if the Content-Length was passed by the initial response header. `null` if not available.
        /// </summary>
        public long? BytesToDownload { get; }
        
        /// <summary>
        /// A Timestamp of when the downloading started
        /// </summary>
        public DateTime DownloadStartedDateTime { get; }
        
        /// <summary>
        /// A Timestamp of the current progress update
        /// </summary>
        public DateTime DateTime { get; }
        
        /// <summary>
        /// Whether or not the download is finished (will only be true with the last progress update) 
        /// </summary>
        public bool IsDone { get; }

        /// <summary>
        /// The percentage that is already downloaded. Will only be available if the Content-Length is passed by the initial response header. If not, will remain 0%
        /// </summary>
        public double PercentageDone => BytesToDownload == null ? 0.0 : (BytesDownloaded / (double)BytesToDownload) * 100;

        /// <summary>
        /// A time length of how long the download is already downloading
        /// </summary>
        public TimeSpan DownloadingTimeSpan => DateTime - DownloadStartedDateTime;

        /// <summary>
        /// The current transfer speed of the download in bytes per second (B/s)
        /// </summary>
        public double TransferSpeedInBytesPerSecond => Math.Round(BytesDownloaded / DownloadingTimeSpan.TotalSeconds);
        
        /// <summary>
        /// The current transfer speed of the download in kilobytes per second (KB/s)
        /// </summary>
        public double TransferSpeedInKBPerSecond => TransferSpeedInBytesPerSecond / 1024;
        
        /// <summary>
        /// The current transfer speed of the download in megabytes per second (MB/s)
        /// </summary>
        public double TransferSpeedInMBPerSecond => TransferSpeedInBytesPerSecond / 1024 / 1024;

        /// <summary>
        /// The estimated time length the download will still need to finish
        /// </summary>
        public TimeSpan? ETATimeSpan
        {
            get
            {
                if (BytesToDownload == null) return null;
                return TimeSpan.FromSeconds((BytesToDownload - BytesDownloaded) / TransferSpeedInBytesPerSecond ?? 0);
            }
        }

        /// <summary>
        /// The estimated time stamp when the download will probably be finished
        /// </summary>
        public DateTime? ETADateTime => DateTime + ETATimeSpan;

        public HttpProgress(long bytesDownloaded, long? bytesToDownload, DateTime downloadStartedDateTime, bool isDone = false)
        {
            DateTime = DateTime.Now;
            
            if (downloadStartedDateTime > DateTime)
                throw new ArgumentException($"{nameof(downloadStartedDateTime)} can't be in the passed", nameof(downloadStartedDateTime));
            if (bytesDownloaded < 0)
                throw new ArgumentException($"{bytesDownloaded} can't be lower than 0", nameof(bytesDownloaded));
            if (bytesToDownload != null && bytesToDownload < 0)
                throw new ArgumentException($"{bytesToDownload} can't be lower than 0", nameof(bytesToDownload));
            
            BytesDownloaded = bytesDownloaded;
            BytesToDownload = bytesToDownload;
            DownloadStartedDateTime = downloadStartedDateTime;
            IsDone = isDone;
        }

        public override string ToString()
        {
            return this.ToString(nameof(HttpProgress));
        }

        public string ToString(string subject)
        {
            return $"[{subject}] {BytesDownloaded} / {(BytesToDownload.HasValue ? BytesToDownload.Value.ToString() : "?")} bytes ({Math.Round(PercentageDone, 2)} %) @ {Math.Round(TransferSpeedInKBPerSecond)} KB/s. {(ETATimeSpan.HasValue ? "ETA: " + Math.Round(ETATimeSpan.Value.TotalSeconds, 1) + " seconds " : "")}";
        }
    }
}