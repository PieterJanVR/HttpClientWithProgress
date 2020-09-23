using System;

namespace HttpClientWithProgress.Models
{
    public class HttpProgress
    {
        public long BytesDownloaded { get; }
        public long? BytesToDownload { get; }
        public DateTime DownloadStartedDateTime { get; }
        public DateTime DateTime { get; }
        
        public bool IsDone { get; }

        public double PercentageDone => BytesToDownload == null ? 0.0 : (BytesDownloaded / (double)BytesToDownload) * 100;

        public TimeSpan DownloadingTimeSpan => DateTime - DownloadStartedDateTime;

        public double TransferSpeedInBytesPerSecond => Math.Round(BytesDownloaded / DownloadingTimeSpan.TotalSeconds);
        public double TransferSpeedInKBPerSecond => TransferSpeedInBytesPerSecond / 1024;
        public double TransferSpeedInMBPerSecond => TransferSpeedInBytesPerSecond / 1024 / 1024;

        public TimeSpan? ETATimeSpan
        {
            get
            {
                if (BytesToDownload == null) return null;
                return TimeSpan.FromSeconds((BytesToDownload - BytesDownloaded) / TransferSpeedInBytesPerSecond ?? 0);
            }
        }

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