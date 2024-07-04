import DataPanel from "./DataPanel";
import "./DataView.css";
import { Fragment, useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { Box, TextField } from "@mui/material";
import { Funnel, MapPin, MapPinLine } from "@phosphor-icons/react";
import { MapContext } from "../../contexts/MapContext";
import LoadDataButton from "./LoadDataButton";
import { LocationDataResponse } from "../../types/LocationDataTypes";
import { fetchLocationData } from "../../services/locationDataService";
import { LatLng } from "leaflet";
import L from "leaflet";

function DataView() {
  const { currentTabsCache } = useContext(TabsContext);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const [filterValue, setFilterValue] = useState("");
  const [ifNeedsReloading, setIfNeedsReloading] = useState(false);
  const [data, setData] = useState<LocationDataResponse | undefined>();

  const handleFilterChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilterValue(event.target.value);
  };

  /**
   * Returns the title of the currently selected tab
   * @returns current tab title
   */
  const getCurrentTabTitle = (): string => {
    const currentTabID = currentTabsCache.currentTabID.toString();
    const currentTab = currentTabsCache.openedTabs.find(
      (tab) => tab.id === currentTabID
    );

    return currentTab ? currentTab.dataset.displayName : "No map loaded";
  };

  /**
   * Check if the "Reload data" button should be visible.
   * Should be run on selecting different coordinates or changing the current map ID.
   */
  useEffect(() => {
    // Check if different coordinates were selected
    if (
      !ifNeedsReloading &&
      currentMapCache.selectedCoordinates !== null &&
      currentMapCache.loadedCoordinates !== currentMapCache.selectedCoordinates
    ) {
      setIfNeedsReloading(true);
      // Check if tab was switched
    } else if (
      !ifNeedsReloading &&
      currentMapCache.selectedCoordinates !== null &&
      currentMapCache.currentTabID !== currentTabsCache.currentTabID
    ) {
      setIfNeedsReloading(true);
    }
  }, [currentMapCache, ifNeedsReloading, currentTabsCache.currentTabID]);

  /**
   * Reloads the location data.
   */
  const reloadData = async () => {
    // Get all parameters
    setIfNeedsReloading(false);
    const currentID = currentTabsCache.currentTabID;
    const currentCoords = currentMapCache.selectedCoordinates;
    // Set the current map cache
    setCurrentMapCache({
      ...currentMapCache,
      loadedCoordinates: currentCoords,
      currentTabID: currentID,
    });
    // Prepare the location data
    if (currentCoords) {
      let coords: LatLng[] = [];
      if (currentCoords instanceof LatLng) {
        // If a single coordinates are selected
        coords = [currentCoords];
        console.log("single");
      } else if (currentCoords instanceof L.Polygon) {
        // If an area (array of coordinates) is selected
        coords = currentCoords.getLatLngs() as LatLng[];
        console.log("area");
      }
      const responseData = await fetchLocationData(currentID, coords);
      if (responseData) {
        setData(responseData);
      }
    } else {
      console.log("Currently selected coordinates are null.");
    }
  };

  return (
    <div className="dataview-container">
      {currentMapCache.loadedCoordinates ? (
        <Fragment>
          <div className="dataview-header-container">
            <b className="dataview-header-title">
              <MapPin size={20} />
              {currentMapCache.loadedCoordinates instanceof LatLng && (
                <div>
                  Custom Marker{" "}
                  <span className="sub-text">
                    ({currentMapCache.loadedCoordinates.lat.toFixed(6)},{" "}
                    {currentMapCache.loadedCoordinates.lng.toFixed(6)})
                  </span>
                </div>
              )}
              {currentMapCache.loadedCoordinates instanceof L.Polygon && (
                <div>Custom Polygon </div>
              )}
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
            mapRows={data?.currentDatasetData ?? []}
            genericRows={data?.generalData ?? []}
            extraRows={data?.extraRows ?? []}
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
          <MapPinLine size={100} />
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
