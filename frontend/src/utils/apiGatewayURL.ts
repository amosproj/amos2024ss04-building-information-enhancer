/**
 * These values will be replaced after build with the .sh script when spinning up docker container.
 */
const currentEnvironment = {
  apiGatewayHost: "API_GATEWAY_HOST",
  apiGatewayPort: "API_GATEWAY_PORT",
};

/**
 * Returns the API Gateway URL for a specific deployment environment.
 * The .join() function ensures that this strings will not be replaced by the .sh script.
 */
export const getAPIGatewayURL = (): string => {
  return (
    "http://" +
    (import.meta.env.DEV ? "localhost" : currentEnvironment.apiGatewayHost) +
    ":" +
    (import.meta.env.DEV ? "8081" : currentEnvironment.apiGatewayPort)
  );
};
