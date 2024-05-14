import "./App.css";
import Dataview from "./components/dataview/dataview";
import MultiMap from "./components/multimap/multimap";

function App() {
  return (
    <div className="app-container">
      <div className="header">
        <div className="title">Building Information Enhancer</div>
      </div>
      <div className="content-container">
        <MultiMap></MultiMap>
        <Dataview></Dataview>
      </div>
    </div>
  );
}

export default App;
