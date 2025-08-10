using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace varkit_aref
{
    public class ArefConfig : BasePluginConfig
    {
        [JsonPropertyName("Prefix")]
        public string Prefix { get; set; } = "{blue}⌈ varkit ⌋";

        [JsonPropertyName("ArefYetki")]
        public List<string> ArefYetki { get; set; } = new List<string>() {
            "@css/generic"
        };
    }
}
