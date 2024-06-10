namespace APIGateway.Models
{
    public class DatasetListResponse
    {
        public List<DatasetBasicData> basicInfoList { get; set; }

    }

    public class DatasetBasicData
    {
        public string datasetId { get; set; }
        public string name { get; set; }
        public string description { get; set; }

    }
}
