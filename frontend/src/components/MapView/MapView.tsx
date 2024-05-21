import { useContext } from "react";
import { MapContainer } from "react-leaflet/MapContainer";
import { Marker } from "react-leaflet/Marker";
import { Popup } from "react-leaflet/Popup";
import { TileLayer } from "react-leaflet/TileLayer";
import "leaflet/dist/leaflet.css";
import "./MapView.css";
import { useMap, useMapEvents } from "react-leaflet/hooks";
import L from "leaflet";
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";
import MapOptions from "./MapOptions";
import useGeoData from "./DataFetch";
import { GeoJSON } from "react-leaflet";
import { MapContext } from "../../contexts/MapContext";

const DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
  iconAnchor: [12, 40],
});

L.Marker.prototype.options.icon = DefaultIcon;

const MapView: React.FC = () => {
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const geoData = useGeoData(currentMapCache.mapBounds, currentMapCache.zoom);

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
      <Marker position={currentMapCache.selectedCoordinates}>
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

  return (
    <div className="tab-map-container">
      <MapOptions />
      <MapContainer
        center={currentMapCache.mapCenter}
        zoom={currentMapCache.zoom}
        className="map"
      >
        {geoData && <GeoJSON data={geoData} />}

        <MapEventsHandler />
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
      </MapContainer>
    </div>
  );
};

export default MapView;
