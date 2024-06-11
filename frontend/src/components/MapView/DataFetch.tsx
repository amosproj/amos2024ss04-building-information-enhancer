import { FeatureCollection, Geometry } from "geojson";
import defaultCityLocationData from "./FeatureCollection.json";
import defaultPolygonData from "./output1000.json";
import { LatLngBounds } from "leaflet";
import { useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { AlertContext } from "../../contexts/AlertContext";
import axios from "axios";
const geojsonCities: FeatureCollection =
  defaultCityLocationData as FeatureCollection;
const geojsonGemeindenPolygons: FeatureCollection =
  defaultPolygonData as FeatureCollection;

// These values will be replaced after build with the .sh script when spinning up docker container.
export const currentEnvironment = {
  apiBaseHost: "API_GATEWAY_HOST",
  apiBasePort: "API_GATEWAY_PORT",
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
      case "empty_map":
        return "";
      default:
        return (
          currentEnvironment.apiBaseHost +
          ":" +
          currentEnvironment.apiBasePort +
          "/api/getDatasetViewportData"
        );
    }
  };

  useEffect(() => {
    if (id === "empty_map") return;
    if (tabProps && tabProps.dataset.lastDataRequestBounds === bounds) {
      console.log("SAME AS LAST TIME");
      return;
    }
    if (tabProps && tabProps.dataset.type === "markers" && zoom <= 10) {
      setData(undefined);
      console.log("too far away");
      return;
    }
    if (id === "house_footprints") {
      if (tabProps?.dataset.data.features.length == 0) {
        setData(geojsonGemeindenPolygons as FeatureCollection<Geometry>);
        onUpdate(geojsonGemeindenPolygons, bounds);
      } else {
        setData(geojsonGemeindenPolygons as FeatureCollection<Geometry>);
        //console.log("already loaded house_footprints");
      }

      return;
    }
    const fetchData = async (bounds: LatLngBounds): Promise<void> => {
      try {
        // Define the query parameters
        const params = {
          BottomLat: bounds.getSouthWest().lat, // bottomLat,
          BottomLong: bounds.getSouthWest().lng, //bottomLong,
          TopLat: bounds.getNorthEast().lat, //topLat,
          TopLong: bounds.getNorthEast().lng, //topLong,
          ZoomLevel: zoom,
          datasetID: id,
        };
        console.log(getApiUrlForDataset());
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [bounds, zoom, id]);

  return data;
};

export default useGeoData;
