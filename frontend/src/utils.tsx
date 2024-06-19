// These values will be replaced after build with the .sh script when spinning up docker container.
const currentEnvironment = {
  apiGatewayHost: "API_GATEWAY_HOST",
  apiGatewayPort: "API_GATEWAY_PORT",
};

// Returns the API Gateway URL for a specific deployment environment
// The .join() function ensures that this strings will not be replace by the .sh script.
export const getAPIGatewayURL = (): string => {
  console.log(
    `Read APIGateway URL: http://${currentEnvironment.apiGatewayHost}:${currentEnvironment.apiGatewayPort}`
  );
  console.log(
    "Final APIGateway URL: " +
      "http://" +
      (import.meta.env.DEV ? "localhost" : currentEnvironment.apiGatewayHost) +
      ":" +
      (import.meta.env.DEV ? "8081" : currentEnvironment.apiGatewayPort)
  );
  return (
    "http://" +
    (import.meta.env.DEV ? "localhost" : currentEnvironment.apiGatewayHost) +
    ":" +
    (import.meta.env.DEV ? "8081" : currentEnvironment.apiGatewayPort)
  );
};

export function modifySvg(svgString: string, size: number = 30) {
  const svgElement = new DOMParser()
    .parseFromString(svgString, "image/svg+xml")
    .querySelector("svg");

  if (svgElement) {
    svgElement.setAttribute("width", size.toString() + "px");
    svgElement.setAttribute("height", size.toString() + "px");
    if (!svgElement.getAttribute("viewBox")) {
      svgElement.setAttribute(
        "viewBox",
        "0 0 " + size.toString() + " " + size.toString()
      );
    }
  }
  return svgElement?.outerHTML || "";
}

export function mergeIcons(
  bigIcon: string,
  smallIcon: string,
  sizeBig: number = 32,
  sizeSmall: number = 10,
  smallIconTranslationX: number = 16,
  smallIconTranslationY: number = 16
) {
  const bigIconSvg = modifySvg(bigIcon, sizeBig);
  const smallIconSvg = modifySvg(smallIcon, sizeSmall);

  // Wrap the SVGs in a parent div with relative positioning
  const combinedHtml = `
    <div style="position: relative; width: ${sizeBig}px; height: ${sizeBig}px;">
      <div style="position: absolute; top: 0; left: 0;">
        ${bigIconSvg}
      </div>
      <div style="position: absolute; top: ${smallIconTranslationY}px; left: ${smallIconTranslationX}px;">
        ${smallIconSvg}
      </div>
    </div>
  `;

  return combinedHtml;
}
