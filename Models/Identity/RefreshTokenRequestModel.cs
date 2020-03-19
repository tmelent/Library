using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.Identity
{
    public class RefreshTokenRequestModel
    {
        [JsonProperty("oldrefreshtoken")]
        public string OldRefreshToken { get; set; }
        [JsonProperty("oldaccesstoken")]
        public string OldAccessToken { get; set; }
    }
}
