import { useCallback, useContext, useState } from "react";
import { MapContainer } from "react-leaflet/MapContainer";
import { Marker } from "react-leaflet/Marker";
import { Popup } from "react-leaflet/Popup";
import { TileLayer } from "react-leaflet/TileLayer";
import "leaflet/dist/leaflet.css";
import "./MapView.css";
import { useMap, useMapEvents } from "react-leaflet/hooks";
import L, { DivIcon, LatLng } from "leaflet";
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";
import MapOptions from "./MapOptions";
import useGeoData from "./DataFetch";
import { GeoJSON, WMSTileLayer } from "react-leaflet";
import { MapContext } from "../../contexts/MapContext";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import { FeatureCollection } from "geojson";
import { Dataset } from "../DatasetsList/DatasetsList";
import { createRoot } from "react-dom/client";
import { flushSync } from "react-dom";
import { MapPin } from "@phosphor-icons/react";

const DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
  iconAnchor: [12, 40],
});

L.Marker.prototype.options.icon = DefaultIcon;

// Utility function to render a React component to HTML string
const renderToHtml = (Component: React.FC) => {
  const div = document.createElement("div");
  const root = createRoot(div);
  flushSync(() => {
    root.render(<Component />);
  });
  return div.innerHTML;
};

const divIconMarker: DivIcon = L.divIcon({
  html: renderToHtml(() => <MapPin size={36} color="#ff0000" weight="fill" />),
  className: "", // Optional: add a custom class name
  iconSize: [36, 36],
  iconAnchor: [18, 36], // Adjust the anchor point as needed
});

interface MapViewProps {
  datasetId: string;
}

const MapView: React.FC<MapViewProps> = ({ datasetId }) => {
  const { currentTabsCache, setCurrentTabsCache } = useContext(TabsContext);

  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const [showSatellite, setShowSatellite] = useState<boolean>(false);
  const toggleShowSatellite = () => {
    setShowSatellite((prevShowSatellite) => !prevShowSatellite);
  };

  const updateDatasetData = useCallback(
    (newData: FeatureCollection) => {
      setCurrentTabsCache((prevCache) => {
        const updatedTabs = prevCache.openedTabs.map((tab) => {
          if (tab.dataset.id === datasetId) {
            return {
              ...tab,
              dataset: {
                ...tab.dataset,
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
    [datasetId, setCurrentTabsCache]
  );

  const geoData = useGeoData(
    datasetId,
    currentMapCache.mapBounds,
    currentMapCache.zoom,
    updateDatasetData
  );

  const MapEventsHandler = () => {
    const map = useMap();
    useMapEvents({
      click: (event) => {
        console.log(event);
        setCurrentMapCache({
          ...currentMapCache,
          selectedCoordinates: event.latlng,
        });
      },
      moveend: (event) => {
        setCurrentMapCache({
          ...currentMapCache,
          mapCenter: event.target.getCenter(),
          mapBounds: event.target.getBounds(),
          zoom: event.target.getZoom(),
        });
      },
    });
    return (
      <Marker
        position={currentMapCache.selectedCoordinates}
        icon={divIconMarker}
      >
        <Popup>
          <span
            // Get the current location of the user
            onClick={() => {
              map
                .locate({ setView: true })
                .on("locationfound", function (event) {
                  setPosition(event.latlng);
                  map.flyTo(event.latlng, map.getZoom(), {
                    animate: true,
                    duration: 50,
                  });
                })
                // If access to the location was denied
                .on("locationerror", function (event) {
                  console.log(event);
                  alert("Location access denied.");
                });
            }}
          >
            {currentMapCache.selectedCoordinates.lat.toFixed(4)},{" "}
            {currentMapCache.selectedCoordinates.lng.toFixed(4)}
          </span>
        </Popup>
      </Marker>
    );
  };

  function setPosition(latlng: L.LatLng) {
    setCurrentMapCache({ ...currentMapCache, selectedCoordinates: latlng });
  }

  // Get the feature collections from pinned tabs
  const pinnedFeatureCollections = currentTabsCache.openedTabs
    .filter((tab: TabProps) => tab.ifPinned)
    .map((tab: TabProps) => tab.dataset);

  const tabProps = currentTabsCache.openedTabs.find(
    (tab: TabProps) => tab.dataset.id === datasetId
  );

  // Check if the current geoData is in the pinnedFeatureCollections
  const isCurrentDataPinned = pinnedFeatureCollections.some(
    (dataset: Dataset) => dataset.data === geoData
  );

  return (
    <div className="tab-map-container">
      <MapOptions toggleShowSatellite={toggleShowSatellite} />
      <MapContainer
        center={currentMapCache.mapCenter}
        zoom={currentMapCache.zoom}
        className="map"
      >
        {pinnedFeatureCollections.map((dataset: Dataset, index: number) => (
          <GeoJSON
            style={{ fillOpacity: 0.1 }}
            key={index}
            data={dataset.data}
            pointToLayer={(_geoJsonPoint, latlng: LatLng) => {
              if (dataset.markerIcon)
                return L.marker(latlng, { icon: dataset.markerIcon });
              else return L.marker(latlng);
            }}
          />
        ))}
        {!isCurrentDataPinned && geoData && (
          <GeoJSON
            style={{ fillOpacity: 0.1 }}
            data={geoData}
            pointToLayer={(_geoJsonPoint, latlng: LatLng) => {
              if (tabProps && tabProps.dataset.markerIcon)
                return L.marker(latlng, { icon: tabProps.dataset.markerIcon });
              else return L.marker(latlng);
            }}
          />
        )}

        <MapEventsHandler />

        {showSatellite ? (
          <div>
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
              url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
            />
            <WMSTileLayer
              url="https://sg.geodatenzentrum.de/wms_sentinel2_de"
              layers="rgb_2020"
              format="image/png"
              transparent={true}
              attribution='&copy; Bundesamt für Kartographie und Geodäsie (BKG), Bayerische Vermessungverwaltung,  <a href="http://sg.geodatenzentrum.de/web_public/gdz/datenquellen/Datenquellen_TopPlusOpen.pdf">Sources</a>'
            />
            <WMSTileLayer
              url="https://geoservices.bayern.de/od/wms/dop/v1/dop40?"
              layers="by_dop40c"
              format="image/png"
              transparent={true}
              attribution="&copy; © Europäische Union, enthält Copernicus Sentinel-2 Daten 2020, verarbeitet durch das Bundesamt für Kartographie und Geodäsie (BKG)"
            />
          </div>
        ) : (
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
        )}
      </MapContainer>
    </div>
  );
};

export default MapView;
