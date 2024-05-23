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
      <span className="secondary-text">
        Select one of the datasets below to load it on the map
      </span>
      <DatasetsList />
    </PopUp>
  );
};

export default DatasetsPopUp;
