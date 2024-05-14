import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import "./App.css";

import MultiMap from "./components/multimap/multimap";
import Button from "@mui/material/Button";
import DataBox from "./components/DataView/DataBox";

function App() {
  return (
    <div className="app-container">
      <div className="header">
        <div className="title">Building Information Enhancer</div>
      </div>
      <div className="content-container">
        <MultiMap></MultiMap>
        <div className="dataview-container">
          <DataBox />
          <Button variant="outlined">Load data</Button>
        </div>
      </div>
    </div>
  );
}

export default App;
