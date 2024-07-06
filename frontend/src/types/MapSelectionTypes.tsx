import { LatLng } from "leaflet";
import { MultiPolygon } from "geojson";

// Define PolygonSelection class
export class PolygonSelection {
  polygon: MultiPolygon;
  displayName: string;
  ifHandSelected: boolean;

  constructor(
    polygon: MultiPolygon,
    displayName: string,
    ifHandSelected: boolean
  ) {
    this.polygon = polygon;
    this.displayName = displayName;
    this.ifHandSelected = ifHandSelected;
  }
}

// An interface for a single marker selection
export class MarkerSelection {
  marker: LatLng;
  displayName: string;
  ifHandSelected: boolean;

  constructor(marker: LatLng, displayName: string, ifHandSelected: boolean) {
    this.marker = marker;
    this.displayName = displayName;
    this.ifHandSelected = ifHandSelected;
  }
}
