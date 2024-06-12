import { useMap } from "react-leaflet";
import { Dataset } from "../DatasetsList/DatasetsList";
import { useCallback, useContext, useEffect, useState } from "react";
import { FeatureCollection } from "geojson";
import { MapContext } from "../../contexts/MapContext";
import { TabsContext } from "../../contexts/TabsContext";
import useGeoData from "./DataFetch";
import L, { DivIcon } from "leaflet";
import { LatLngBounds } from "leaflet";
import "proj4leaflet";
import "proj4";
import axios from "axios";
import { DatasetMetaData } from "../DatasetsList/DatasetTypes";
import { getAPIGatewayURL } from "../../utils";
import { createRoot } from "react-dom/client";
import { flushSync } from "react-dom";
import { MapPin } from "@phosphor-icons/react";

interface MapDatasetVisualizerProps {
  dataset: Dataset;
}

// Utility function to render a React component to HTML string
const renderToHtml = (Component: React.FC) => {
  const div = document.createElement("div");
  const root = createRoot(div);
  flushSync(() => {
    root.render(<Component />);
  });
  return div.innerHTML;
};

const createDivIcon = (iconName: string) => {
  console.log(iconName);
  return L.divIcon({
    html: '<div style="font-size:32px;">' + iconName + "</div>",
    className: "", // Optional: add a custom class name
    iconSize: [34, 34],
    iconAnchor: [17, 17], // Adjust the anchor point as needed
  });
};

const divIcondefault: DivIcon = L.divIcon({
  html: renderToHtml(() => <MapPin size={32} weight="duotone" />),
  className: "", // Optional: add a custom class name
  iconSize: [34, 34],
  iconAnchor: [17, 17], // Adjust the anchor point as needed
});

const MapDatasetVisualizer: React.FC<MapDatasetVisualizerProps> = ({
  dataset,
}) => {
  const map = useMap();
  const { currentMapCache } = useContext(MapContext);
  const { setCurrentTabsCache } = useContext(TabsContext);

  const [metadataLoading, setMetadataLoading] = useState(false); // State to track metadata loading

  // Function to fetch metadata for the dataset
  const fetchMetadata = useCallback(async () => {
    console.log("HEYYYYOOOOOOOO");
    try {
      setMetadataLoading(true); // Set loading state to true
      const params = {
        datasetID: dataset.id,
      };
      const response = await axios.get<DatasetMetaData>(
        getAPIGatewayURL() + "/api/getDatasetMetadata/",
        {
          params,
        }
      ); // Make the API call with dataset id as parameter
      // Assuming the response data structure matches DatasetBasicData type
      const metadata = response.data;
      // Update dataset metadata in state or context as needed
      setCurrentTabsCache((prevCache) => {
        const updatedTabs = prevCache.openedTabs.map((tab) => {
          if (tab.dataset.id === dataset.id) {
            return {
              ...tab,
              dataset: {
                ...tab.dataset,
                metaData: metadata, // Update metadata
              },
            };
          }
          return tab;
        });
        return {
          ...prevCache,
          openedTabs: updatedTabs,
        };
      });
    } catch (error) {
      console.error("Error fetching metadata:", error);
    } finally {
      setMetadataLoading(false); // Set loading state to false
    }
  }, [dataset.id, setCurrentTabsCache, setMetadataLoading]);

  useEffect(() => {
    // Fetch metadata only if it's not already loaded and metadataLoading state is false
    if (!dataset.metaData && !metadataLoading) {
      fetchMetadata();
    }
  }, [dataset.metaData, metadataLoading, fetchMetadata]);

  const updateDatasetData = useCallback(
    (newData: FeatureCollection, bounds: LatLngBounds) => {
      setCurrentTabsCache((prevCache) => {
        const updatedTabs = prevCache.openedTabs.map((tab) => {
          if (tab.dataset.id === dataset.id) {
            return {
              ...tab,
              dataset: {
                ...tab.dataset,
                lastDataRequestBounds: bounds,
                data: newData,
              },
            };
          }
          return tab;
        });

        return {
          ...prevCache,
          openedTabs: updatedTabs,
        };
      });
    },
    [dataset.id, setCurrentTabsCache]
  );
  const geoData = useGeoData(
    dataset.id,
    currentMapCache.mapBounds,
    currentMapCache.zoom,
    updateDatasetData
  );
  const myCRS = new L.Proj.CRS(
    "EPSG:25832",
    "+proj=utm +zone=32 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs"
  );
  const coordsToLatLngWithCRS = (coords: number[]) => {
    const latLng = myCRS.unproject(L.point(coords[0], coords[1]));
    return latLng;
  };

  useEffect(() => {
    if (!geoData) return;

    if (dataset.id === "house_footprints") {
      console.log("hi from data visualizer");

      if (currentMapCache.zoom < 13) {
        const markerClusterGroup = L.markerClusterGroup();
        L.geoJson(geoData, {
          coordsToLatLng: coordsToLatLngWithCRS,
          onEachFeature: function (feature, featureLayer) {
            if (feature.geometry.type === "Polygon") {
              const bounds = (
                featureLayer as unknown as { getBounds: () => L.LatLngBounds }
              ).getBounds();
              const center = bounds.getCenter();
              const marker = L.marker(center);
              marker.addTo(markerClusterGroup);
            }
          },
        });

        //map.fitBounds(g.getBounds());
        map.addLayer(markerClusterGroup);
        return () => {
          map.removeLayer(markerClusterGroup);
        };
      } else {
        const geojsonLayer = L.geoJson(geoData, {
          coordsToLatLng: coordsToLatLngWithCRS,
        });
        geojsonLayer.addTo(map);

        return () => {
          map.removeLayer(geojsonLayer);
        };
      }
    } else {
      const geojsonLayer = L.geoJson(geoData, {
        pointToLayer: function (_feature, latlng) {
          return L.marker(latlng, {
            icon: dataset.metaData
              ? createDivIcon(dataset.metaData.icon)
              : divIcondefault,
          });
        },

        style: { fillOpacity: 0.1 },
      });
      const markerClusterGroup = L.markerClusterGroup();

      markerClusterGroup.addLayer(geojsonLayer);
      map.addLayer(markerClusterGroup);

      return () => {
        map.removeLayer(markerClusterGroup);
      };
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dataset, currentMapCache.zoom, map, geoData]);

  return null;
};

export default MapDatasetVisualizer;
