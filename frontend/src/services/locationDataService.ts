import axios from "axios";
import { LocationDataResponse } from "../types/LocationDataTypes";
import { getAPIGatewayURL } from "../utils/apiGatewayURL";
import { LatLng } from "leaflet";

/**
 * Fetches the data from a specific location
 * @param datasetId the dataset ID of the current map
 * @param location an array of coordinates
 * @returns
 */
export const fetchLocationData = async (
  datasetId: string,
  location: LatLng[]
): Promise<LocationDataResponse | undefined> => {
  // Build the request body
  const requestBody = {
    datasetId: datasetId,
    location: location,
  };
  console.log(requestBody);
  try {
    const response = await axios.put<LocationDataResponse>(
      getAPIGatewayURL() + "/api/loadLocationData",
      requestBody
    );

    if (response.status === 200) {
      return response.data;
    } else {
      console.error(
        `Error loading location data, status code: ${response.status}, message: ${response.statusText}`
      );
      return undefined;
    }
  } catch (error) {
    if (axios.isAxiosError(error) && error.response) {
      console.error(
        `Error loading location data, status code: ${error.response.status}, message: ${error.response.statusText}`,
        error.response.data
      );
    } else if (axios.isAxiosError(error)) {
      console.error("Axios error loading location data:", error.message);
    } else {
      console.error("Unknown error loading location data", error);
    }
    return undefined;
  }
};
