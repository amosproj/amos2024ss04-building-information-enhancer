import DataPanel from "./DataPanel";
import "./DataView.css";
import { Fragment, useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { Box, TextField } from "@mui/material";
import { Funnel, MapPin, MapPinLine } from "@phosphor-icons/react";
import { MapContext } from "../../contexts/MapContext";
import LoadDataButton from "./LoadDataButton";
import axios from "axios";
import { getAPIGatewayURL } from "../../utils";

const loadLocationData = async (
  latitude: number,
  longitude: number,
  datasetId: string
): Promise<LocationDataResponse | undefined> => {
  const requestBody = {
    datasetId: datasetId,
    location: [{ latitude, longitude }],
  };

  try {
    const response = await axios.put<LocationDataResponse>(
      getAPIGatewayURL() + "/api/loadLocationData",
      requestBody
    );
    return response.data;
  } catch (error) {
    console.error("Error loading location data", error);
    return undefined;
  }
};

interface LocationDataResponse {
  currentDatasetData: DatasetItem[];
  generalData: DatasetItem[];
  extraRows: DatasetItem[];
}

interface DatasetItem {
  key: string;
  value: string;
  mapId: string;
}

function DataView() {
  const { currentTabsCache } = useContext(TabsContext);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const [filterValue, setFilterValue] = useState("");
  const [ifNeedsReloading, setIfNeedsReloading] = useState(false);
  const [data, setData] = useState<LocationDataResponse | undefined>();

  const handleFilterChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilterValue(event.target.value);
  };

  const getCurrentTabTitle = (): string => {
    const currentTabID = currentTabsCache.currentTabID.toString();
    const currentTab = currentTabsCache.openedTabs.find(
      (tab) => tab.id === currentTabID
    );

    return currentTab ? currentTab.dataset.displayName : "No map loaded";
  };

  useEffect(() => {
    if (
      !ifNeedsReloading &&
      currentMapCache.selectedCoordinates !== null &&
      currentMapCache.loadedCoordinates !== currentMapCache.selectedCoordinates
    ) {
      setIfNeedsReloading(true);
    }
  }, [currentMapCache, ifNeedsReloading]);

  const reloadData = async () => {
    if (currentMapCache.selectedCoordinates) {
      const { lat, lng } = currentMapCache.selectedCoordinates;
      setIfNeedsReloading(false);
      setCurrentMapCache({
        ...currentMapCache,
        loadedCoordinates: currentMapCache.selectedCoordinates,
      });
      const responseData = await loadLocationData(
        lat,
        lng,
        getCurrentTabTitle()
      );
      setData(responseData);
    }
  };

  return (
    <div className="dataview-container">
      {currentMapCache.loadedCoordinates ? (
        <Fragment>
          <div className="dataview-header-container">
            <b className="dataview-header-title">
              <MapPin size={20} />
              {currentMapCache.loadedCoordinates.lat.toFixed(6)},{" "}
              {currentMapCache.loadedCoordinates.lng.toFixed(6)}
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
