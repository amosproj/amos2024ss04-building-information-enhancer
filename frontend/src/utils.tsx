// These values will be replaced after build with the .sh script when spinning up docker container.
const currentEnvironment = {
  apiGatewayHost: "API_GATEWAY_HOST",
  apiGatewayPort: "API_GATEWAY_PORT",
};

// Returns the API Gateway URL for a specific deployment environment
// The .join() function ensures that this strings will not be replace by the .sh script.
export const getAPIGatewayURL = (): string => {
  return (
    "http://" +
    (currentEnvironment.apiGatewayHost === ["API_", "GATEWAY_", "HOST"].join()
      ? currentEnvironment.apiGatewayHost
      : "localhost") +
    ":" +
    (currentEnvironment.apiGatewayPort === ["API_", "GATEWAY_", "PORT"].join()
      ? currentEnvironment.apiGatewayPort
      : "8081")
  );
};
