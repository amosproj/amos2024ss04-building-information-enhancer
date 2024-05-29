import React from "react";
import PopUp from "./PopUp";
import { TabProps } from "../../contexts/TabsContext";

interface TabInfoPopUpProps {
  onToggleIfOpenedDialog: () => void;
  ifOpenedDialog: boolean;
  currentTab: TabProps;
}

const TabInfoPopUp: React.FC<TabInfoPopUpProps> = ({
  onToggleIfOpenedDialog,
  ifOpenedDialog,
  currentTab,
}) => {
  return (
    <PopUp
      title={currentTab.dataset.displayName}
      onClose={onToggleIfOpenedDialog}
      ifOpenedDialog={ifOpenedDialog}
      titleIcon={currentTab.dataset.datasetIcon}
    >
      <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
        <span className="secondary-text" style={{ paddingBottom: "1rem" }}>
          {currentTab.dataset.description}
        </span>
      </div>
    </PopUp>
  );
};

export default TabInfoPopUp;
