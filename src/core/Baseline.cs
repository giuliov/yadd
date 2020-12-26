using System;
using System.Text.Json.Serialization;

namespace yadd.core
{
    public class Baseline
    {
        [JsonIgnore]
        public BaselineId Id { get; set; }
        [JsonIgnore]
        public BaselineId ParentId { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
        public ServerVersionInfo ServerInfo { get; set; }
        public InformationSchema InformationSchema { get; set; }
    }
}
