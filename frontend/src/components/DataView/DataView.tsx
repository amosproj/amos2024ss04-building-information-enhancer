import DataPanel from "./DataPanel";
import "./DataView.css";

function DataView() {
  return (
    <div className="data-panels-container">
      <DataPanel listTitle="Map Name" filterPanelId={1} />
      <DataPanel listTitle="General Data" filterPanelId={2} />
      <DataPanel listTitle="Extra Capabilities" filterPanelId={3} />
    </div>
  );
}

export default DataView;
