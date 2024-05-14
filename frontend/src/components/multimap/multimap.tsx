import { useState } from "react";
import "./multimap.css";
import Box from "@mui/material/Box";
import Tab from "@mui/material/Tab";
import TabContext from "@mui/lab/TabContext";
import TabList from "@mui/lab/TabList";
import TabPanel from "@mui/lab/TabPanel";
import { Add } from "@mui/icons-material";

const MultiMap = () => {
  const [currentTab, setCurrentTab] = useState(0);

  const handleChange = (event: React.SyntheticEvent, newTab: number) => {
    setCurrentTab(newTab);
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
            <Tab label="Charging stations" value="0" />
            <Tab label="Environmental Index" value="1" />
            <Tab label="Socioeconomic Impact" value="2" />
          </TabList>
          <button className="add-tab-button">
            <Add />
          </button>
        </div>
        <TabPanel value="0" className="tab">
          Charging stations Map
        </TabPanel>
        <TabPanel value="1" className="tab">
          Environmental Index Map
        </TabPanel>
        <TabPanel value="2" className="tab">
          Socioeconomic Impact Map
        </TabPanel>
      </TabContext>
    </div>
  );
};

export default MultiMap;
