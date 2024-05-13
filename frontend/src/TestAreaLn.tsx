import { Button } from "@mui/material";
import { useState } from "react";
import SearchWithFavoritesDlg from "./SearchWithFavoritesDlg";

const TestAreaLn: React.FC = ({}) => {
  const [openDialog, setOpenDialog] = useState(false);

  const handleOpenDialog = () => {
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
  };

  const [favorites, setFavorites] = useState<string[]>(["Hans", "Kunibert"]);
  const [options, setOptions] = useState<string[]>([
    "Hansiii",
    "Kunibertaaaa",
    "Alfred",
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
    </div>
  );
};

export default TestAreaLn;
