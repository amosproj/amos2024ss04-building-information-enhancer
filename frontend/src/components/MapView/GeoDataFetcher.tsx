import { FeatureCollection, Geometry } from "geojson";
import { LatLngBounds } from "leaflet";
import { useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { AlertContext } from "../../contexts/AlertContext";
import { fetchViewportDataForDataset } from "../../services/viewportDataService";

/***
 * Function to fetch the data from the backend.
 * Connects to the API Gateway.
 */
const GeoDataFetcher = (
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

  // Fetch the data
  useEffect(() => {
    // Check if empty map
    if (id === "empty_map") return;
    // Check if the same viewport
    if (tabProps && tabProps.dataset.lastDataRequestBounds === bounds) {
      return;
    }
    // Check if
    if (tabProps && tabProps.dataset.type === "markers" && zoom <= 10) {
      setData(undefined);
      console.log("too far away");
      return;
    }
    const fetchData = async (bounds: LatLngBounds): Promise<void> => {
      const viewportData = await fetchViewportDataForDataset(id, bounds, zoom);
      if (viewportData) {
        console.log("t");
        setData(viewportData);
        onUpdate(viewportData, bounds);
      } else {
        // Display alert
        setCurrentAlertCache({
          ...currentAlertCache,
          isAlertOpened: true,
          text: "Fetching data failed.",
        });
      }
    };

    fetchData(bounds);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [bounds, zoom, id]);

  return data;
};

export default GeoDataFetcher;
