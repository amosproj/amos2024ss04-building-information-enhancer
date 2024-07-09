import DataPanel from "./DataPanel";
import "./DataView.css";
import { Fragment, useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { Box, TextField, Tooltip } from "@mui/material";
import { Funnel, MapPin, MapPinLine } from "@phosphor-icons/react";
import { MapContext } from "../../contexts/MapContext";
import LoadDataButton from "./LoadDataButton";
import { LocationDataResponse } from "../../types/LocationDataTypes";
import { fetchLocationData } from "../../services/locationDataService";
import {
  MarkerSelection,
  PolygonSelection,
} from "../../types/MapSelectionTypes";
import { MultiPolygon, Position } from "geojson";
import { CircularProgress } from "@mui/material";

// Function to filter and return an array of outer polygons
function getOuterPolygons(multiPolygon: MultiPolygon): Position[][] {
  // Filter out the inner polygons (holes) and keep only the outer ones
  return multiPolygon.coordinates.map((polygon) => polygon[0]);
}

function DataView() {
  const [isLoading, setIsLoading] = useState(false);
  const { currentTabsCache, getCurrentTab } = useContext(TabsContext);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const [filterValue, setFilterValue] = useState("");
  const [ifNeedsReloading, setIfNeedsReloading] = useState(false);
  const [data, setData] = useState<LocationDataResponse | undefined>();

  const handleFilterChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilterValue(event.target.value);
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
    setIsLoading(true); // Set loading to true before starting the fetch request
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
      let coords: Position[][] = [];

      if (currentCoords instanceof MarkerSelection) {
        const singlePosition: Position = [
          currentCoords.marker.lng,
          currentCoords.marker.lat,
        ];
        coords = [[singlePosition]];
      } else if (currentCoords instanceof PolygonSelection) {
        // we have multipolygons which can have quite complicated inner structures.
        // we simplfiy fot the current api in a way that we ignore all inner "holes" or other parts and only take
        // the outer parts. so the independent general polygons.
        coords = getOuterPolygons(currentCoords.polygon);
      }

      // Send the location request
      const currentTab = getCurrentTab();
      if (currentTab) {
        const responseData = await fetchLocationData(
          currentTab.dataset.id,
          coords
        );
        if (responseData) {
          setData(responseData);
        }
      }
    } else {
      console.log("Currently selected coordinates are null.");
    }

    setIsLoading(false); // Set loading to false after the fetch request completes
  };

  return (
    <div className="dataview-container">
      {currentMapCache.loadedCoordinates ? (
        <Fragment>
          <div className="dataview-header-container">
            <b className="dataview-header-title">
              <MapPin size={20} />
              <div>
                <Tooltip
                  title={currentMapCache.loadedCoordinates.displayName}
                  arrow
                >
                  <span>
                    {currentMapCache.loadedCoordinates.displayName.substring(
                      0,
                      40
                    ) +
                      (currentMapCache.loadedCoordinates.displayName.length > 40
                        ? "... "
                        : "")}
                  </span>
                </Tooltip>
                {currentMapCache.loadedCoordinates instanceof
                  MarkerSelection && (
                  <div className="sub-text">
                    ({currentMapCache.loadedCoordinates.marker.lat.toFixed(6)},{" "}
                    {currentMapCache.loadedCoordinates.marker.lng.toFixed(6)})
                  </div>
                )}
              </div>
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
          {isLoading ? (
            <div className="spinner-container">
              <CircularProgress />
            </div>
          ) : (
            <DataPanel
              listTitle={"General data"}
              filterValue={filterValue}
              mapRows={data?.generalData ?? []}
              genericRows={data?.individualData ?? []}
            />
          )}
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
