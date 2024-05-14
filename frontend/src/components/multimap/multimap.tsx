import { useState } from "react";
import "./multimap.css";
import Tab from "@mui/material/Tab";
import TabContext from "@mui/lab/TabContext";
import TabList from "@mui/lab/TabList";
import TabPanel from "@mui/lab/TabPanel";
import { Add, Close } from "@mui/icons-material";
import Tooltip from "@mui/material/Tooltip";

// Tab
type TabType = {
  id: string;
  title: string;
  content: string;
};

// Default tabs array to show at the start of the website
const defaultTabs = [
  { id: "1", title: "Charging Stations", content: "Charging Stations Map" },
];

const MultiMap = () => {
  // Keeps track of the current tab focused
  const [currentTab, setCurrentTab] = useState<string>("1");
  // Keeps track of the opened tabs
  const [openedTabs, setOpenedTabs] = useState<TabType[]>(defaultTabs);

  // Handles the change of the current tab
  const handleChange = (_event: React.SyntheticEvent, newTab: number) => {
    setCurrentTab(newTab.toString());
  };

  // Opens a new tab
  const openNewTab = () => {
    const newTab: TabType = {
      id: (openedTabs.length + 1).toString(),
      title: `New Tab ${openedTabs.length + 1}`,
      content: `Content for Tab ${openedTabs.length + 1}`,
    };
    setOpenedTabs([...openedTabs, newTab]);
  };

  // Closes a specific tab
  const closeTab = (id: string) => {
    // Close the tab
    const newOpenedTabs = openedTabs.filter((tab) => tab.id !== id);
    // Reorder the tabs IDs
    let startingID = 0;
    const updatedOpenedTabs = newOpenedTabs.map((tab) => {
      startingID = startingID + 1;
      return {
        ...tab,
        id: startingID.toString(),
      };
    });
    // Set the new tabs state
    setOpenedTabs(updatedOpenedTabs);
    // Update the newest current tab
    if (Number(currentTab) > openedTabs.length - 1) {
      setCurrentTab((openedTabs.length - 1).toString());
    }
  };

  return (
    <div className="multimap-container">
      <TabContext value={currentTab}>
        <div className="tab-list-container">
          <TabList
            variant="scrollable"
            scrollButtons="auto"
            onChange={handleChange}
            aria-label="lab API multimap tabs"
            selectionFollowsFocus
          >
            {openedTabs.map((tab) => {
              return (
                <Tab
                  label={
                    <div className="tab-title-container">
                      <span>{tab.title}</span>
                      <div className="tab-icons-container">
                        <Tooltip title="Close Tab">
                          <Close
                            onClick={() => {
                              closeTab(tab.id);
                            }}
                          />
                        </Tooltip>
                      </div>
                    </div>
                  }
                  value={tab.id.toString()}
                  key={tab.id}
                ></Tab>
              );
            })}
          </TabList>
          <Tooltip title="Add a new dataset">
            <button className="add-tab-button" onClick={openNewTab}>
              <Add />
            </button>
          </Tooltip>
        </div>
        {openedTabs.map((tab: TabType) => {
          return (
            <TabPanel value={tab.id.toString()} className="tab" key={tab.id}>
              {tab.content}
            </TabPanel>
          );
        })}
      </TabContext>
    </div>
  );
};

export default MultiMap;
