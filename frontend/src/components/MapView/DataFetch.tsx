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
        const bottomLat = bounds.getSouth();
        const bottomLong = bounds.getWest();
        const topLat = bounds.getNorth();
        const topLong = bounds.getEast();

        const url = `https://localhost:5001/api/v1.0/Dataset/1/data?bottomLat=${bottomLat}&bottomLong=${bottomLong}&topLat=${topLat}&topLong=${topLong}&zoomLevel=${zoom}`;
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
