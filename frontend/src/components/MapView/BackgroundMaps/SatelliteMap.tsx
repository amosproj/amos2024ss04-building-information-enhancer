import L from "leaflet";
import { TileLayer, WMSTileLayer } from "react-leaflet";

const SatelliteMap = () => {
  return (
    <div>
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
        url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
      />
      <WMSTileLayer
        url="https://sg.geodatenzentrum.de/wms_sentinel2_de"
        layers="rgb_2020"
        format="image/png"
        transparent={true}
        attribution='&copy; Bundesamt für Kartographie und Geodäsie (BKG), Bayerische Vermessungverwaltung,  <a href="http://sg.geodatenzentrum.de/web_public/gdz/datenquellen/Datenquellen_TopPlusOpen.pdf">Sources</a>'
        bounds={L.latLngBounds([47.141, 5.561], [55.054, 15.579])}
      />
    </div>
  );
};

export default SatelliteMap;
