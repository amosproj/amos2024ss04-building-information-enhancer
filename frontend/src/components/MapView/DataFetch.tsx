import { FeatureCollection, Geometry } from "geojson";
import defaultCityLocationData from "./FeatureCollection.json";
import defaultPolygonData from "./gemeinden_simplify20.json";
import { LatLngBounds } from "leaflet";
import { useEffect, useState } from "react";
const geojsonCities: FeatureCollection =
  defaultCityLocationData as FeatureCollection;
const geojsonGemeindenPolygons: FeatureCollection =
  defaultPolygonData as FeatureCollection;

const useGeoData = (
  id: string,
  bounds: LatLngBounds,
  zoom: number,
  onUpdate?: (data: FeatureCollection<Geometry>) => void
): FeatureCollection<Geometry> | undefined => {
  const [data, setData] = useState<FeatureCollection<Geometry>>();

  useEffect(() => {
    /* eslint-disable */
    const fetchData = async (_bounds: LatLngBounds) => {
      /* eslint-enable */
      if (id === "house_footprints") {
        setData(geojsonGemeindenPolygons as FeatureCollection<Geometry>);
        onUpdate(geojsonGemeindenPolygons);
        return;
      }

      try {
        const bottomLat = bounds.getSouthWest().lat;
        const bottomLong = bounds.getSouthWest().lng;
        const topLat = bounds.getNorthEast().lat;
        const topLong = bounds.getNorthEast().lng;

        const url = `http://localhost:8081/api/v1.0/Dataset/1/data?BottomLat=${bottomLat}&BottomLong=${bottomLong}&TopLat=${topLat}&TopLong=${topLong}`;
        console.log(url);

        const response = await fetch(url);
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        const result = await response.json();
        setData(result as FeatureCollection<Geometry>);
        onUpdate(result);
      } catch (error) {
        console.error("Fetching data failed, using local GeoJSON data:", error);
        setData(geojsonCities as FeatureCollection<Geometry>);
        onUpdate(geojsonCities);
      }
    };

    fetchData(bounds);
  }, [bounds, zoom, id, onUpdate]);

  return data;
};

export default useGeoData;
