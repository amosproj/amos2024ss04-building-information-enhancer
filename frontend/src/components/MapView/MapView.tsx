import { useState } from "react";
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
  const [if3D, setIf3D] = useState<boolean>(false);
  const [mapType, setMapType] = useState<
    "normal" | "satellite" | "parcel" | "aerial"
  >("normal");

  /**
   * Changes the layer type
   * @param type type of the layer
   */
  const handleMapTypeChange = (
    type: "normal" | "satellite" | "parcel" | "aerial"
  ) => {
    setMapType(type);
  };

  /**
   * Toggles the 3D View
   */
  const toggle3D = () => {
    console.log("3D");
    setIf3D(!if3D);
  };

  return (
    <div className="tab-map-container">
      {if3D ? (
        <ThreeDView datasetId={datasetId} mapType={mapType} />
      ) : (
        <LeafletMap datasetId={datasetId} mapType={mapType} />
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
