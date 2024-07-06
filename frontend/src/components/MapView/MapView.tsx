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

  return (
    <div className="tab-map-container">
      <LeafletMap datasetId={datasetId} mapType={mapType} />
      <MapOptions onMapTypeChange={handleMapTypeChange} />
    </div>
  );
};

export default MapView;
