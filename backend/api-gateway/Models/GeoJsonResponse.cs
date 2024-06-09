using System.Collections.Generic;

namespace api_gateway.Models
{
    public class GeoJsonResponse
    {
        public string type { get; set; }
        public List<Feature> features { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<float> coordinates { get; set; }
    }

    public class Properties
    {
        public string name { get; set; }
    }
}
