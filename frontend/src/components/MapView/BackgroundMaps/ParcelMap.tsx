import { TileLayer, WMSTileLayer } from "react-leaflet";

const ParcelMap = () => {
  return (
    <div>
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
        url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
      />
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.de/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
        url="https://wmtsod1.bayernwolke.de/wmts/by_webkarte/{s}/{z}/{x}/{y}"
      />
      <WMSTileLayer
        url="https://geoservices.bayern.de/wms/v1/ogc_alkis_parzellarkarte.cgi?"
        layers="by_alkis_parzellarkarte_farbe"
      />
    </div>
  );
};

export default ParcelMap;
