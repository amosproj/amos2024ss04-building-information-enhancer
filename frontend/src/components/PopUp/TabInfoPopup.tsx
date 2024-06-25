import React from "react";
import PopUp from "./PopUp";
import { TabProps } from "../../contexts/TabsContext";
import { Divider } from "@mui/material";
import { Article, MapPin } from "@phosphor-icons/react";

import "./TabInfoPopUp.css";

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
        <Divider />
        <span
          className="overline text-header"
          style={{ paddingBottom: "1rem" }}
        >
          <Article size={20} /> Dataset Description
        </span>
        <span className="secondary-text" style={{ paddingBottom: "1rem" }}>
          {currentTab.dataset.metaData?.longDescription}
        </span>
        <Divider />
        <span
          className="overline text-header"
          style={{ paddingBottom: "1rem" }}
        >
          <MapPin size={20} />
          Marker Type
        </span>
        <span
          className="secondary-text"
          style={{ paddingBottom: "1rem", textTransform: "capitalize" }}
        >
          {currentTab.dataset.type}
        </span>
      </div>
    </PopUp>
  );
};

export default TabInfoPopUp;
