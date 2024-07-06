import L from "leaflet";

export const modifySvg = (svgString: string, size: number = 30) => {
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
};

export const mergeIcons = (
  bigIcon: string,
  smallIcon: string,
  sizeBig: number = 32,
  sizeSmall: number = 10,
  smallIconTranslationX: number = 16,
  smallIconTranslationY: number = 16
) => {
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
};

export const pinSvg =
  '<svg width="32" height="32" fill="#000000" viewBox="0 0 256 256" version="1.1" id="svg1" xmlns="http://www.w3.org/2000/svg" xmlns:svg="http://www.w3.org/2000/svg">  <defs id="defs1" />  <path d="m 128,16 a 88.1,88.1 0 0 0 -88,88 c 0,75.3 80,132.17 83.41,134.55 a 8,8 0 0 0 9.18,0 C 136,236.17 216,179.3 216,104 A 88.1,88.1 0 0 0 128,16 Z m 0,56 a 32,32 0 1 1 -32,32 32,32 0 0 1 32,-32 z" id="path1" />  <ellipse style="fill:#ffffff;stroke:#000000;stroke-width:0;stroke-dasharray:none;stroke-opacity:1" id="path12" cx="128.39645" cy="104.18782" rx="81.751793" ry="81.967773" /></svg>';

export const createDivIcon = (iconSvgString: string) => {
  const combinedSvg = mergeIcons(pinSvg, iconSvgString, 40, 16, 12, 6);

  return L.divIcon({
    html: combinedSvg,
    className: "", // Optional: add a custom class name
    iconSize: [40, 40],
    iconAnchor: [20, 20], // Adjust the anchor point as needed
  });
};
