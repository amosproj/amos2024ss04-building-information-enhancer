import { Fragment } from "react/jsx-runtime";
import {
  Divider,
  List,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
} from "@mui/material";
import { useContext, useEffect, useState } from "react";
import { TabProps, TabsContext } from "../../contexts/TabsContext";

import "./DatasetsList.css";
import { AlertContext } from "../../contexts/AlertContext";
import { FeatureCollection } from "geojson";
import L, { LatLngBounds } from "leaflet";
import { MarkersTypes } from "./MarkersTypes";
import axios from "axios";
import { DatasetBasicData, DatasetMetaData } from "./DatasetTypes";
import { getAPIGatewayURL } from "../../utils";
import CustomSvgIcon from "./CustomSvgIcon";

// Dataset Type
export type Dataset = {
  id: string;
  displayName: string;
  description: string;
  type: MarkersTypes;
  datasetIcon: JSX.Element;
  data: FeatureCollection;
  lastDataRequestBounds: LatLngBounds;
  metaData: DatasetMetaData | undefined;
};

// Define an empty FeatureCollection
const emptyFeatureCollection: FeatureCollection = {
  type: "FeatureCollection",
  features: [],
};

const svgIconDefault: string =
  '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M228.92,49.69a8,8,0,0,0-6.86-1.45L160.93,63.52,99.58,32.84a8,8,0,0,0-5.52-.6l-64,16A8,8,0,0,0,24,56V200a8,8,0,0,0,9.94,7.76l61.13-15.28,61.35,30.68A8.15,8.15,0,0,0,160,224a8,8,0,0,0,1.94-.24l64-16A8,8,0,0,0,232,200V56A8,8,0,0,0,228.92,49.69ZM104,52.94l48,24V203.06l-48-24ZM40,62.25l48-12v127.5l-48,12Zm176,131.5-48,12V78.25l48-12Z"></path></svg>';

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
        const response = await axios.get<DatasetBasicData[]>(
          getAPIGatewayURL() + "/api/getDatasetList"
        );
        const datasetsData = response.data.map((dataset: DatasetBasicData) => {
          const data: Dataset = {
            id: dataset.datasetId,
            displayName: dataset.name,
            description: dataset.description,
            type: MarkersTypes.None,
            datasetIcon: dataset.icon ? (
              <CustomSvgIcon svgString={dataset.icon} />
            ) : (
              <CustomSvgIcon svgString={svgIconDefault} />
            ),
            metaData: undefined,
            data: emptyFeatureCollection,
            lastDataRequestBounds: L.latLngBounds(
              L.latLng(0, 0),
              L.latLng(0, 0)
            ),
          };
          return data;
        });
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
                <ListItemAvatar>{dataset.datasetIcon}</ListItemAvatar>
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
