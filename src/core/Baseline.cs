using System.Text.Json.Serialization;

namespace yadd.core
{
    public class Baseline
    {
        public BaselineId Id { get; set; }
        public BaselineId ParentId { get; set; }
        public InformationSchema InformationSchema { get; set; }
    }
}
