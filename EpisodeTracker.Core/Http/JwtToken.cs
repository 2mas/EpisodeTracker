using System;

namespace EpisodeTracker.Http
{
    public class JwtToken
    {
        public string Token { get; set; }

        public DateTime Expiration { get; set; }

        public bool IsValid()
        {
            return Expiration > DateTime.Now && !String.IsNullOrEmpty(Token);
        }
    }
}
