import { FeatureCollection, Geometry } from "geojson";
import defaultCityLocationData from "./FeatureCollection.json";
import defaultPolygonData from "./Bundeslaender.json";
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
  onUpdate: (data: FeatureCollection<Geometry>) => void
): FeatureCollection<Geometry> | undefined => {
  const [data, setData] = useState<FeatureCollection<Geometry>>();

  useEffect(() => {
    const fetchData = async (bounds: LatLngBounds) => {
      if (id === "charging_stations") {
        setData(geojsonCities as FeatureCollection<Geometry>);
        onUpdate(geojsonCities);
      } else if (id === "house_footprints") {
        setData(geojsonGemeindenPolygons as FeatureCollection<Geometry>);
        onUpdate(geojsonGemeindenPolygons);
      }

      /*try {
        // const bottomLat = bounds.getSouth();
        // const bottomLong = bounds.getWest();
        // const topLat = bounds.getNorth();
        // const topLong = bounds.getEast();
        // TO-DO: Fix so it is not hardcoded
        const url = `http://localhost:8081/api/v1.0/Dataset/1/data?BottomLat=9&BottomLong=10&TopLat=48&TopLong=49`;
        const response = await fetch(url);
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        const result = await response.json();
        setData(result as FeatureCollection<Geometry>);
      } catch (error) {
        console.error("Fetching data failed, using local GeoJSON data:", error);
        setData(geojsonData as FeatureCollection<Geometry>);
      }*/
    };

    fetchData(bounds);
  }, [bounds, zoom]);

  return data;
};

export default useGeoData;
