import { useState } from "react";
import { MagnifyingGlass } from "@phosphor-icons/react";
import "./MapOptions.css";
import { Tooltip } from "@mui/material";
import SearchPopUp from "../PopUp/SearchPopUp";

const MapOptions: React.FC = () => {
  // Stores the state of if the search popup is open
  const [ifOpenedDialog, setIfOpenedDialog] = useState(false);
  const toggleIfOpenedDialog = () => {
    setIfOpenedDialog(!ifOpenedDialog);
  };

  return (
    <div className="map-options-container">
      <Tooltip arrow title="Search for an address">
        <MagnifyingGlass
          weight="duotone"
          className="search-map-icon"
          onClick={toggleIfOpenedDialog}
        />
      </Tooltip>
      <SearchPopUp
        onToggleIfOpenedDialog={toggleIfOpenedDialog}
        ifOpenedDialog={ifOpenedDialog}
      />
    </div>
  );
};

export default MapOptions;
