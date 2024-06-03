import DataPanel from "./DataPanel";
import "./DataView.css";
import Button from "@mui/material/Button";
import { useContext, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { Box, TextField } from "@mui/material";
import { Funnel, MapPin } from "@phosphor-icons/react";

function DataView() {
  // Access the tabs context
  const { currentTabsCache } = useContext(TabsContext);
  // Filter data
  const [filterValue, setFilterValue] = useState("");

  const handleFilterChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilterValue(event.target.value);
  };

  // Function to always return the title of the currently opened tab
  const getCurrentTabTitle = (): string => {
    const currentTabID = currentTabsCache.currentTabID.toString();
    const currentTab = currentTabsCache.openedTabs.find(
      (tab) => tab.id === currentTabID
    );

    return currentTab ? currentTab.dataset.displayName : "No map loaded";
  };

  return (
    <div className="dataview-container">
      <div className="dataview-header-container">
        <b className="dataview-header-title">
          <MapPin size={20} /> Nuremberg
        </b>
        <Box id="filter-panel">
          <TextField
            label={
              <div className="search-box-label">
                <Funnel size={20} /> Filter data
              </div>
            }
            variant="outlined"
            size="small"
            value={filterValue}
            onChange={handleFilterChange}
          />
        </Box>
      </div>
      <DataPanel listTitle={getCurrentTabTitle()} filterValue={filterValue} />
      <Button variant="outlined">Load data</Button>
    </div>
  );
}

export default DataView;
