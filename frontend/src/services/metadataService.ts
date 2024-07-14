import axios from "axios";
import { DatasetMetaData } from "../types/DatasetTypes";
import { getAPIGatewayURL } from "../utils/apiGatewayURL";

/**
 * Fetches the metadata for a specific dataset from the metadata database.
 * @param datasetID the dataset of which the metadata will be fetched
 * @returns fetched metadata
 */
export const fetchMetadataForDataset = async (
  datasetID: string
): Promise<DatasetMetaData | undefined> => {
  // Prepare the parameters
  const params = {
    datasetID: datasetID,
  };
  try {
    // Make the API call
    const response = await axios.get<DatasetMetaData>(
      getAPIGatewayURL() + "/api/getDatasetMetadata/",
      {
        params,
      }
    );
    // Check the response status
    if (response.status === 200) {
      // Return the metadata
      return response.data;
    } else {
      // Log the error message and status code
      console.error(
        `Error fetching metadata, status code: ${response.status}, message: ${response.statusText}`
      );
      return undefined;
    }
  } catch (error) {
    // Console log the error
    if (axios.isAxiosError(error) && error.response) {
      console.error(
        `Error fetching metadata, status code: ${error.response.status}, message: ${error.response.statusText}`,
        error.response.data
      );
    } else if (axios.isAxiosError(error)) {
      console.error("Axios error fetching metadata:", error.message);
    } else {
      console.error("Unknown error fetching metadata", error);
    }
    return undefined;
  }
};
