import React from "react";
import PopUp from "./PopUp";
import { TabProps } from "../../contexts/TabsContext";
import { Divider } from "@mui/material";
import { Article, MapPin, Hash } from "@phosphor-icons/react";

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
          <Article size={20} weight="bold" /> <b>Dataset Description</b>
        </span>
        <span className="secondary-text" style={{ paddingBottom: "1rem" }}>
          {currentTab.dataset.metaData?.longDescription}
        </span>
        <Divider />
        <div className="tab-info-additional">
          <span
            className="overline text-header"
            style={{ paddingBottom: "1rem" }}
          >
            <MapPin size={20} weight="bold" />
            <b>Display Type: </b>
            <span
              className="secondary-text"
              style={{ textTransform: "capitalize" }}
            >
              {currentTab.dataset.metaData
                ? currentTab.dataset.metaData.type
                : "None"}
            </span>
          </span>
          <span
            className="overline text-header"
            style={{ paddingBottom: "1rem" }}
          >
            <Hash size={20} weight="bold" />
            <b>Total Data Points: </b>
            <span
              className="secondary-text"
              style={{ textTransform: "capitalize" }}
            >
              {currentTab.dataset.metaData
                ? currentTab.dataset.metaData.tables.reduce(
                    (sum, table) => sum + table.numberOfLines,
                    0
                  )
                : "0"}
            </span>
          </span>
        </div>
      </div>
    </PopUp>
  );
};

export default TabInfoPopUp;
