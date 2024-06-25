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
  tables: [];
}
