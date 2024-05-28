import { FeatureCollection, Geometry } from "geojson";
import defaultCityLocationData from "./FeatureCollection.json";
import defaultPolygonData from "./gemeinden_simplify20.json";
import { LatLngBounds } from "leaflet";
import { useContext, useEffect, useState } from "react";
import axios from "axios";
import { AlertContext } from "../../contexts/AlertContext";
const geojsonCities: FeatureCollection =
  defaultCityLocationData as FeatureCollection;
const geojsonGemeindenPolygons: FeatureCollection =
  defaultPolygonData as FeatureCollection;

// Define the base of the API URL
const baseApiUrl = `http://${import.meta.env.VITE_BACKEND_HOST}:${
  import.meta.env.VITE_BACKEND_PORT
}`;

const useGeoData = (
  id: string,
  bounds: LatLngBounds,
  zoom: number,
  onUpdate: (data: FeatureCollection<Geometry>) => void
): FeatureCollection<Geometry> | undefined => {
  const [data, setData] = useState<FeatureCollection<Geometry>>();
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  // Returns the API URL of the endpoint for a specific dataset
  const getApiUrlForDataset = (): string => {
    switch (id) {
      case "charging_stations":
        return baseApiUrl + "/api/v1.0/Dataset/1/data";
      case "house_footprints":
        return baseApiUrl + "";
      default:
        // Display alert
        setCurrentAlertCache({
          ...currentAlertCache,
          isAlertOpened: true,
          text: "Dataset with provided ID does not exist.",
        });
        return "";
    }
  };

  useEffect(() => {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const fetchData = async (_bounds: LatLngBounds): Promise<void> => {
      try {
        const bottomLat = bounds.getSouth();
        const bottomLong = bounds.getWest();
        const topLat = bounds.getNorth();
        const topLong = bounds.getEast();
        console.log(baseApiUrl);
        // Define the query parameters
        const params = {
          BottomLat: bottomLat,
          BottomLong: bottomLong,
          TopLat: topLat,
          TopLong: topLong,
          ZoomLevel: zoom,
        };
        const response = await axios.get<FeatureCollection<Geometry>>(
          getApiUrlForDataset(),
          {
            params,
          }
        );
        setData(response.data);
        onUpdate(response.data);
      } catch (error) {
        // Display alert
        setCurrentAlertCache({
          ...currentAlertCache,
          isAlertOpened: true,
          text: "Fetching data failed, using local GeoJSON data.",
        });
        console.error("Fetching data failed, using local GeoJSON data.", error);
        // Console log the error
        if (axios.isAxiosError(error)) {
          console.error("Axios error fetching data:", error.message);
        } else {
          console.error("Unknown error fetching data:", error);
        }
        // Load the static data
        switch (id) {
          case "charging_stations":
            setData(geojsonCities as FeatureCollection<Geometry>);
            onUpdate(geojsonCities);
            return;
          case "house_footprints":
            setData(geojsonGemeindenPolygons as FeatureCollection<Geometry>);
            onUpdate(geojsonGemeindenPolygons);
            return;
          default:
            return;
        }
      }
    };

    fetchData(bounds);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [bounds, zoom, id]);

  return data;
};

export default useGeoData;
