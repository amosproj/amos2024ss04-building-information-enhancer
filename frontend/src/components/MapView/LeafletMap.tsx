import { Fragment, useContext, useEffect, useState } from "react";
import {
  MapContainer,
  TileLayer,
  WMSTileLayer,
  ZoomControl,
} from "react-leaflet";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import { MapContext } from "../../contexts/MapContext";
import MapDatasetVisualizer from "./MapDatasetVisualizer";
import { Dataset } from "../../types/DatasetTypes";
import MapEventsHandler from "./MapEventsHandler";
import ZoomWarningLabel from "../ZoomWarningLabel/ZoomWarningLabel";
import L from "leaflet";

interface LeafletMapProps {
  datasetId: string;
  mapType: string;
}

const LeafletMap: React.FC<LeafletMapProps> = ({ datasetId, mapType }) => {
  const { currentTabsCache, getCurrentTab, getOrFetchMetadata } =
    useContext(TabsContext);
  const [map, setMap] = useState<L.Map | null>(null);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const [isGrayscale, setIsGrayscale] = useState<boolean>(false);

  const currentTab = getCurrentTab();

  /**
   * Fetches the metadata of the current tab
   */
  useEffect(() => {
    if (currentTab) {
      getOrFetchMetadata(currentTab.dataset.id);
    }
  }, [currentTab, getOrFetchMetadata]);

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
    }
  }, [map, setCurrentMapCache]);

  useEffect(() => {
    if (currentTab && currentTab.dataset.metaData) {
      setIsGrayscale(
        currentMapCache.zoom <= currentTab.dataset.metaData.minZoomLevel
      );
    }
  }, [currentMapCache.zoom, currentTab]);

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

        {mapType === "satellite" && (
          <div>
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
              url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
            />
            <WMSTileLayer
              url="https://sg.geodatenzentrum.de/wms_sentinel2_de"
              layers="rgb_2020"
              format="image/png"
              transparent={true}
              attribution='&copy; Bundesamt für Kartographie und Geodäsie (BKG), Bayerische Vermessungverwaltung,  <a href="http://sg.geodatenzentrum.de/web_public/gdz/datenquellen/Datenquellen_TopPlusOpen.pdf">Sources</a>'
              bounds={L.latLngBounds([47.141, 5.561], [55.054, 15.579])}
            />
          </div>
        )}
        {mapType === "aerial" && (
          <div>
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
              url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
            />
            <WMSTileLayer
              url="https://geoservices.bayern.de/od/wms/dop/v1/dop40?"
              layers="by_dop40c"
              format="image/png"
              transparent={true}
              attribution="&copy; © Europäische Union, enthält Copernicus Sentinel-2 Daten 2020, verarbeitet durch das Bundesamt für Kartographie und Geodäsie (BKG)"
              bounds={L.latLngBounds([47.141, 5.561], [55.054, 15.579])}
            />
          </div>
        )}
        {mapType === "normal" && (
          <div>
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
          </div>
        )}
        {mapType === "parzellar" && (
          <div>
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
              url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
            />
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
              url="https://wmtsod1.bayernwolke.de/wmts/by_webkarte/{s}/{z}/{x}/{y}"
            />
            <WMSTileLayer
              url="https://geoservices.bayern.de/wms/v1/ogc_alkis_parzellarkarte.cgi?"
              layers="by_alkis_parzellarkarte_farbe"
            />
          </div>
        )}
        <ZoomWarningLabel />
      </MapContainer>
    </div>
  );
};

export default LeafletMap;
