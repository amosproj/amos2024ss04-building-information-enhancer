import axios from "axios";
import { FeatureCollection, Geometry } from "geojson";
import { LatLngBounds } from "leaflet";
import { getAPIGatewayURL } from "./metadataService";

/**
 * Fetches the viewport data for a specific dataset from the backend.
 * @param id the dataset ID
 * @param bounds the bounding box of the viewport
 * @param zoom the zoom level of the map
 * @returns fetched viewport data
 */
export const fetchViewportDataForDataset = async (
  id: string,
  bounds: LatLngBounds,
  zoom: number
): Promise<FeatureCollection<Geometry> | undefined> => {
  // Define the query parameters
  const params = {
    BottomLat: bounds.getSouthWest().lat,
    BottomLong: bounds.getSouthWest().lng,
    TopLat: bounds.getNorthEast().lat,
    TopLong: bounds.getNorthEast().lng,
    ZoomLevel: zoom,
    datasetID: id,
  };

  try {
    // Make the API call
    const response = await axios.get<FeatureCollection<Geometry>>(
      getAPIGatewayURL() + "/api/getDatasetViewportData",
      {
        params,
      }
    );
    // Return the viewport data
    return response.data;
  } catch (error) {
    // Console log the error
    if (axios.isAxiosError(error)) {
      console.error("Axios error fetching data:", error.message);
    } else {
      console.error("Unknown error fetching data:", error);
    }
  }
};
