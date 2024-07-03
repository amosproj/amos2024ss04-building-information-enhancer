export interface LocationDataResponse {
  currentDatasetData: DatasetItem[];
  generalData: DatasetItem[];
  extraRows: DatasetItem[];
}

export interface DatasetItem {
  key: string;
  value: string;
  mapId: string;
}
