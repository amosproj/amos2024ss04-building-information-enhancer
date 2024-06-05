import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import "./App.css";

import DataView from "./components/DataView/DataView";
import MultiMap from "./components/MultiMap/MultiMap";
import { Fragment, useContext } from "react";
import { TabsContext } from "./contexts/TabsContext";
import MainMenu from "./components/MainMenu/MainMenu";
import ErrorAlert from "./components/Alerts/ErrorAlert";

function App() {
  const { currentTabsCache } = useContext(TabsContext);

  return (
    <div className="app-container">
      {currentTabsCache.openedTabs.length === 0 ? (
        <MainMenu />
      ) : (
        <Fragment>
          <div className="content-container">
            <MultiMap />
            <DataView />
          </div>
        </Fragment>
      )}
      <ErrorAlert />
    </div>
  );
}

export default App;
