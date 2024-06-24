namespace APIGateway.Models
{
    public class DatasetBasicData
    {
        public string DatasetId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public List<TableData> Tables { get; set; }
    }

        public class TableData
    {
        public string Name { get; set; }
        public int NumberOfLines { get; set; }
    }
}
