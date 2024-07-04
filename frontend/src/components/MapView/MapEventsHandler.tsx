import React, { Fragment, useContext } from "react";
import { Marker } from "react-leaflet";
import { useMapEvents } from "react-leaflet/hooks";
import { MapContext } from "../../contexts/MapContext";
import L, { DivIcon, LatLng } from "leaflet";
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

const MapEventsHandler: React.FC = () => {
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);

  // Add events
  useMapEvents({
    click: (event) => {
      if (!currentMapCache.isDrawing) {
        currentMapCache.polygon?.remove();
        setCurrentMapCache({
          ...currentMapCache,
          selectedCoordinates: event.latlng,
          polygon: null,
        });
      }
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

  return currentMapCache.selectedCoordinates !== null &&
    currentMapCache.selectedCoordinates instanceof LatLng ? (
    <Marker
      position={currentMapCache.selectedCoordinates}
      icon={divIconMarker}
    />
  ) : (
    <Fragment />
  );
};

export default MapEventsHandler;
