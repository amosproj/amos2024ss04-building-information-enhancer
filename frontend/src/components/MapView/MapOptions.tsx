import { StackSimple } from "@phosphor-icons/react";
import "./MapOptions.css";
import { Tooltip } from "@mui/material";
import SearchBar from "../PopUp/SearchBar";

interface MapOptionsProps {
  toggleShowSatellite: () => void;
}

const MapOptions: React.FC<MapOptionsProps> = ({ toggleShowSatellite }) => {
  return (
    <div className="map-options-container">
      <div className="search-bar">
        <SearchBar />
      </div>
      <Tooltip arrow title="Switch layers" placement="right">
        <div className="layers-map-icon-container leaflet-touch leaflet-bar leaflet-control leaflet-control-custom">
          <StackSimple
            className="layers-map-icon"
            onClick={toggleShowSatellite}
          />
        </div>
      </Tooltip>
    </div>
  );
};

export default MapOptions;
