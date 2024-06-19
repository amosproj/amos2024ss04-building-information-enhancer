import { FeatureCollection, Geometry } from "geojson";
import { LatLngBounds } from "leaflet";
import { useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { AlertContext } from "../../contexts/AlertContext";
import axios from "axios";
import { getAPIGatewayURL } from "../../utils";

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

  // Returns the API Gateway URL of the endpoint for a specific dataset.
  const getViewportDatasetEndpoint = (): string => {
    switch (id) {
      case "empty_map":
        return "";
      default:
        return getAPIGatewayURL() + "/api/getDatasetViewportData";
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
        console.log(getViewportDatasetEndpoint());
        const response = await axios.get<FeatureCollection<Geometry>>(
          getViewportDatasetEndpoint(),
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
      }
    };

    fetchData(bounds);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [bounds, zoom, id]);

  return data;
};

export default useGeoData;
