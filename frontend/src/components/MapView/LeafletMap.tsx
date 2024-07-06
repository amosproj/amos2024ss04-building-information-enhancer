import React, {
  Fragment,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { MapContainer, ZoomControl } from "react-leaflet";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import { MapContext } from "../../contexts/MapContext";
import MapDatasetVisualizer from "./MapDatasetVisualizer";
import { Dataset } from "../../types/DatasetTypes";
import MapEventsHandler from "./MapEventsHandler";
import ZoomWarningLabel from "../ZoomWarningLabel/ZoomWarningLabel";
import L, { LeafletEvent } from "leaflet";
import "leaflet-draw/dist/leaflet.draw.css";
import "leaflet-draw";
import SatelliteMap from "./BackgroundMaps/SatelliteMap";
import AerialMap from "./BackgroundMaps/AerialMap";
import NormalMap from "./BackgroundMaps/NormalMap";
import ParcelMap from "./BackgroundMaps/ParcelMap";
import "./LeafletMap.css";
import {
  MarkerSelection,
  PolygonSelection,
} from "../../types/MapSelectionTypes";

interface LeafletMapProps {
  datasetId: string;
  mapType: string;
}

const LeafletMap: React.FC<LeafletMapProps> = ({ datasetId, mapType }) => {
  const { currentTabsCache, getCurrentTab, getOrFetchMetadata } =
    useContext(TabsContext);
  const [map, setMap] = useState<L.Map | null>(null);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const currentMapCacheRef = useRef(currentMapCache);
  const [isGrayscale, setIsGrayscale] = useState<boolean>(false);
  const [polygonDrawer, setPolygonDrawer] = useState<L.Draw.Polygon | null>(
    null
  );
  const [drawnItems, setDrawnItems] = useState<L.FeatureGroup | null>(null);
  const currentTab = getCurrentTab();

  // Update ref value whenever currentMapCache changes
  useEffect(() => {
    currentMapCacheRef.current = currentMapCache;
  }, [currentMapCache]);

  /**
   * Toggle polygon drawer
   */
  useEffect(() => {
    if (polygonDrawer) {
      if (currentMapCache.isDrawing) {
        if (drawnItems) {
          drawnItems.clearLayers();
        }
        setCurrentMapCache({ ...currentMapCache, selectedCoordinates: null });
        polygonDrawer.enable();
      } else {
        polygonDrawer.disable();
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentMapCache.isDrawing]);

  /**
   * Delete the selection if other coordinate was selected
   */
  useEffect(() => {
    if (currentMapCache.selectedCoordinates instanceof MarkerSelection) {
      polygonDrawer?.disable();
      if (drawnItems) {
        drawnItems.clearLayers();
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentMapCache.selectedCoordinates]);

  /**
   * Fetches the metadata of the current tab
   */
  useEffect(() => {
    if (currentTab) {
      getOrFetchMetadata(currentTab.dataset.id);
    }
  }, [currentTab, getOrFetchMetadata]);

  /**
   * Refresh the map bounds on map change
   */
  useEffect(() => {
    if (map) {
      const initialBounds = map.getBounds();
      const initialCenter = map.getCenter();
      const initialZoom = map.getZoom();
      setCurrentMapCache((prevCache) => ({
        ...prevCache,
        mapInstance: map,
        mapCenter: initialCenter,
        mapBounds: initialBounds,
        zoom: initialZoom,
      }));
      // Allow for drawing polygons
      const drawnItems = new L.FeatureGroup();
      setDrawnItems(drawnItems);
      map.addLayer(drawnItems);
      setPolygonDrawer(new L.Draw.Polygon(map as L.DrawMap));
      // Bind for polygon created
      map.on(L.Draw.Event.CREATED, (event: LeafletEvent) => {
        const drawnObject = (event as L.DrawEvents.Created).layer;
        if (drawnObject instanceof L.Polygon) {
          if (drawnItems) {
            drawnItems.addLayer(drawnObject);
            const latLongs = drawnObject.toGeoJSON();
            const polygonSelection = new PolygonSelection(
              latLongs,
              "Custom Polygon",
              true
            );
            console.log(polygonSelection);
            console.log(currentMapCacheRef.current);
            setCurrentMapCache({
              ...currentMapCacheRef.current,
              selectedCoordinates: polygonSelection,
              isDrawing: false,
            });
          }
        }
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [map]);

  /**
   * Check for the zoom level threshold to apply the zoom warning and its effects
   */
  useEffect(() => {
    if (currentTab && currentTab.dataset.metaData) {
      setIsGrayscale(
        currentMapCache.zoom <= currentTab.dataset.metaData.minZoomLevel
      );
    }
  }, [currentMapCache.zoom, currentTab]);

  /**
   * Adds or removes the grayscale css value
   */
  useEffect(() => {
    if (map) {
      if (isGrayscale) {
        map.getContainer().classList.add("grayscale-overlay");
      } else {
        map.getContainer().classList.remove("grayscale-overlay");
      }
    }
  }, [isGrayscale, map]);

  const pinnedFeatureCollections = currentTabsCache.openedTabs
    .filter((tab: TabProps) => tab.ifPinned)
    .map((tab: TabProps) => tab.dataset);

  const isCurrentDataPinned = pinnedFeatureCollections.some(
    (dataset: Dataset) => dataset.id === datasetId
  );

  return (
    <div className="tab-map-container">
      <MapContainer
        center={currentMapCache.mapCenter}
        zoom={currentMapCache.zoom}
        className="map"
        ref={setMap}
        zoomControl={false}
        maxBounds={L.latLngBounds([47.1512, 5.6259], [54.967, 15.4446])}
        minZoom={6}
      >
        <ZoomControl position="topright" />
        {isGrayscale ? (
          <Fragment />
        ) : (
          <div>
            {pinnedFeatureCollections.map((dataset: Dataset, index: number) => (
              <MapDatasetVisualizer dataset={dataset} key={index} />
            ))}
            {!isCurrentDataPinned && currentTab && (
              <MapDatasetVisualizer dataset={currentTab.dataset} />
            )}
          </div>
        )}
        <MapEventsHandler />
        {mapType === "satellite" && <SatelliteMap />}
        {mapType === "aerial" && <AerialMap />}
        {mapType === "normal" && <NormalMap />}
        {mapType === "parcel" && <ParcelMap />}
        <ZoomWarningLabel />
      </MapContainer>
    </div>
  );
};

export default LeafletMap;
