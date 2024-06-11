import { Fragment } from "react/jsx-runtime";
import {
  Divider,
  List,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
} from "@mui/material";
import { Icon, MapPin, MapTrifold } from "@phosphor-icons/react";
import { useContext, useEffect, useState } from "react";
import { TabProps, TabsContext } from "../../contexts/TabsContext";

import "./DatasetsList.css";
import { AlertContext } from "../../contexts/AlertContext";
import { FeatureCollection } from "geojson";
import L, { Icon as LIcon, DivIcon, LatLngBounds } from "leaflet";
import { createRoot } from "react-dom/client";
import { flushSync } from "react-dom";
import { MarkersTypes } from "./MarkersTypes";
import axios from "axios";
import { DatasetBasicData, DatasetListResponse } from "./DatasetTypes";

// Dataset Type
export type Dataset = {
  id: string;
  displayName: string;
  description: string;
  type: MarkersTypes;
  datasetIcon: Icon;
  markerIcon: LIcon | DivIcon | undefined;
  data: FeatureCollection;
  lastDataRequestBounds: LatLngBounds;
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

const createDivIcon = (iconName: string) => {
  console.log(iconName);
  return L.divIcon({
    html: iconName,
    className: "", // Optional: add a custom class name
    iconSize: [34, 34],
    iconAnchor: [17, 17], // Adjust the anchor point as needed
  });
};

const divIcondefault: DivIcon = L.divIcon({
  html: renderToHtml(() => <MapPin size={32} weight="duotone" />),
  className: "", // Optional: add a custom class name
  iconSize: [34, 34],
  iconAnchor: [17, 17], // Adjust the anchor point as needed
});

interface DatasetsListProps {
  closeDialog: () => void;
}

const DatasetsList: React.FC<DatasetsListProps> = ({ closeDialog }) => {
  const [datasets, setDatasets] = useState<Dataset[]>([]);
  const { currentTabsCache, setCurrentTabsCache } = useContext(TabsContext);
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  useEffect(() => {
    const fetchDatasets = async () => {
      try {
        // Make the API call with the expected response type
        const response = await axios.get<DatasetListResponse>(
          "https://localhost:49922/api/getDatasetList"
        );
        const datasetsData = response.data.basicInfoList.map(
          (dataset: DatasetBasicData) => {
            const data: Dataset = {
              id: dataset.datasetId,
              displayName: dataset.name,
              description: dataset.description,
              type: MarkersTypes.None, // This should be updated based on your logic
              datasetIcon: MapTrifold, // You should update this to map the correct icon
              markerIcon: dataset.icon
                ? createDivIcon(dataset.icon)
                : divIcondefault, // Update this as needed
              data: emptyFeatureCollection,
              lastDataRequestBounds: L.latLngBounds(
                L.latLng(0, 0),
                L.latLng(0, 0)
              ),
            };
            return data;
          }
        );
        setDatasets(datasetsData);
      } catch (error) {
        console.error("Error fetching datasets:", error);
      }
    };

    fetchDatasets();
  }, []);

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
        {datasets.map((dataset) => {
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
