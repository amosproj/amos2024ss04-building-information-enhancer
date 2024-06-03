import DataPanel from "./DataPanel";
import "./DataView.css";
import { Fragment, useContext, useEffect, useState } from "react";
import { TabsContext } from "../../contexts/TabsContext";
import { Box, TextField } from "@mui/material";
import { Funnel, MapPin, Warning } from "@phosphor-icons/react";
import { MapContext } from "../../contexts/MapContext";
import LoadDataButton from "./LoadDataButton";

const randomNumber = (
  min: number,
  max: number,
  precision: number = 0
): number => {
  const factor = Math.pow(10, precision);
  const randomValue = Math.random() * (max - min) + min;
  return Math.round(randomValue * factor) / factor;
};

const randomGrade = (): string => {
  const grades = ["A", "B", "C", "D", "E", "F"];
  const randomIndex = Math.floor(Math.random() * grades.length);
  return grades[randomIndex];
};

const generateData = () => {
  return {
    mapRows: [
      {
        id: 1,
        key: "Pollution Level",
        value: `${randomNumber(950, 1050)} (Moore Scale)`,
        button: 0,
      },
      {
        id: 2,
        key: "Resource Efficiency",
        value: `${randomNumber(30, 100)}%`,
        button: 1,
      },
      {
        id: 3,
        key: "Socio-economic Evaluation",
        value: `Grade ${randomGrade()}`,
      },
      {
        id: 4,
        key: "Carbon Footprint",
        value: `${randomNumber(5, 10, 2)} CO2 m^2 (per capita)`,
      },
      {
        id: 5,
        key: "Ecosystem Integrity",
        value: `Grade ${randomGrade()}`,
        button: 1,
      },
    ],
    genericRows: [
      {
        id: 1,
        key: "Native vegetation",
        value: `${randomNumber(30, 100)}%`,
        button: 0,
      },
      {
        id: 2,
        key: "Municipal Waste Recycled",
        value: `${randomNumber(30, 100)}%`,
        button: 1,
      },
      { id: 3, key: "Poverty Rate", value: `${randomNumber(30, 100)}%` },
      {
        id: 4,
        key: "Energy Consumption",
        value: `${randomNumber(100, 300)} kWh per capita`,
        button: 1,
      },
      {
        id: 5,
        key: "Green Space Coverage",
        value: `${randomNumber(30, 100)}%`,
        button: 1,
      },
    ],
    extraRows: [
      {
        id: 1,
        key: "Biodiversity Index",
        value: `${randomNumber(0, 1, 1)} (Shannon Diversity)`,
      },
    ],
  };
};

function DataView() {
  const { currentTabsCache } = useContext(TabsContext);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const [filterValue, setFilterValue] = useState("");
  const [ifNeedsReloading, setIfNeedsReloading] = useState(false);
  const [data, setData] = useState(generateData());

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

  const reloadData = () => {
    setIfNeedsReloading(false);
    setCurrentMapCache({
      ...currentMapCache,
      loadedCoordinates: currentMapCache.selectedCoordinates,
    });
    setData(generateData());
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
            mapRows={data.mapRows}
            genericRows={data.genericRows}
            extraRows={data.extraRows}
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
