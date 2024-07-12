import { FeatureCollection } from "geojson";
import { MarkersTypes } from "./MarkersTypes";
import { LatLngBounds } from "leaflet";

// Dataset Type
export type Dataset = {
  id: string;
  displayName: string;
  shortDescription: string;
  datasetIcon: JSX.Element;
  data: FeatureCollection;
  lastDataRequestBounds: LatLngBounds;
  metaData: DatasetMetaData | undefined;
};

export interface DatasetBasicData {
  datasetId: string;
  name: string;
  shortDescription: string;
  icon: string;
}

export interface DatasetMetaData {
  icon: string;
  type: MarkersTypes;
  longDescription: string;
  minZoomLevel: number;
  markersThreshold: number;
  displayProperty: DisplayProperty[];
  tables: Table[];
}

export interface DisplayProperty {
  displayName: string;
  value: string;
}

export interface Table {
  name: string;
  numberOfLines: number;
}
