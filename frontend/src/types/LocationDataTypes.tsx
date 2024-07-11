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
  value: string | null;
  datasetID: string | null;
  coordinate: number[] | null;
  subdata: SubdataItem[];
}
