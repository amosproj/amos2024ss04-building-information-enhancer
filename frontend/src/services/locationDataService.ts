import axios from "axios";
import { LocationDataResponse } from "../types/LocationDataTypes";
import { getAPIGatewayURL } from "../utils/apiGatewayURL";
import { Position } from "geojson";

/**
 * Fetches the data from a specific location
 * @param datasetId the dataset ID of the current map
 * @param location an array of coordinates
 * @returns
 */
export const fetchLocationData = async (
  datasetId: string,
  location: Position[][][] | Position[][]
): Promise<LocationDataResponse | undefined> => {
  // Build the request body
  const requestBody = {
    datasetId: datasetId,
    location: location,
  };
  try {
    // const response = await axios.put<LocationDataResponse>(
    //   getAPIGatewayURL() + "/api/loadLocationData",
    //   requestBody
    // );

    const response: LocationDataResponse = {
      currentDatasetData: [
        {
          key: "temperature",
          value: "22°C",
          mapId: "",
        },
        {
          key: "humidity",
          value: "45%",
          mapId: "",
        },
        {
          key: "windSpeed",
          value: "15 km/h",
          mapId: "",
        },
      ],
      generalData: [
        {
          key: "population",
          value: "1,000,000",
          mapId: "map004",
        },
        {
          key: "area",
          value: "500 sq km",
          mapId: "",
        },
        {
          key: "elevation",
          value: "300 meters",
          mapId: "",
        },
      ],
      extraRows: [
        {
          key: "timeZone",
          value: "GMT+2",
          mapId: "",
        },
        {
          key: "language",
          value: "English",
          mapId: "EV_charging_stations",
        },
        {
          key: "currency",
          value: "USD",
          mapId: "",
        },
      ],
    };
    return response;

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
