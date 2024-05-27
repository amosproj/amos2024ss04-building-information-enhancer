import { Fragment } from "react/jsx-runtime";
import {
  Divider,
  List,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
} from "@mui/material";
import { ChargingStation, Icon, Blueprint } from "@phosphor-icons/react";
import { useContext } from "react";
import { TabProps, TabsContext } from "../../contexts/TabsContext";

import "./DatasetsList.css";
import { AlertContext } from "../../contexts/AlertContext";
import { FeatureCollection } from "geojson";
import L, { Icon as LIcon, DivIcon } from "leaflet";
import { createRoot } from "react-dom/client";
import { flushSync } from "react-dom";

// Dataset Type
export type Dataset = {
  id: string;
  displayName: string;
  description: string;
  type: string;
  datasetIcon: Icon;
  markerIcon: LIcon | DivIcon | undefined;
  data: FeatureCollection;
};

// Define an empty FeatureCollection
const emptyFeatureCollection: FeatureCollection = {
  type: "FeatureCollection",
  features: [],
};

// Utility function to render a React component to HTML string
const renderToHtml = (Component: React.FC) => {
  const div = document.createElement("div");
  const root = createRoot(div);
  flushSync(() => {
    root.render(<Component />);
  });
  return div.innerHTML;
};

const divIconChargingStation: DivIcon = L.divIcon({
  html: renderToHtml(() => <ChargingStation size={32} weight="duotone" />),
  className: "", // Optional: add a custom class name
  iconSize: [34, 34],
  iconAnchor: [17, 17], // Adjust the anchor point as needed
});

const datasetsData: Dataset[] = [
  {
    id: "charging_stations",
    displayName: "Charging stations",
    description: "Locations of all charging stations in Germany.",
    type: "markers",
    datasetIcon: ChargingStation,
    markerIcon: divIconChargingStation,
    data: emptyFeatureCollection,
  },
  {
    id: "house_footprints",
    displayName: "House Footprints",
    description: "Footprints for the hauses.",
    type: "areas",
    datasetIcon: Blueprint,
    markerIcon: undefined,
    data: emptyFeatureCollection,
  },
];

interface DatasetsListProps {
  closeDialog: () => void;
}

const DatasetsList: React.FC<DatasetsListProps> = ({ closeDialog }) => {
  const { currentTabsCache, setCurrentTabsCache } = useContext(TabsContext);
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  // Opens a new tab
  const openNewTab = (dataset: Dataset) => {
    if (
      currentTabsCache.openedTabs.some((tab) => tab.dataset.id === dataset.id)
    ) {
      setCurrentAlertCache({
        ...currentAlertCache,
        isAlertOpened: true,
        text: "This dataset was already added.",
      });
      return;
    }
    const newTabID = currentTabsCache.openedTabs.length + 1;
    const newTab: TabProps = {
      id: newTabID.toString(),
      dataset: dataset,
      ifPinned: false,
    };
    setCurrentTabsCache({
      ...currentTabsCache,
      openedTabs: [...currentTabsCache.openedTabs, newTab],
    });
    // Close the dialog if necessary
    closeDialog();
  };

  return (
    <div className="datasets-list-container">
      <Divider style={{ marginTop: "1rem" }} />
      <List sx={{ width: "100%", bgcolor: "background.paper", padding: 0 }}>
        {datasetsData.map((dataset) => {
          return (
            <Fragment key={dataset.id}>
              <ListItemButton
                key={dataset.id}
                onClick={() => {
                  openNewTab(dataset);
                }}
              >
                <ListItemAvatar>
                  <dataset.datasetIcon size={30} className="dataset-icon" />
                </ListItemAvatar>
                <ListItemText
                  primary={dataset.displayName}
                  secondary={dataset.description}
                />
              </ListItemButton>
              <Divider />
            </Fragment>
          );
        })}
      </List>
    </div>
  );
};

export default DatasetsList;
