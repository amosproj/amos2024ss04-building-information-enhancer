export interface DatasetBasicData {
  datasetId: string;
  name: string;
  description: string;
  icon: string;
}

export interface DatasetListResponse {
  basicInfoList: DatasetBasicData[];
}
