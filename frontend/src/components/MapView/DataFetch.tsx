import { FeatureCollection, Geometry } from "geojson";
import data from "./FeatureCollection.json";
import { LatLngBounds } from "leaflet";
import { useContext, useEffect, useState } from "react";
import { AlertContext } from "../../contexts/AlertContext";
const geojsonData: FeatureCollection = data as FeatureCollection;
import axios from "axios";

// Define the API URL
const apiUrl = `http://${import.meta.env.VITE_BACKEND_HOST}:${
  import.meta.env.VITE_BACKEND_PORT
}/api/v1.0/Dataset/1/data`;

const useGeoData = (
  bounds: LatLngBounds,
  zoom: number
): FeatureCollection<Geometry> | undefined => {
  const [data, setData] = useState<FeatureCollection<Geometry>>();
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  useEffect(() => {
    // Function to send the request
    const fetchData = async (): Promise<void> => {
      try {
        const bottomLat = bounds.getSouth();
        const bottomLong = bounds.getWest();
        const topLat = bounds.getNorth();
        const topLong = bounds.getEast();
        console.log(apiUrl);
        // Define the query parameters
        const params = {
          BottomLat: bottomLat,
          BottomLong: bottomLong,
          TopLat: topLat,
          TopLong: topLong,
          ZoomLevel: zoom,
        };
        const response = await axios.get<FeatureCollection<Geometry>>(apiUrl, {
          params,
        });
        setData(response.data as FeatureCollection<Geometry>);
      } catch (error) {
        // Display alert
        setCurrentAlertCache({
          ...currentAlertCache,
          isAlertOpened: true,
          text: "Fetching data failed, using local GeoJSON data.",
        });
        // Console log the error
        if (axios.isAxiosError(error)) {
          console.error("Axios error fetching data:", error.message);
        } else {
          console.error("Unknown error fetching data:", error);
        }
        setData(geojsonData as FeatureCollection<Geometry>);
      }
    };

    fetchData();
  }, [bounds, zoom]);

  return data;
};

export default useGeoData;
