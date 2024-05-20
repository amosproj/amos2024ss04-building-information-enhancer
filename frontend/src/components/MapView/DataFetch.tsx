import { FeatureCollection, Geometry } from "geojson";
import data from "./FeatureCollection.json";
import { LatLng } from "leaflet";
const geojsonData: FeatureCollection = data as FeatureCollection;

export function fetchData(
  _topLeft: LatLng,
  _bottomRight: LatLng
): FeatureCollection<Geometry> {
  return geojsonData;
}
