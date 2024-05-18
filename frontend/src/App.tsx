import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import "./App.css";

import Button from "@mui/material/Button";
import DataView from "./components/DataView/DataView";
import MultiMap from "./components/MultiMap/MultiMap";

function App() {
  return (
    <div className="app-container">
      <div className="header">
        <div className="title">Building Information Enhancer</div>
      </div>
      <div className="content-container">
        <MultiMap></MultiMap>
        <div className="dataview-container">
          <DataView />
          <Button variant="outlined">Load data</Button>
        </div>
      </div>
    </div>
  );
}

export default App;
