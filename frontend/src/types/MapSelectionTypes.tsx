import { LatLng } from "leaflet";
import { Feature, Polygon } from "geojson";

// An interface for the map selection

export type MapSelection = PolygonSelection | MarkerSelection | null;

// Define PolygonSelection class
export class PolygonSelection {
  polygon: Feature<Polygon>;
  displayName: string;
  ifHandDrawn: boolean;

  constructor(
    polygon: Feature<Polygon>,
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
