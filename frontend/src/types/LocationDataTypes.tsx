/**
 * A response object from the location endpoint.
 */
export interface LocationDataResponse {
  individualData: DatasetItem[];
  selectionData: DatasetItem[];
}

/**
 * A single dataset row visible in the data view.
 */
export interface DatasetItem {
  displayName: string;
  value: string | null;
  datasetId: string | null;
  coordinate: number[] | null;
  subdata: SubdataItem[] | null;
}

/**
 * Sub rows for the data view and the dataset row.
 */
export interface SubdataItem {
  key: string;
  value: string;
}
