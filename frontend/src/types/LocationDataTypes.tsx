export interface LocationDataResponse {
  individualData: DatasetItem[];
  selectionData: DatasetItem[];
}

export interface SubdataItem {
  key: string;
  value: string;
}

export interface DatasetItem {
  displayName: string;
  datasetID: string;
  coordinate: number[];
  subdata: SubdataItem[];
}
