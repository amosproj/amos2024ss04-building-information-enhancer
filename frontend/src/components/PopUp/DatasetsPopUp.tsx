import React from "react";
import PopUp from "./PopUp";
import DatasetsList from "../DatasetsList/DatasetsList";

interface DatasetsPopUpProps {
  onToggleIfOpenedDialog: () => void;
  ifOpenedDialog: boolean;
}

/**
 * Pop-up for list of the datasets.
 */
const DatasetsPopUp: React.FC<DatasetsPopUpProps> = ({
  onToggleIfOpenedDialog,
  ifOpenedDialog,
}) => {
  return (
    <PopUp
      title="Available Datasets"
      onClose={onToggleIfOpenedDialog}
      ifOpenedDialog={ifOpenedDialog}
      titleIcon={undefined}
    >
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          textAlign: "center",
        }}
      >
        <span className="secondary-text" style={{ paddingBottom: "1rem" }}>
          Select one of the datasets below to create a new map tab
        </span>
        <DatasetsList closeDialog={onToggleIfOpenedDialog} />
      </div>
    </PopUp>
  );
};

export default DatasetsPopUp;
