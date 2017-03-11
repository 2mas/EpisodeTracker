using System;

namespace EpisodeTracker.Exceptions
{
    [Serializable]
    public class ApiCredentialException : FormatException
    {
        public ApiCredentialException(string message) : base(message)
        {
        }

        public ApiCredentialException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
