import { TileLayer } from "react-leaflet";

const NormalMap = () => {
  return (
    <div>
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
    </div>
  );
};

export default NormalMap;
