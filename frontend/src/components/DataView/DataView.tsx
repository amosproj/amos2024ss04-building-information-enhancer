import DataPanel from "./DataPanel";
import "./DataView.css";
import Button from "@mui/material/Button";
import { Fragment, useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { Box, TextField } from "@mui/material";
import { ArrowsClockwise, Funnel, MapPin } from "@phosphor-icons/react";
import { MapContext } from "../../contexts/MapContext";

function DataView() {
  // Access the tabs context
  const { currentTabsCache } = useContext(TabsContext);
  // Filter data
  const [filterValue, setFilterValue] = useState("");
  // Map selection - Reload data
  const { currentMapCache } = useContext(MapContext);
  const [ifNeedsReloading, setIfNeedsReloading] = useState(false);
  const [lastSelectedCoords, setLastSelectedCoords] = useState(
    currentMapCache.selectedCoordinates
  );

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

  // Changes the reload data button
  useEffect(() => {
    if (
      !ifNeedsReloading &&
      currentMapCache.selectedCoordinates !== null &&
      lastSelectedCoords !== currentMapCache.selectedCoordinates
    ) {
      setIfNeedsReloading(true);
    }
    setLastSelectedCoords(currentMapCache.selectedCoordinates);
  }, [currentMapCache, ifNeedsReloading, lastSelectedCoords]);

  const reloadData = () => {
    setIfNeedsReloading(false);
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
      {ifNeedsReloading ? (
        <div className="load-data-container">
          <div className="load-data-button" onClick={reloadData}>
            <Button
              variant="contained"
              color={ifNeedsReloading ? "warning" : "info"}
            >
              {ifNeedsReloading ? (
                <div className="load-data-reload">
                  <ArrowsClockwise />
                  Reload Data
                </div>
              ) : (
                "Load data"
              )}
            </Button>
          </div>
        </div>
      ) : (
        <Fragment />
      )}
    </div>
  );
}

export default DataView;
