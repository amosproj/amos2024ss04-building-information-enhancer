import { useContext, useState } from "react";
import "leaflet/dist/leaflet.css";
import "leaflet.markercluster/dist/MarkerCluster.css";
import "leaflet.markercluster/dist/MarkerCluster.Default.css";
import "leaflet.markercluster";
import "./MapView.css";
import L from "leaflet";
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";
import MapOptions from "./MapOptions";
import LeafletMap from "./LeafletMap";
import ThreeDView from "../ThreeDView/ThreeDView";
import { MapTypes } from "../../types/MapTypes";
import { MapContext } from "../../contexts/MapContext";

const DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
  iconAnchor: [12, 40],
});

L.Marker.prototype.options.icon = DefaultIcon;

interface MapViewProps {
  datasetId: string;
}

/**
 * One of the main components on the website. Displays the map on the left side of the page.
 */
const MapView: React.FC<MapViewProps> = ({ datasetId }) => {
  const [if3D, setIf3D] = useState<boolean>(false);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);

  /**
   * Changes the layer type
   * @param type type of the layer
   */
  const handleMapTypeChange = (type: MapTypes) => {
    setCurrentMapCache({ ...currentMapCache, mapType: type });
  };

  /**
   * Toggles the 3D View
   */
  const toggle3D = () => {
    setIf3D(!if3D);
  };

  return (
    <div className="tab-map-container">
      {if3D ? (
        <ThreeDView datasetId={datasetId} />
      ) : (
        <LeafletMap datasetId={datasetId} if3D={false} />
      )}
      <MapOptions
        onMapTypeChange={handleMapTypeChange}
        if3D={if3D}
        toggle3D={toggle3D}
      />
    </div>
  );
};

export default MapView;
