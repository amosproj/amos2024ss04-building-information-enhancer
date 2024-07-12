import { useMap } from "react-leaflet";
import { Fragment, useCallback, useContext, useEffect, useState } from "react";
import { Feature, FeatureCollection, GeoJsonProperties, Point } from "geojson";
import { MapContext } from "../../contexts/MapContext";
import { TabsContext } from "../../contexts/TabsContext";
import GeoDataFetcher from "./GeoDataFetcher";
import L, { DivIcon, LatLng } from "leaflet";
import { LatLngBounds } from "leaflet";
import "proj4leaflet";
import "proj4";
import { createRoot } from "react-dom/client";
import { flushSync } from "react-dom";
import { MapPin } from "@phosphor-icons/react";
import { Dataset, DisplayProperty } from "../../types/DatasetTypes";
import { MarkersTypes } from "../../types/MarkersTypes";
import { createDivIcon } from "../../utils/mergeIcons";
import { convertPolygonsToMarkers } from "../../utils/polgonsToMarkers";
import { Popup } from "react-leaflet/Popup";
import { MarkerSelection } from "../../types/MapSelectionTypes";

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
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);

  const { setCurrentTabsCache } = useContext(TabsContext);
  const [popupData, setPopupData] = useState<Feature<
    Point,
    GeoJsonProperties
  > | null>(null);
  const [isPopupOpen, setIsPopupOpen] = useState(false);
  const [latLngCoordinates, setLatLngCoordinates] = useState<LatLng>(
    new LatLng(51.505, -0.09)
  );

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

  // Function to determine the color based on usageType using PolygonColoring from metadata
  const getColor = (usageType: string) => {
    if (dataset.metaData && dataset.metaData.polygonColoring) {
      for (const coloring of dataset.metaData.polygonColoring) {
        if (coloring.values.includes(usageType)) {
          return coloring.color;
        }
      }
    }
    return "#3388ff";
  };

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
          const geojsonLayer = L.geoJson(geoData, {
            style: (feature) => {
              return {
                color: feature
                  ? getColor(feature.properties.usageType)
                  : "#3388ff",
                fillOpacity: 0.2,
              };
            },
          });
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
            const marker = L.marker(latlng, {
              icon: dataset.metaData
                ? createDivIcon(dataset.metaData.icon)
                : divIcondefault,
            });
            return marker;
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
        pointToLayer: function (feature, latlng) {
          const marker = L.marker(latlng, {
            icon: dataset.metaData
              ? createDivIcon(dataset.metaData.icon)
              : divIcondefault,
          }).on("click", () => {
            setPopupData(feature);
            setIsPopupOpen(true);
            setLatLngCoordinates(latlng);
            // Select a marker on the map
            const markerSelection = new MarkerSelection(
              latlng,
              "Custom Marker",
              true
            );
            setCurrentMapCache((prevCache) => ({
              ...prevCache,
              selectedCoordinates: markerSelection,
            }));
          });
          return marker;
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

  /**
   * Creates a list of the properties for the popup data
   * @param popupData the popup data object
   * @param displayProperties the list of properties to display with displayName and value fields
   * @returns a list of strings
   */
  const listProperties = (popupData: GeoJsonProperties): DisplayProperty[] => {
    const listOfProperties: DisplayProperty[] = [];
    if (popupData && dataset.metaData) {
      for (const property in popupData.properties) {
        if (
          Object.prototype.hasOwnProperty.call(popupData.properties, property)
        ) {
          // Check if the property is in the displayProperties list
          const displayProperty = dataset.metaData.displayProperty.find(
            (dp) => dp.value === property
          );
          // If it is, format and push to the list
          if (displayProperty) {
            listOfProperties.push({
              displayName: displayProperty.displayName,
              value: String(popupData.properties[property]),
            });
          }
        }
      }
    }
    return listOfProperties;
  };

  return (
    <>
      {isPopupOpen && popupData && (
        <Popup position={latLngCoordinates} offset={[0, -25]}>
          {popupData.properties ? (
            <Fragment>
              {listProperties(popupData).map((displayProperty) => {
                return (
                  <div key={displayProperty.displayName}>
                    <b>{displayProperty.displayName}: </b>
                    <span>{displayProperty.value}</span>
                  </div>
                );
              })}
            </Fragment>
          ) : (
            <p>No data available</p>
          )}
        </Popup>
      )}
    </>
  );
};

export default MapDatasetVisualizer;
