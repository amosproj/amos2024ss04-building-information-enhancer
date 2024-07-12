import { LatLng } from "leaflet";
import { MultiPolygon } from "geojson";

/**
 * A map selection of a polygon. Either by manualy drawing it or by search bar.
 */
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

/**
 * A map selection of a single location/marker.
 */
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
