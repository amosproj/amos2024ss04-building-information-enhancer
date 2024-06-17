namespace APIGateway.Models
{
    public class DatasetMetadata
    {
        public string Icon { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Zoom level is higher the closer you look at something. If current zoom level is below this, it shouldnt display any value
        /// </summary>
        public int MinZoomLevel { get; set; }

        /// <summary>
        /// The display property is the property that should be shown in a popup
        /// </summary>
        public string DisplayProperty { get; set; }

    }
}
