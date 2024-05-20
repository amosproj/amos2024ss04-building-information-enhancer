import { FeatureCollection, Geometry } from "geojson";
import data from "./FeatureCollection.json";
import { LatLng, LatLngBounds } from "leaflet";
import { useEffect, useState } from "react";
const geojsonData: FeatureCollection = data as FeatureCollection;

export function fetchData(
  _topLeft: LatLng,
  _bottomRight: LatLng
): FeatureCollection<Geometry> {
  return geojsonData;
}

const useGeoData = (
  bounds: LatLngBounds,
  zoom: number
): FeatureCollection<Geometry> | undefined => {
  const [data, setData] = useState<FeatureCollection<Geometry>>();

  useEffect(() => {
    const fetchData = async () => {
      // Uncomment and adjust the following lines to fetch data from an API
      // const url = `https://example.com/api/geo?bounds=${bounds.toBBoxString()}&zoom=${zoom}`;
      // const response = await fetch(url);
      // const result = await response.json();
      setData(geojsonData as FeatureCollection<Geometry>);
    };

    fetchData();
  }, [bounds, zoom]);

  return data;
};

export default useGeoData;
