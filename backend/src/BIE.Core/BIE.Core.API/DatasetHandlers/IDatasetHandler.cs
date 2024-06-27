namespace BIE.Core.API.DatasetHandlers;

public interface IDatasetHandler
{
    public string GetDataInsideArea(float minX, float minY, float maxX, float maxY);
}