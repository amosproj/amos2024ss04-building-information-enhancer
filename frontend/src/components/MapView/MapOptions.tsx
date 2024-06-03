//import { useState } from "react";
//import { Stack } from "@phosphor-icons/react";
//import { useState } from "react";
import { ArrowsClockwise } from "@phosphor-icons/react";
import "./MapOptions.css";
import { Tooltip } from "@mui/material";
//import SearchPopUp from "../PopUp/SearchPopUp";
import SearchBar from "../PopUp/SearchBar";

interface MapOptionsProps {
  toggleShowSatellite: () => void;
}

const MapOptions: React.FC<MapOptionsProps> = ({ toggleShowSatellite }) => {
  // Stores the state of if the search popup is open
  //const [ifOpenedDialog, setIfOpenedDialog] = useState(false);
  // const toggleIfOpenedDialog = () => {
  //   setIfOpenedDialog(!ifOpenedDialog);
  // };

  return (
    <div className="map-options-container">
      <div className="search-bar">
      <SearchBar/>
      </div>
      {/* <Tooltip arrow title="Search for an address" placement="right">
        <div className="search-map-icon-container leaflet-bar leaflet-control leaflet-control-custom">
          <MagnifyingGlass
            className="search-map-icon"
            onClick={toggleIfOpenedDialog}
          />
        </div>
      </Tooltip> */}
      <Tooltip arrow title="Switch layers" placement="right">
        <div className="layers-map-icon-container leaflet-touch leaflet-bar leaflet-control leaflet-control-custom">
          <ArrowsClockwise
            /** ArrowsClockwise requested from po. stack only if we have more than 2 backgrounds */ className="layers-map-icon"
            onClick={toggleShowSatellite}
          />
        </div>
      </Tooltip>
      {/* <SearchPopUp
        onToggleIfOpenedDialog={toggleIfOpenedDialog}
        ifOpenedDialog={ifOpenedDialog}
      /> */}
    </div>
  );
};

export default MapOptions;
