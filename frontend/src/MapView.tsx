import { useState } from "react";
import { MapContainer } from "react-leaflet/MapContainer";
import { Marker } from "react-leaflet/Marker";
import { Popup } from "react-leaflet/Popup";
import { TileLayer } from "react-leaflet/TileLayer";
import "leaflet/dist/leaflet.css";
import { useMap, useMapEvents } from "react-leaflet/hooks";
import L, { LatLng } from "leaflet";
import Button from "@mui/material/Button";

const MapView: React.FC = ({}) => {
  const center: LatLng = L.latLng([49.5732, 11.0288]); // Initial center coordinates
  const [markerPosition, setMarkerPosition] = useState<LatLng>(center);

  const MapEventsHandler = () => {
    const map = useMap();
    useMapEvents({
      click: (e) => {
        setMarkerPosition(e.latlng);
      },
    });
    return (
      <Marker position={markerPosition}>
        <Popup>
          <span
            onClick={() => {
              map
                .locate({ setView: true })
                .on("locationfound", function (e) {
                  setPosition(e.latlng);
                  map.flyTo(e.latlng, map.getZoom(), {
                    animate: true,
                    duration: 50,
                  });
                })
                .on("locationerror", function (e) {
                  console.log(e);
                  alert("Location access denied.");
                });
            }}
          >
            {markerPosition.lat.toFixed(4)}; | {markerPosition.lng.toFixed(4)};
          </span>
        </Popup>
      </Marker>
    );
  };

  function setPosition(latlng: L.LatLng) {
    setPosition(latlng);
  }

  return (
    <div className="map-container" style={{ height: "400px" }}>
      <MapContainer center={center} zoom={13} style={{ height: "400px" }}>
        <MapEventsHandler />
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <Button
          variant="text"
          onClick={() => {
            const map = useMap();
            map.locate().on("locationfound", function (e) {
              setPosition(e.latlng);
              map.flyTo(e.latlng, map.getZoom());
            });
          }}
        >
          Text
        </Button>
      </MapContainer>
    </div>
  );
};

export default MapView;
