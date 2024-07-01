import axios from "axios";
import { DatasetMetaData } from "../types/DatasetTypes";

/**
 * These values will be replaced after build with the .sh script when spinning up docker container.
 */
const currentEnvironment = {
  apiGatewayHost: "API_GATEWAY_HOST",
  apiGatewayPort: "API_GATEWAY_PORT",
};

/**
 * Returns the API Gateway URL for a specific deployment environment.
 * The .join() function ensures that this strings will not be replace by the .sh script.
 */
export const getAPIGatewayURL = (): string => {
  return (
    "http://" +
    (import.meta.env.DEV ? "localhost" : currentEnvironment.apiGatewayHost) +
    ":" +
    (import.meta.env.DEV ? "8081" : currentEnvironment.apiGatewayPort)
  );
};

/**
 * Fetches the metadata for a specific dataset from the metadata database.
 * @param datasetID the dataset of which the metadata will be fetched
 * @returns fetched metadata
 */
export const fetchMetadataForDataset = async (
  datasetID: string
): Promise<DatasetMetaData> => {
  // Prepare the parameters
  const params = {
    datasetID: datasetID,
  };
  // Make the API call
  const response = await axios.get<DatasetMetaData>(
    getAPIGatewayURL() + "/api/getDatasetMetadata/",
    {
      params,
    }
  );
  // Return the metadata
  return response.data;
};
