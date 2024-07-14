import { Tooltip } from "@mui/material";
import { Plus } from "@phosphor-icons/react";
import { useState } from "react";
import DatasetsPopUp from "../PopUp/DatasetsPopUp";
import "./NewTabButton.css";

/**
 * The new tab button for adding a new tab.
 */
const NewTabButton = () => {
  // Stores the state of if the datasets popup is open
  const [ifOpenedDialog, setIfOpenedDialog] = useState(false);

  /**
   * Toggles the new tab dialog.
   */
  const toggleIfOpenedDialog = () => {
    setIfOpenedDialog(!ifOpenedDialog);
  };

  return (
    <div>
      <Tooltip title="Add a new dataset" arrow>
        <button className="add-tab-button" onClick={toggleIfOpenedDialog}>
          <Plus />
        </button>
      </Tooltip>
      <DatasetsPopUp
        onToggleIfOpenedDialog={toggleIfOpenedDialog}
        ifOpenedDialog={ifOpenedDialog}
      />
    </div>
  );
};

export default NewTabButton;
