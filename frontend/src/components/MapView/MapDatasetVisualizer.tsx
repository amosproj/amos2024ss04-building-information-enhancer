import { useMap } from "react-leaflet";
import { useCallback, useContext, useEffect } from "react";
import { FeatureCollection } from "geojson";
import { MapContext } from "../../contexts/MapContext";
import { TabsContext } from "../../contexts/TabsContext";
import GeoDataFetcher from "./GeoDataFetcher";
import L, { DivIcon } from "leaflet";
import { LatLngBounds } from "leaflet";
import "proj4leaflet";
import "proj4";
import { createRoot } from "react-dom/client";
import { flushSync } from "react-dom";
import { MapPin } from "@phosphor-icons/react";
import { Dataset } from "../../types/DatasetTypes";
import { MarkersTypes } from "../../types/MarkersTypes";
import { createDivIcon } from "../../utils/mergeIcons";
import { convertPolygonsToMarkers } from "../../utils/polgonsToMarkers";

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
  const geoData = GeoDataFetcher(
    dataset.id,
    currentMapCache.mapBounds,
    currentMapCache.zoom,
    updateDatasetData
  );

  useEffect(() => {
    // Check if data has been fetched
    if (!geoData || !dataset.metaData) return;
    // Check if dataset type is none
    if (dataset.metaData.type === MarkersTypes.None) {
      return;
    }
    // For Areas type datasets
    else if (dataset.metaData.type === MarkersTypes.Areas) {
      // Check if the zoom level is above the markers threshold
      // If yes, display markers instead of polygons
      if (currentMapCache.zoom > dataset.metaData.markersThreshold) {
        // Add the polygons to the map
        try {
          const geojsonLayer = L.geoJson(geoData);
          geojsonLayer.addTo(map);
          return () => {
            map.removeLayer(geojsonLayer);
          };
        } catch (error) {
          console.error("Error adding GeoJSON layer to the map:", error);
        }
      } else {
        // Convert polygons to markers
        const markerData = convertPolygonsToMarkers(geoData);
        // Add the markers to the map instead
        const geojsonLayer = L.geoJson(markerData, {
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
      // For Markers type datasets
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
  }, [dataset, currentMapCache.zoom, map, geoData]);

  return null;
};

export default MapDatasetVisualizer;
