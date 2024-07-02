import { useContext, useEffect, useState } from "react";
import { MapContainer } from "react-leaflet/MapContainer";

import { TileLayer } from "react-leaflet/TileLayer";
import "leaflet/dist/leaflet.css";
import "leaflet.markercluster/dist/MarkerCluster.css";
import "leaflet.markercluster/dist/MarkerCluster.Default.css";
import "leaflet.markercluster";

import "./MapView.css";
import L from "leaflet";
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";
import MapOptions from "./MapOptions";
import { WMSTileLayer, ZoomControl } from "react-leaflet";
import { MapContext } from "../../contexts/MapContext";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import { Dataset } from "../DatasetsList/DatasetsList";
import MapDatasetVisualizer from "./MapDatasetVisualizer";
import MapEventsHandler from "./MapEventsHandler";
import ZoomWarningLabel from "./ZoomWarningLabel";
import { MarkersTypes } from "../DatasetsList/MarkersTypes";

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
  const { currentTabsCache } = useContext(TabsContext);
  const [map, setMap] = useState<L.Map | null>(null);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);

  const [mapType, setMapType] = useState<"normal" | "satellite" | "parzellar" | "aerial">("normal");

  const handleMapTypeChange = (type: "normal" | "satellite" | "parzellar" | "aerial") => {
    setMapType(type);
  };

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

  // Get the feature collections from pinned tabs
  const pinnedFeatureCollections = currentTabsCache.openedTabs
    .filter((tab: TabProps) => tab.ifPinned)
    .map((tab: TabProps) => tab.dataset);

  const tabProps = currentTabsCache.openedTabs.find(
    (tab: TabProps) => tab.dataset.id === datasetId
  );

  // Check if the current geoData is in the pinnedFeatureCollections
  const isCurrentDataPinned = pinnedFeatureCollections.some(
    (dataset: Dataset) => dataset.id === datasetId
  );
  //console.log("current data pinned" + isCurrentDataPinned);
  const minZoomForLabel = 10;

  return (
    <div className="tab-map-container">
      <MapOptions onMapTypeChange={handleMapTypeChange} />
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

        {pinnedFeatureCollections.map((dataset: Dataset, index: number) => (
          <MapDatasetVisualizer dataset={dataset} key={index} />
        ))}
        {!isCurrentDataPinned && tabProps && (
          <MapDatasetVisualizer dataset={tabProps.dataset} />
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
              //bounds={L.latLngBounds([47.141, 5.561], [55.054, 15.579])}
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
        {tabProps && tabProps.dataset.type === MarkersTypes.Markers && (
          <ZoomWarningLabel
            label="Zoom in to see the markers"
            minZoom={minZoomForLabel}
          />
        )}
      </MapContainer>
    </div>
  );
};

export default MapView;
