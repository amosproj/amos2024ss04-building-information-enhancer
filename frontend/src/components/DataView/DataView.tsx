import DataPanel from "./DataPanel";
import "./DataView.css";
import { Fragment, useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { Box, TextField } from "@mui/material";
import { Funnel, MapPin, Warning } from "@phosphor-icons/react";
import { MapContext } from "../../contexts/MapContext";
import LoadDataButton from "./LoadDataButton";

function DataView() {
  // Access the tabs context
  const { currentTabsCache } = useContext(TabsContext);
  // Filter data
  const [filterValue, setFilterValue] = useState("");
  // Map selection - Reload data
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const [ifNeedsReloading, setIfNeedsReloading] = useState(false);

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
      currentMapCache.loadedCoordinates !== currentMapCache.selectedCoordinates
    ) {
      setIfNeedsReloading(true);
    }
  }, [currentMapCache, ifNeedsReloading]);

  const reloadData = () => {
    setIfNeedsReloading(false);
    setCurrentMapCache({
      ...currentMapCache,
      loadedCoordinates: currentMapCache.selectedCoordinates,
    });
  };

  return (
    <div className="dataview-container">
      {currentMapCache.loadedCoordinates ? (
        <Fragment>
          <div className="dataview-header-container">
            <b className="dataview-header-title">
              <MapPin size={20} /> Nuremberg
            </b>
            <Box id="filter-panel" style={{ maxWidth: "18rem", width: "100%" }}>
              <TextField
                style={{ width: "100%" }}
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
          <DataPanel
            listTitle={getCurrentTabTitle()}
            filterValue={filterValue}
          />
          {ifNeedsReloading ? (
            <div className="load-data-container">
              <div className="load-data-button" onClick={reloadData}>
                <LoadDataButton
                  disabled={false}
                  ifNeedsReloading={ifNeedsReloading}
                />
              </div>
            </div>
          ) : (
            <Fragment />
          )}
        </Fragment>
      ) : (
        <div className="dataview-empty">
          <Warning size={100} />
          <h2>No coordinates selected</h2>
          <span>Click on the map, to select a new location</span>
          <div className="dataview-empty-button">
            <div onClick={reloadData}>
              <LoadDataButton
                disabled={currentMapCache.selectedCoordinates ? false : true}
                ifNeedsReloading={ifNeedsReloading}
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default DataView;
