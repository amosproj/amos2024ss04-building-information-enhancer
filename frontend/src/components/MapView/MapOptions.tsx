import { useState } from "react";
import { MagnifyingGlass } from "@phosphor-icons/react";
import "./MapOptions.css";
import { Tooltip } from "@mui/material";
import PopUp from "../Popup/Popup";

interface OptionItem {
  id: string;
  displayValue: string;
}

const MapOptions: React.FC = () => {
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
    <div className="map-options-container">
      <Tooltip title="Search Address">
        <MagnifyingGlass
          weight="duotone"
          className="search-map-icon"
          onClick={handleOpenDialog}
        />
      </Tooltip>
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
  );
};

export default MapOptions;
