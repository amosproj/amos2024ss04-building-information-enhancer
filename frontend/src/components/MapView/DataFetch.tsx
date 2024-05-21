import { FeatureCollection, Geometry } from "geojson";
import data from "./FeatureCollection.json";
import { LatLngBounds } from "leaflet";
import { useEffect, useState } from "react";
const geojsonData: FeatureCollection = data as FeatureCollection;

const useGeoData = (
  bounds: LatLngBounds,
  zoom: number
): FeatureCollection<Geometry> | undefined => {
  const [data, setData] = useState<FeatureCollection<Geometry>>();

  useEffect(() => {
    /* eslint-disable */ const fetchData = async (_bounds: LatLngBounds) => {
      /* eslint-enable */
      // Uncomment and adjust the following lines to fetch data from an API
      // const url = `https://example.com/api/geo?bounds=${bounds.toBBoxString()}&zoom=${zoom}`;
      // const response = await fetch(url);
      // const result = await response.json();
      setData(geojsonData as FeatureCollection<Geometry>);
    };

    fetchData(bounds);
    console.log(
      `Data fetched for bounds: ${bounds.toBBoxString()} and zoom: ${zoom}`
    );
  }, [bounds, zoom]);

  return data;
};

export default useGeoData;
