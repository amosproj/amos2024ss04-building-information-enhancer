import { useState } from "react";
import "./multimap.css";
import Tab from "@mui/material/Tab";
import TabContext from "@mui/lab/TabContext";
import TabList from "@mui/lab/TabList";
import TabPanel from "@mui/lab/TabPanel";
import { Add } from "@mui/icons-material";

// Tab
type Tab = {
  id: number;
  title: string;
  content: string;
};

// Default tabs array to show at the start of the website
const defaultTabs = [
  { id: 1, title: "Charging Stations", content: "Charging Stations Map" },
];

const MultiMap = () => {
  // Keeps track of the current tab focused
  const [currentTab, setCurrentTab] = useState<string>("1");
  // Keeps track of the opened tabs
  const [openedTabs, setOpenedTabs] = useState<Tab[]>(defaultTabs);

  // Handles the change of the current tab
  const handleChange = (event: React.SyntheticEvent, newTab: number) => {
    setCurrentTab(newTab.toString());
  };

  // Opens a new tab
  const openNewTab = () => {
    const newTab = {
      id: openedTabs.length + 1,
      title: `New Tab ${openedTabs.length + 1}`,
      content: `Content for Tab ${openedTabs.length + 1}`,
    };
    setOpenedTabs([...openedTabs, newTab]);
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
                <Tab label={tab.title} value={tab.id.toString()} key={tab.id} />
              );
            })}
          </TabList>
          <button className="add-tab-button" onClick={openNewTab}>
            <Add />
          </button>
        </div>
        {openedTabs.map((tab: Tab) => {
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
