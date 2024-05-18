import { Fragment, useState } from "react";
import "./MultiMap.css";
import Tab from "@mui/material/Tab";
import TabContext from "@mui/lab/TabContext";
import TabList from "@mui/lab/TabList";
import TabPanel from "@mui/lab/TabPanel";
import {
  Plus,
  X,
  PushPin,
  PushPinSlash,
  PushPinSimple,
} from "@phosphor-icons/react";
import Tooltip from "@mui/material/Tooltip";
import MapView from "../MapView/MapView";

// Tab
type TabProps = {
  id: string;
  title: string;
  description: string;
  ifPinned: boolean;
};

// Default tabs array to show at the start of the website
const defaultTabs = [
  {
    id: "1",
    title: "Charging Stations",
    description: "Here is the short description of the Charging Stations Map.",
    ifPinned: false,
  },
];

const MultiMap = () => {
  // Keeps track of the current tab focused
  const [currentTab, setCurrentTab] = useState<string>("1");
  // Keeps track of the opened tabs
  const [openedTabs, setOpenedTabs] = useState<TabProps[]>(defaultTabs);

  // Handles the change of the current tab
  const handleChange = (_event: React.SyntheticEvent, newTab: number) => {
    setCurrentTab(newTab.toString());
  };

  // Opens a new tab
  const openNewTab = () => {
    const newTab: TabProps = {
      id: (openedTabs.length + 1).toString(),
      title: `New Tab ${openedTabs.length + 1}`,
      description: `Description for Tab ${openedTabs.length + 1}`,
      ifPinned: false,
    };
    setOpenedTabs([...openedTabs, newTab]);
  };

  // Closes a specific tab
  const closeTab = (id: string) => {
    // Check if this is the last tab
    if (openedTabs.length === 1) {
      alert("The last tab can not be closed.");
      return;
    } else if (openedTabs.find((tab) => tab.id === id)?.ifPinned === true) {
      alert("You can not close a pinned tab.");
      return;
    }
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

  // Toggle pinning of a specific tab
  const toggleTabPinned = (tabId: string) => {
    setOpenedTabs((prevTabs) =>
      prevTabs.map((tab) =>
        tab.id === tabId ? { ...tab, ifPinned: !tab.ifPinned } : tab
      )
    );
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
                        {tab.ifPinned ? (
                          <PushPinSimple
                            weight="fill"
                            className="pinned-tab-icon"
                            onClick={() => {
                              closeTab(tab.id);
                            }}
                          />
                        ) : (
                          <Fragment />
                        )}

                        <Tooltip title="Close Tab" arrow>
                          <X
                            className="close-tab-icon"
                            onClick={() => {
                              closeTab(tab.id);
                            }}
                          />
                        </Tooltip>
                        <Tooltip
                          title={!tab.ifPinned ? "Pin Tab" : "Unpin Tab"}
                          arrow
                          onClick={() => {
                            toggleTabPinned(tab.id);
                          }}
                        >
                          <div className="pin-tab-container">
                            {!tab.ifPinned ? (
                              <PushPin className="pin-tab-icon" />
                            ) : (
                              <PushPinSlash
                                weight="fill"
                                className="unpin-tab-icon"
                              />
                            )}
                          </div>
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
          <Tooltip title="Add a new dataset" arrow>
            <button className="add-tab-button" onClick={openNewTab}>
              <Plus />
            </button>
          </Tooltip>
        </div>
        {openedTabs.map((tab: TabProps) => {
          return (
            <TabPanel value={tab.id.toString()} className="tab" key={tab.id}>
              <span className="tab-description-container">
                {tab.description}
              </span>
              <div className="tab-map-container">
                <MapView />
              </div>
            </TabPanel>
          );
        })}
      </TabContext>
    </div>
  );
};

export default MultiMap;
