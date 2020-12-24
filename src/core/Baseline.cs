using System.Text.Json.Serialization;

namespace yadd.core
{
    public class Baseline
    {
        [JsonIgnore]
        public BaselineId Id { get; set; }
        [JsonIgnore]
        public BaselineId ParentId { get; set; }
        public InformationSchema InformationSchema { get; set; }
    }
}
