export interface DatasetBasicData {
  datasetId: string;
  name: string;
  description: string;
  icon: string;
}

export interface DatasetMetaData {
  icon: string;
  type: string;
  minZoomLevel: number;
  displayProperty: string;
}
