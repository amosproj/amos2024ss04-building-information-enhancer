import { Fragment, useContext, useEffect, useState } from "react";
import {
  MapContainer,
  TileLayer,
  WMSTileLayer,
  ZoomControl,
} from "react-leaflet";
import "leaflet/dist/leaflet.css";
import "leaflet.markercluster/dist/MarkerCluster.css";
import "leaflet.markercluster/dist/MarkerCluster.Default.css";
import "leaflet.markercluster";
import "./MapView.css";
import L from "leaflet";
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";
import MapOptions from "./MapOptions";
import { MapContext } from "../../contexts/MapContext";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import MapDatasetVisualizer from "./MapDatasetVisualizer";
import MapEventsHandler from "./MapEventsHandler";
import { Dataset } from "../../types/DatasetTypes";
import ZoomWarningLabel from "../ZoomWarningLabel/ZoomWarningLabel";
import ThreeDView from "../ThreeDView/ThreeDView";

const DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
  iconAnchor: [12, 40],
});

L.Marker.prototype.options.icon = DefaultIcon;

interface MapViewProps {
  datasetId: string;
}

const MapView: React.FC<MapViewProps> = ({ datasetId }) => {
  const { currentTabsCache, getCurrentTab, getOrFetchMetadata } =
    useContext(TabsContext);
  const [map, setMap] = useState<L.Map | null>(null);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const [isGrayscale, setIsGrayscale] = useState<boolean>(false);
  const [mapType, setMapType] = useState<
    "normal" | "satellite" | "parzellar" | "aerial"
  >("normal");
  const [if3D, setIf3D] = useState<boolean>(false);

  /**
   * Toggles the 3D View
   */
  const toggle3D = () => {
    console.log("3D");
    setIf3D(!if3D);
  };

  /**
   * Changes the layer type
   * @param type type of the layer
   */
  const handleMapTypeChange = (
    type: "normal" | "satellite" | "parzellar" | "aerial"
  ) => {
    setMapType(type);
  };

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
      {if3D ? (
        <ThreeDView />
      ) : (
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
              {pinnedFeatureCollections.map(
                (dataset: Dataset, index: number) => (
                  <MapDatasetVisualizer dataset={dataset} key={index} />
                )
              )}
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
      )}
      <MapOptions onMapTypeChange={handleMapTypeChange} toggle3D={toggle3D} />
    </div>
  );
};

export default MapView;
