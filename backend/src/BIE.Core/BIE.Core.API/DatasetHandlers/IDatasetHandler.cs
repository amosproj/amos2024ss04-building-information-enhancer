using BieMetadata;

namespace BIE.Core.API.DatasetHandlers;

public interface IDatasetHandler
{
    public string GetDataInsideArea(BoundingBox boundingBox);
}