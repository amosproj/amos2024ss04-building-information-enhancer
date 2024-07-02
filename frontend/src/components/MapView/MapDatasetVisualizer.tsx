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
import { mergeIcons } from "../../utils/mergeIcons";
import { createRoot } from "react-dom/client";
import { flushSync } from "react-dom";
import { MapPin } from "@phosphor-icons/react";
import { Dataset } from "../../types/DatasetTypes";
import { MarkersTypes } from "../../types/MarkersTypes";

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

const pinSvg =
  '<svg width="32" height="32" fill="#000000" viewBox="0 0 256 256" version="1.1" id="svg1" xmlns="http://www.w3.org/2000/svg" xmlns:svg="http://www.w3.org/2000/svg">  <defs id="defs1" />  <path d="m 128,16 a 88.1,88.1 0 0 0 -88,88 c 0,75.3 80,132.17 83.41,134.55 a 8,8 0 0 0 9.18,0 C 136,236.17 216,179.3 216,104 A 88.1,88.1 0 0 0 128,16 Z m 0,56 a 32,32 0 1 1 -32,32 32,32 0 0 1 32,-32 z" id="path1" />  <ellipse style="fill:#ffffff;stroke:#000000;stroke-width:0;stroke-dasharray:none;stroke-opacity:1" id="path12" cx="128.39645" cy="104.18782" rx="81.751793" ry="81.967773" /></svg>';

const createDivIcon = (iconSvgString: string) => {
  const combinedSvg = mergeIcons(pinSvg, iconSvgString, 40, 16, 12, 6);

  return L.divIcon({
    html: combinedSvg,
    className: "", // Optional: add a custom class name
    iconSize: [40, 40],
    iconAnchor: [20, 20], // Adjust the anchor point as needed
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
        console.log("pol");
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
        console.log("mark");
        // Add the markers to the map instead
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
