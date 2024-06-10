import { Fragment } from "react/jsx-runtime";
import {
  Divider,
  List,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
} from "@mui/material";
import {
  ChargingStation,
  Icon,
  Blueprint,
  MapTrifold,
} from "@phosphor-icons/react";
import * as P from "@phosphor-icons/react";
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
import { renderToStaticMarkup } from "react-dom/server";

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

const fetchDatasets = async () => {
  try {
    const response = await axios.get<DatasetListResponse>(
      "https://localhost:5001/api/getDatasetList"
    );
    return response.data.basicInfoList;
  } catch (error) {
    console.error("Error fetching datasets:", error);
    return [];
  }
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

// Define a type for the keys of the P object
type PhosphorIconName = keyof typeof P;

const createDivIcon = (iconName: string) => {
  return L.divIcon({
    html: iconName,
    className: "", // Optional: add a custom class name
    iconSize: [34, 34],
    iconAnchor: [17, 17], // Adjust the anchor point as needed
  });
};

const divIconChargingStation: DivIcon = L.divIcon({
  html: renderToHtml(() => <ChargingStation size={32} weight="duotone" />),
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
          "https://localhost:5001/api/getDatasetList"
        );
        const test: DivIcon = createDivIcon(
          '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256"><rect width="256" height="256" fill="none"/><path d="M216,112v16c0,53-88,88-88,112,0-24-88-59-88-112V112" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="24"/><path d="M80,56h96a48,48,0,0,1,48,48v0a8,8,0,0,1-8,8H40a8,8,0,0,1-8-8v0A48,48,0,0,1,80,56Z" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="24"/><path d="M128,56V48a32,32,0,0,1,32-32" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="24"/></svg>'
        );
        const datasetsData = response.data.basicInfoList.map(
          (dataset: DatasetBasicData) => {
            const data: Dataset = {
              id: dataset.datasetId,
              displayName: dataset.name,
              description: dataset.description,
              type: MarkersTypes.None, // This should be updated based on your logic
              datasetIcon: MapTrifold, // You should update this to map the correct icon
              markerIcon: test, // Update this as needed
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
