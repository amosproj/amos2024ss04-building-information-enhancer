import { Button } from "@mui/material";
import { useState } from "react";
import SearchWithFavoritesDlg from "./SearchWithFavoritesDlg";
import MapView from "./MapView";

interface OptionItem {
  id: string;
  displayValue: string;
}

const TestAreaLn: React.FC = () => {
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
    { id: "7", displayValue: "48.8566 2.3522" },
  { id: "8", displayValue: "34.0522 -118.2437" },
  { id: "9", displayValue: "51.5074 -0.1278" },
  { id: "10", displayValue: "35.6895 139.6917" },
  { id: "11", displayValue: "55.7558 37.6173" },
  { id: "12", displayValue: "40.7128 -74.0060" },
  { id: "13", displayValue: "37.7749 -122.4194" },
  { id: "14", displayValue: "-33.8688 151.2093" },
  { id: "15", displayValue: "-23.5505 -46.6333" },
  { id: "16", displayValue: "800 900" },
  { id: "17", displayValue: "808 99" },
  { id: "18", displayValue: "80 9" },
  { id: "19", displayValue: "49°26'46.6N 11°04'33.7E" },
  { id: "20", displayValue: "4926'46.6N 1104'33.7E" },
  { id: "21", displayValue: "492646.6N 110433.7E" },
  { id: "22", displayValue: "492646.6 110433.7" },




  ]);

  const onCurrentSearchChanged = () => {};

  return (
    <div>
      <p>Test area ln</p>
      <Button variant="outlined" onClick={handleOpenDialog}>
        Open Dialog
      </Button>
      <SearchWithFavoritesDlg
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
      ></SearchWithFavoritesDlg>
      <MapView />
    </div>
  );
};

export default TestAreaLn;
