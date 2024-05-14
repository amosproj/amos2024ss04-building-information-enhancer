import { Button } from "@mui/material";
import { useState } from "react";
import SearchWithFavoritesDlg from "./SearchWithFavoritesDlg";
import MapView from "./MapView";

interface OptionItem {
  id: string;
  displayValue: string;
}

const TestAreaLn: React.FC = ({}) => {
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
    <div>
      <p>Test area ln</p>
      <Button variant="outlined" onClick={handleOpenDialog}>
        Open Dialog
      </Button>
      <SearchWithFavoritesDlg
        title="Search names"
        favorites={favorites}
        setFavorites={setFavorites}
        onClose={handleCloseDialog}
        openDialog={openDialog}
        onCurrentSearchChanged={onCurrentSearchChanged}
        options={options}
      ></SearchWithFavoritesDlg>
      <MapView />
    </div>
  );
};

export default TestAreaLn;
