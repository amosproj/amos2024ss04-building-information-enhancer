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
