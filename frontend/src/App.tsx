import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import "./App.css";

import DataView from "./components/DataView/DataView";
import MultiMap from "./components/MultiMap/MultiMap";

function App() {
  return (
    <div className="app-container">
      <div className="header">Building Information Enhancer</div>
      <div className="content-container">
        <MultiMap />
        <DataView />
      </div>
    </div>
  );
}

export default App;
