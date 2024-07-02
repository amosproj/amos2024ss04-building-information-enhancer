import axios from "axios";
import { DatasetBasicData } from "../types/DatasetTypes";
import { getAPIGatewayURL } from "../utils/apiGatewayURL";

/**
 * Fetches the list of datasets from the server.
 * @returns fetched datasets
 */
export const fetchDatasets = async (): Promise<
  DatasetBasicData[] | undefined
> => {
  try {
    // Make the API call
    const response = await axios.get<DatasetBasicData[]>(
      getAPIGatewayURL() + "/api/getDatasetList"
    );
    // Check the response status
    if (response.status === 200) {
      // Return the datasets
      return response.data;
    } else {
      // Log the error message and status code
      console.error(
        `Error fetching datasets, status code: ${response.status}, message: ${response.statusText}`
      );
      return undefined;
    }
  } catch (error) {
    // Console log the error
    if (axios.isAxiosError(error) && error.response) {
      console.error(
        `Error fetching datasets, status code: ${error.response.status}, message: ${error.response.statusText}`,
        error.response.data
      );
    } else if (axios.isAxiosError(error)) {
      console.error("Axios error fetching datasets:", error.message);
    } else {
      console.error("Unknown error fetching datasets", error);
    }
    return undefined;
  }
};
