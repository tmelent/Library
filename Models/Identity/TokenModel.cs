using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Library.Models.Identity
{
    public class TokenModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("access_expiration_date")]
        public DateTimeOffset AccessExpirationDate { get; set; }
        [JsonProperty("refresh_token")]
        public RefreshToken RefreshToken { get; set; }

        [JsonProperty("httpStatusCode")]
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
