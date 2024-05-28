import DataPanel from "./DataPanel";
import "./DataView.css";
import FilterBar from "./FilterBar";

import Button from "@mui/material/Button";
import TabPanel from "@mui/lab/TabPanel/TabPanel";
import TabContext from "@mui/lab/TabContext/TabContext";
import Tab from "@mui/material/Tab/Tab";
import TabList from "@mui/lab/TabList/TabList";
import { CaretDown } from "@phosphor-icons/react";
import { useContext, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import SearchPopUp from "../PopUp/SearchPopUp";

function DataView() {
  // Access the tabs context
  const { currentTabsCache } = useContext(TabsContext);

  // Function to always return the title of the currently opened tab
  const getCurrentTabTitle = (): string => {
    const currentTabID = currentTabsCache.currentTabID.toString();
    const currentTab = currentTabsCache.openedTabs.find(
      (tab) => tab.id === currentTabID
    );

    return currentTab ? currentTab.dataset.displayName : "No map loaded";
  };

  // Stores the state of if the search popup is open
  const [ifOpenedDialog, setIfOpenedDialog] = useState(false);
  const toggleIfOpenedDialog = () => {
    setIfOpenedDialog(!ifOpenedDialog);
  };

  
  return (
    <div className="dataview-container">
      <TabContext value="1">
        <div className="tab-list-container">
          <TabList
            variant="scrollable"
            scrollButtons="auto"
            onChange={() => {}}
            aria-label="lab API multimap tabs"
            selectionFollowsFocus
          >
            <Tab
              label={
                <div className="dataview-title-container">
                  <span>Nuremberg</span>
                  <div className="favourite-icon-container">
                    <CaretDown
                      weight="bold"
                      className="location-icon"
                      onClick={toggleIfOpenedDialog}
                    />
                    <SearchPopUp
                      onToggleIfOpenedDialog={toggleIfOpenedDialog}
                      ifOpenedDialog={ifOpenedDialog}
                    ></SearchPopUp>
                  </div>
                </div>
              }
              value="1"
            ></Tab>
          </TabList>
        </div>

        <TabPanel value="1" className="tab dataview-tab">
          <div className="datapanels-container">
            <div className="data-panels-container">

              <DataPanel listTitle={getCurrentTabTitle()} />

            </div>
            <Button variant="outlined">Load data</Button>
          </div>
        </TabPanel>
      </TabContext>
    </div>
  );
}

export default DataView;
