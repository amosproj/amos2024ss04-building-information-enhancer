import DataPanel from "./DataPanel";
import "./DataView.css";

import Button from "@mui/material/Button";
import TabPanel from "@mui/lab/TabPanel/TabPanel";
import TabContext from "@mui/lab/TabContext/TabContext";
import Tab from "@mui/material/Tab/Tab";
import TabList from "@mui/lab/TabList/TabList";
import { CaretDown } from "@phosphor-icons/react";
import PopUp from "../Popup/Popup";
import { useState } from "react";

interface OptionItem {
  id: string;
  displayValue: string;
}

function DataView() {
  const [openDialog, setOpenDialog] = useState(false);

  const handleOpenDialog = () => {
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
  };

  const [favorites, setFavorites] = useState<OptionItem[]>([
    { id: "1", displayValue: "Nuremberg" },
    { id: "2", displayValue: "Munich" },
  ]);
  const [options] = useState<OptionItem[]>([
    { id: "1", displayValue: "Nuremberg" },
    { id: "2", displayValue: "Munich" },
    {
      id: "3",
      displayValue: "Andreij Sacharow Platz 1, 90402 Nuremberg",
    },
    { id: "4", displayValue: "Main train station Nuremberg" },
    { id: "5", displayValue: "Walter-Meckauer-Street 20" },
    { id: "6", displayValue: "49°26'46.6\"N 11°04'33.7\"E" },
  ]);

  const onCurrentSearchChanged = () => {};

  return (
    <div className="dataview-container">
      <TabContext value="1">
        <div className="tab-list-container">
          <TabList
            variant="scrollable"
            scrollButtons="auto"
            onChange={() => {}}
            aria-label="lab API multimap tabs"
            selectionFollowsFocus
          >
            <Tab
              label={
                <div className="dataview-title-container">
                  <span>Nuremberg</span>
                  <div className="favourite-icon-container">
                    <CaretDown
                      weight="bold"
                      className="location-icon"
                      onClick={handleOpenDialog}
                    />
                    <PopUp
                      title="Locations"
                      favorites={favorites}
                      setFavorites={setFavorites}
                      onClose={handleCloseDialog}
                      openDialog={openDialog}
                      onCurrentSearchChanged={onCurrentSearchChanged}
                      options={options}
                      onItemSelected={(item) => {
                        handleCloseDialog();
                        setTimeout(() => {
                          alert(item.displayValue);
                        }, 400);
                      }}
                    ></PopUp>
                  </div>
                </div>
              }
              value="1"
            ></Tab>
          </TabList>
        </div>

        <TabPanel value="1" className="tab dataview-tab">
          <div className="datapanels-container">
            <div className="data-panels-container">
              <DataPanel listTitle="Map Name" filterPanelId={1} />
              <DataPanel listTitle="General Data" filterPanelId={2} />
              <DataPanel listTitle="Extra Capabilities" filterPanelId={3} />
            </div>
            <Button variant="outlined">Load data</Button>
          </div>
        </TabPanel>
      </TabContext>
    </div>
  );
}

export default DataView;
