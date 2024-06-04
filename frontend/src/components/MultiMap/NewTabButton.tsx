import { Tooltip } from "@mui/material";
import { Plus } from "@phosphor-icons/react";
import { useState } from "react";
import DatasetsPopUp from "../PopUp/DatasetsPopUp";

const NewTabButton = () => {
  // Stores the state of if the datasets popup is open
  const [ifOpenedDialog, setIfOpenedDialog] = useState(false);
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
