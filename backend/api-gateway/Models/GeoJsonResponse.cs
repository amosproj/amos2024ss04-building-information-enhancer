using System.Collections.Generic;

namespace APIGateway.Models
{

    public class GeoJsonResponse
    {
        public string type { get; set; }  = string.Empty;
        public List<Feature> features { get; set; } = new List<Feature>();
    }

    public class Feature
    {
        public string type { get; set; } = string.Empty;
        public Geometry geometry { get; set; } = new Geometry();
        public Properties properties { get; set; } = new Properties();
    }

    public class Geometry
    {
        public string type { get; set; } = string.Empty;
        public List<float> coordinates { get; set; } = new List<float>();
    }

    public class Properties
    {
        public string name { get; set; } = string.Empty;
    }
}
