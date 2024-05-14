import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import "./App.css";

import MultiMap from "./components/multimap/multimap";
import Button from "@mui/material/Button";
import DataBox from "./components/DataView/DataBox";
import Dataview from "./components/DataViewOld/dataview";
import { useState } from "react";

function App() {
  const [currentComp, setCurrentComp] = useState(1);

  return (
    <div>
      <div className="toggle-work">
        <div
          onClick={() => {
            setCurrentComp(1);
          }}
        >
          [1]
        </div>
        <div
          onClick={() => {
            setCurrentComp(2);
          }}
        >
          [2]
        </div>
        <div
          onClick={() => {
            setCurrentComp(3);
          }}
        >
          [3]
        </div>
      </div>
      {currentComp === 1 && (
        <div className="app-container">
          <div className="header">
            <div className="title">Building Information Enhancer</div>
          </div>
          <div className="content-container">
            <MultiMap></MultiMap>
            <div className="dataview-container">
              <Dataview />
              <Button variant="outlined">Load data</Button>
            </div>
          </div>
        </div>
      )}
      {currentComp === 2 && <DataBox />}
      {currentComp === 3 && <DataBox />}
    </div>
  );
}

export default App;
