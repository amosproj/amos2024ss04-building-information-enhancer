import { FeatureCollection } from "geojson";
import { MarkersTypes } from "./MarkersTypes";
import { LatLngBounds } from "leaflet";

// Dataset Type
export type Dataset = {
  id: string;
  displayName: string;
  shortDescription: string;
  type: MarkersTypes;
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
  type: string;
  longDescription: string;
  minZoomLevel: number;
  displayProperty: string;
  tables: Table[];
}

export interface Table {
  name: string;
  numberOfLines: number;
}