import DataPanel from "./DataPanel";
import "./DataView.css";

import Button from "@mui/material/Button";
import TabPanel from "@mui/lab/TabPanel/TabPanel";
import TabContext from "@mui/lab/TabContext/TabContext";
import Tab from "@mui/material/Tab/Tab";
import TabList from "@mui/lab/TabList/TabList";

function DataView() {
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
            <Tab label="Nuremberg" value="1"></Tab>
          </TabList>
        </div>

        <TabPanel value="1" className="tab dataview-tab">
          <div className="datapanels-container">
            <div className="data-panels-container">
              <DataPanel listTitle="Map Name" filterPanelId={1} />
              <DataPanel listTitle="General Data" filterPanelId={2} />
              <DataPanel listTitle="Extra Capabilities" filterPanelId={3} />
            </div>
            <Button variant="outlined">Load data</Button>
          </div>
        </TabPanel>
      </TabContext>
    </div>
  );
}

export default DataView;
