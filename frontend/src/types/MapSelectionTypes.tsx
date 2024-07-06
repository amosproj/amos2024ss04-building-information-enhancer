import { LatLng } from "leaflet";
import { Feature, MultiPolygon } from "geojson";

// Define PolygonSelection class
export class PolygonSelection {
  polygon: Feature<MultiPolygon>;
  displayName: string;
  ifHandDrawn: boolean;

  constructor(
    polygon: Feature<MultiPolygon>,
    displayName: string,
    ifHandDrawn: boolean
  ) {
    this.polygon = polygon;
    this.displayName = displayName;
    this.ifHandDrawn = ifHandDrawn;
  }
}

// An interface for a single marker selection
export class MarkerSelection {
  marker: LatLng;

  constructor(marker: LatLng) {
    this.marker = marker;
  }
}
