import { FeatureCollection } from "geojson";
import { MarkersTypes } from "./MarkersTypes";
import { LatLngBounds } from "leaflet";

/**
 * Type corresponding to one dataset.
 */
export type Dataset = {
  id: string;
  displayName: string;
  shortDescription: string;
  datasetIcon: JSX.Element;
  data: FeatureCollection;
  lastDataRequestBounds: LatLngBounds;
  metaData: DatasetMetaData | undefined;
};

/**
 * Type for the basicData from the metadata database.
 */
export interface DatasetBasicData {
  datasetId: string;
  name: string;
  shortDescription: string;
  icon: string;
}

/**
 * Type of the additionalData from the metadata database.
 */
export interface DatasetMetaData {
  icon: string;
  type: MarkersTypes;
  longDescription: string;
  minZoomLevel: number;
  markersThreshold: number;
  displayProperty: DisplayProperty[];
  tables: Table[];
  polygonColoring: PolygonColoring | null;
}

/**
 * Display property type used for the marker popups.
 */
export interface DisplayProperty {
  displayName: string;
  value: string;
}

/**
 * Table type for storing the number of ingested lines for each dataset.
 */
export interface Table {
  name: string;
  numberOfLines: number;
}

/**
 * The map of types of colors for the polygons.
 */
export interface PolygonColoring {
  attributeName: string;
  colors: PolygonColor[];
}

/**
 * Individual entry in the color map.
 */
export interface PolygonColor {
  color: string;
  values: string[];
}
