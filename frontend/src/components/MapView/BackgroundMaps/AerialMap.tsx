import L from "leaflet";
import { TileLayer, WMSTileLayer } from "react-leaflet";

const AerialMap = () => {
  return (
    <div>
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
        url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
      />
      <WMSTileLayer
        url="https://geoservices.bayern.de/od/wms/dop/v1/dop40?"
        layers="by_dop40c"
        format="image/png"
        transparent={true}
        attribution="&copy; © Europäische Union, enthält Copernicus Sentinel-2 Daten 2020, verarbeitet durch das Bundesamt für Kartographie und Geodäsie (BKG)"
        bounds={L.latLngBounds([47.141, 5.561], [55.054, 15.579])}
      />
    </div>
  );
};

export default AerialMap;
