import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import "./App.css";

import DataView from "./components/DataView/DataView";
import MultiMap from "./components/MultiMap/MultiMap";
import { Fragment, useContext, useState } from "react";
import { TabsContext } from "./contexts/TabsContext";
import MainMenu from "./components/MainMenu/MainMenu";
import ErrorAlert from "./components/Alerts/ErrorAlert";

import Tooltip from '@mui/material/Tooltip';
import { CaretLeft, CaretRight } from "@phosphor-icons/react";

import { MapContext } from "./contexts/MapContext";

function App() {

  const { currentMapCache } = useContext(MapContext);

  const { currentTabsCache } = useContext(TabsContext);

  const [dataViewVisible, setDataViewVisible] = useState(true);

  const toggleDataView = () => {
    setDataViewVisible((prevVisible) => !prevVisible);
    const { mapInstance } = currentMapCache;
    setTimeout(function(){ mapInstance?.invalidateSize()}, 400);
  };

  return (
    <div className="app-container">
      {currentTabsCache.openedTabs.length === 0 ? (
        <MainMenu />
      ) : (
        <Fragment>
          <div className="content-container">
          <div className={`multi-map ${!dataViewVisible ? "full-width" : ""}`}>
          <div className="multimap-container">
          
          <Tooltip title={dataViewVisible ? "Hide Data View" : "Show Data View"}>
                  <button onClick={toggleDataView} className="toggle-button">
                    {dataViewVisible ? (
                      <CaretRight/>
                    ) : (
                      <CaretLeft />
                    )}
                  </button>
                </Tooltip>
      
            <MultiMap />
            </div>
            </div>
            <div className={`data-view ${!dataViewVisible ? "hidden" : ""}`}>
            <DataView />
            </div>
          </div>
        </Fragment>
      )}
      <ErrorAlert />
    </div>
  );
}

export default App;
