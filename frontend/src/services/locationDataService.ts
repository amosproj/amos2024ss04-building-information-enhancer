import axios from "axios";
import { LocationDataResponse } from "../types/LocationDataTypes";
import { getAPIGatewayURL } from "../utils/apiGatewayURL";

export const fetchLocationData = async (): Promise<
  LocationDataResponse | undefined
> => {
  const requestBody = {
    datasetId: "example_dataset",
    location: [
      { latitude: 51.509865, longitude: -0.118092 }, // Example coordinate
    ],
  };

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
