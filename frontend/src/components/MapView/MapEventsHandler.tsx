import { Fragment, useContext } from "react";
import { Marker } from "react-leaflet/Marker";
import { Popup } from "react-leaflet/Popup";
import { useMap, useMapEvents } from "react-leaflet/hooks";
import { MapContext } from "../../contexts/MapContext";
import L, { DivIcon } from "leaflet";
import { MapPin } from "@phosphor-icons/react";
import { createRoot } from "react-dom/client";
import { flushSync } from "react-dom";

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

const MapEventsHandler = () => {
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);

  const setPosition = (latlng: L.LatLng) => {
    setCurrentMapCache({ ...currentMapCache, selectedCoordinates: latlng });
  };

  const map = useMap();
  // Add events
  useMapEvents({
    click: (event) => {
      console.log(event);
      currentMapCache.polygon?.remove();
      setCurrentMapCache({
        ...currentMapCache,
        selectedCoordinates: event.latlng,
        polygon: null,
      });
    },
    moveend: (event) => {
      console.log("updates");
      setCurrentMapCache({
        ...currentMapCache,
        mapCenter: event.target.getCenter(),
        mapBounds: event.target.getBounds(),
        zoom: event.target.getZoom(),
      });
    },
  });

  return currentMapCache.selectedCoordinates !== null ? (
    <Marker position={currentMapCache.selectedCoordinates} icon={divIconMarker}>
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
  ) : (
    <Fragment />
  );
};

export default MapEventsHandler;
