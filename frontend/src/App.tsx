import "./App.css";
import Dataview from "./components/dataview/dataview";
import MultiMap from "./components/multimap/multimap";
import Button from "@mui/material/Button";

function App() {
  return (
    <div className="app-container">
      <div className="header">
        <div className="title">Building Information Enhancer</div>
      </div>
      <div className="content-container">
        <MultiMap></MultiMap>
        <div className="dataview-container">
          <Dataview></Dataview>
          <Button variant="outlined">Load data</Button>
        </div>
      </div>
    </div>
  );
}

export default App;
