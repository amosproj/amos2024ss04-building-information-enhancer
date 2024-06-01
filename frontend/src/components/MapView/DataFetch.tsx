import { FeatureCollection, Geometry } from "geojson";
import defaultCityLocationData from "./FeatureCollection.json";
import defaultPolygonData from "./output1000.json";
import { LatLngBounds } from "leaflet";
import { useContext, useEffect, useState } from "react";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import { AlertContext } from "../../contexts/AlertContext";
import axios from "axios";
import { MapContext } from "../../contexts/MapContext";
const geojsonCities: FeatureCollection =
  defaultCityLocationData as FeatureCollection;
const geojsonGemeindenPolygons: FeatureCollection =
  defaultPolygonData as FeatureCollection;

// Define the base of the API URL
const getBaseApiUrl = () => {
  switch (import.meta.env.VITE_STAGE) {
    case "production":
      return `http://${import.meta.env.VITE_API_HOST_PRODUCTION}:${
        import.meta.env.VITE_API_PORT_PRODUCTION
      }`;
    case "test":
      return `http://${import.meta.env.VITE_API_HOST_TEST}:${
        import.meta.env.VITE_API_PORT_TEST
      }`;
    default:
  }
  return `http://${import.meta.env.VITE_API_HOST_DEV}:${
    import.meta.env.VITE_API_PORT_DEV
  }`;
};

const useGeoData = (
  id: string,
  bounds: LatLngBounds,
  zoom: number,

  onUpdate: (data: FeatureCollection<Geometry>, bounds: LatLngBounds) => void
): FeatureCollection<Geometry> | undefined => {
  const [data, setData] = useState<FeatureCollection<Geometry>>();
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  const { currentTabsCache } = useContext(TabsContext);

  const tabProps = currentTabsCache.openedTabs.find(
    (tab) => tab.dataset.id === id
  );

  // Returns the API URL of the endpoint for a specific dataset
  const getApiUrlForDataset = (): string => {
    switch (id) {
      case "charging_stations":
        return getBaseApiUrl() + "/api/v1.0/Dataset/1/data";
      case "house_footprints":
        return getBaseApiUrl() + "";
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

  /*useEffect(() => {
    if (tabProps && tabProps.dataset.lastDataRequestBounds === bounds) {
      console.log("SAME AS LAST TIME");
      return;
    }
    const fetchData = async (bounds: LatLngBounds) => {
      if (id === "house_footprints") {
        setData(geojsonGemeindenPolygons as FeatureCollection<Geometry>);
        if (onUpdate) onUpdate(geojsonGemeindenPolygons, bounds);
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
        if (onUpdate) onUpdate(result, bounds);
      } catch (error) {
        console.error("Fetching data failed, using local GeoJSON data:", error);
        setData(geojsonCities as FeatureCollection<Geometry>);
        if (onUpdate) onUpdate(geojsonCities, bounds);
      }
    };

    fetchData(bounds);
  }, [tabProps, bounds, zoom, id, onUpdate]);*/

  useEffect(() => {
    if (tabProps && tabProps.dataset.lastDataRequestBounds === bounds) {
      console.log("SAME AS LAST TIME");
      return;
    }
    if (tabProps && tabProps.dataset.type === "markers" && zoom <= 10) {
      console.log("too far away");
      return;
    }
    if (id === "house_footprints") {
      if (tabProps?.dataset.data.features.length == 0) {
        setData(geojsonGemeindenPolygons as FeatureCollection<Geometry>);
        onUpdate(geojsonGemeindenPolygons, bounds);
      } else {
        console.log("already loaded house_footprints");
      }

      return;
    }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const fetchData = async (bounds: LatLngBounds): Promise<void> => {
      try {
        // Define the query parameters
        const params = {
          BottomLat: bounds.getSouthWest().lat, // bottomLat,
          BottomLong: bounds.getSouthWest().lng, //bottomLong,
          TopLat: bounds.getNorthEast().lat, //topLat,
          TopLong: bounds.getNorthEast().lng, //topLong,
          ZoomLevel: zoom,
        };
        //console.log(getApiUrlForDataset());
        const url = `http://localhost:8081/api/v1.0/Dataset/1/data?BottomLat=${params.BottomLat}&BottomLong=${params.BottomLong}&TopLat=${params.TopLat}&TopLong=${params.TopLong}`;
        console.log(url);
        const response = await axios.get<FeatureCollection<Geometry>>(
          getApiUrlForDataset(),
          {
            params,
          }
        );
        setData(response.data);
        onUpdate(response.data, bounds);
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
            onUpdate(geojsonCities, bounds);
            return;
          case "house_footprints":
            setData(geojsonGemeindenPolygons as FeatureCollection<Geometry>);
            onUpdate(geojsonGemeindenPolygons, bounds);
            return;
          default:
            return;
        }
      }
    };

    fetchData(bounds);
  }, [bounds, zoom, id]);

  return data;
};

export default useGeoData;
