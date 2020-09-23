using System;

namespace HttpClientWithProgress.Models
{
    public class ProgressException : Exception
    {
        public ProgressException(Exception innerException)
            : base($"An exception occured inside the progress delegate: '{innerException.Message}'. See inner exception for more details", innerException) { }
    }
}