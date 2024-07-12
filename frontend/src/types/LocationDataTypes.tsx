export interface LocationDataResponse {
  individualData: DatasetItem[];
  generalData: DatasetItem[];
}

export interface DatasetItem {
  key: string;
  value: string;
  mapId: string;
}
