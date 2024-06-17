namespace APIGateway.Models
{
    public class DatasetListResponse
    {
        public List<DatasetBasicData> datasetBasicDataList { get; set; }

    }

    public class DatasetBasicData
    {
        public string DatasetId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}
