import { useState } from "react";
import { MapContainer } from "react-leaflet/MapContainer";
import { Marker } from "react-leaflet/Marker";
import { Popup } from "react-leaflet/Popup";
import { TileLayer } from "react-leaflet/TileLayer";
import "leaflet/dist/leaflet.css";
import "./MapView.css";
import { useMap, useMapEvents } from "react-leaflet/hooks";
import L, { LatLng, LatLngBounds } from "leaflet";
import Button from "@mui/material/Button";
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";
import MapOptions from "./MapOptions";
import useGeoData, { fetchData } from "./DataFetch";
import { GeoJSON } from "react-leaflet";

const DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
  iconAnchor: [12, 40],
});

L.Marker.prototype.options.icon = DefaultIcon;

function Btn() {
  const map = useMap();
  return (
    <Button
      variant="text"
      onClick={() => {
        map.locate().on("locationfound", function (e) {
          map.flyTo(e.latlng, map.getZoom());
        });
      }}
    >
      Text
    </Button>
  );
}

const MapView: React.FC = () => {
  const center: LatLng = L.latLng([49.5732, 11.0288]); // Initial center coordinates
  const [bounds, setBounds] = useState<LatLngBounds>(
    new LatLngBounds([0, 0], [0, 0])
  );
  const [zoom, setZoom] = useState<number>(13);
  const geoData = useGeoData(bounds, zoom);
  const [markerPosition, setMarkerPosition] = useState<LatLng>(center);

  const UpdateBounds = () => {
    useMapEvents({
      moveend: (event) => {
        setBounds(event.target.getBounds());
        setZoom(event.target.getZoom());
      },
    });
    return null;
  };

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
    <div className="tab-map-container">
      <MapOptions />

      <MapContainer center={center} zoom={13} className="map">
        {geoData && <GeoJSON data={geoData} />}
        <UpdateBounds />

        <MapEventsHandler />
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <Btn />
      </MapContainer>
    </div>
  );
};

export default MapView;
