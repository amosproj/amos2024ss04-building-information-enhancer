import axios from "axios";
import { LocationDataResponse } from "../types/LocationDataTypes";
import { getAPIGatewayURL } from "./metadataService";

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
    return response.data;
  } catch (error) {
    console.error("Error loading location data", error);
    return undefined;
  }
};
