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
  const { getOrFetchMetadata, getCurrentTab } = useContext(TabsContext);

  const currentTab = getCurrentTab();

  /**
   * Checks if the metadata has been fetched, if not does it.
   * Next, it fetches the viewport data itself.
   */
  const fetchMetadataAndData = async () => {
    // Check if the currentTab is not defined or if it is an empty map
    if (!currentTab || id === "empty_map") return;
    // Skip if the viewport did not change
    if (currentTab.dataset.lastDataRequestBounds === bounds) {
      return;
    }
    // Fetch the metadata if not done so already
    if (!currentTab.dataset.metaData) {
      const metadata = await getOrFetchMetadata(currentTab.dataset.id);
      // Update currentTab with the newly fetched metadata
      currentTab.dataset.metaData = metadata;
    }
    // Check if the zoom threshold has not been achieved
    if (
      currentTab &&
      currentTab.dataset.metaData &&
      currentTab.dataset.metaData.type === "markers" &&
      zoom <= currentTab.dataset.metaData!.minZoomLevel
    ) {
      setData(undefined);
      return;
    }
    fetchData(bounds);
  };

  /**
   * Fetches the viewport data for a specific dataset.
   * @param bounds the bounds of the viewport
   */
  const fetchData = async (bounds: LatLngBounds): Promise<void> => {
    const viewportData = await fetchViewportDataForDataset(id, bounds, zoom);
    if (viewportData) {
      setData(viewportData);
      onUpdate(viewportData, bounds);
    } else {
      // Display alert
      setCurrentAlertCache({
        ...currentAlertCache,
        isAlertOpened: true,
        text: "Fetching data failed.",
      });
      console.error("Error fetching data.");
    }
  };

  /**
   * Fetches the data whenever the viewport, zoom or datasetID changes.
   */
  useEffect(() => {
    fetchMetadataAndData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [bounds, zoom, id]);

  return data;
};

export default GeoDataFetcher;
