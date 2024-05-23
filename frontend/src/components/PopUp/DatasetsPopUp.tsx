import React from "react";
import PopUp from "./PopUp";
import DatasetsList from "../DatasetsList/DatasetsList";

interface DatasetsPopUpProps {
  onToggleIfOpenedDialog: () => void;
  ifOpenedDialog: boolean;
}

const DatasetsPopUp: React.FC<DatasetsPopUpProps> = ({
  onToggleIfOpenedDialog,
  ifOpenedDialog,
}) => {
  return (
    <PopUp
      title="Available Datasets"
      onClose={onToggleIfOpenedDialog}
      ifOpenedDialog={ifOpenedDialog}
    >
      <div style={{ display: "flex", flexDirection: "column" }}>
        <span className="secondary-text" style={{ paddingBottom: "1rem" }}>
          Select one of the datasets below to create a new map tab
        </span>
        <DatasetsList />
      </div>
    </PopUp>
  );
};

export default DatasetsPopUp;
