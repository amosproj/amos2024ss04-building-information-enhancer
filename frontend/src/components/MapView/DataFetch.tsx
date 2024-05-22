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
    const fetchData = async () => {
      try {
        // const bottomLat = bounds.getSouth();
        // const bottomLong = bounds.getWest();
        // const topLat = bounds.getNorth();
        // const topLong = bounds.getEast();
        // TO-DO: Fix so it is not hardcoded
        const url = `https://api-gateway:8081/api/v1.0/Dataset/1/data?BottomLat=9&BottomLong=10&TopLat=48&TopLong=49`;
        const response = await fetch(url);
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        const result = await response.json();
        setData(result as FeatureCollection<Geometry>);
      } catch (error) {
        console.error("Fetching data failed, using local GeoJSON data:", error);
        setData(geojsonData as FeatureCollection<Geometry>);
      }
    };

    fetchData();
  }, [bounds, zoom]);

  return data;
};

export default useGeoData;
