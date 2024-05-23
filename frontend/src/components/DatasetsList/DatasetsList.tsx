import { Fragment } from "react/jsx-runtime";
import {
  Divider,
  List,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
} from "@mui/material";
import { ChargingStation, Icon, Blueprint } from "@phosphor-icons/react";
import { useContext, useState } from "react";
import { TabProps, TabsContext } from "../../contexts/TabsContext";

import "./DatasetsList.css";
import ErrorAlert from "../Alerts/ErrorAlert";

// Dataset Type
export type Dataset = {
  id: string;
  displayName: string;
  description: string;
  type: string;
  datasetIcon: Icon;
  markerIcon: Icon | undefined;
  data: JSON[];
};

const datasetsData: Dataset[] = [
  {
    id: "charging_stations",
    displayName: "Charging stations",
    description: "Locations of all charging stations in Germany.",
    type: "markers",
    datasetIcon: ChargingStation,
    markerIcon: ChargingStation,
    data: [],
  },
  {
    id: "house_footprints",
    displayName: "House Footprints",
    description: "Footprints for the hauses.",
    type: "areas",
    datasetIcon: Blueprint,
    markerIcon: undefined,
    data: [],
  },
];

const DatasetsList = () => {
  const { currentTabsCache, setCurrentTabsCache } = useContext(TabsContext);
  const [alertOpen, setAlertOpen] = useState(false);

  // Opens a new tab
  const openNewTab = (dataset: Dataset) => {
    if (
      currentTabsCache.openedTabs.some((tab) => tab.dataset.id === dataset.id)
    ) {
      setAlertOpen(true);
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
      <ErrorAlert
        alertOpen={alertOpen}
        setAlertOpen={setAlertOpen}
        text="This dataset was already added."
      />
    </div>
  );
};

export default DatasetsList;
