import _default from "@emotion/styled";
import React, { Dispatch, SetStateAction, useState } from "react";
import {
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Dialog,
  DialogActions,
  DialogTitle,
  DialogContent,
  Button,
  Divider,
  TextField,
} from "@mui/material";
import StarIcon from "@mui/icons-material/Star";

interface SearchWithFavoritesDlgProps {
  openDialog: boolean;
  onClose: () => void;
  title: string;
  favorites: string[];
  setFavorites: Dispatch<SetStateAction<string[]>>;
  options: string[];
  onCurrentSearchChanged: (currentSearch: string) => void;
}

const SearchWithFavoritesDlg: React.FC<SearchWithFavoritesDlgProps> = ({
  openDialog,
  onClose,
  title,
  favorites,
  setFavorites,
  options,
  onCurrentSearchChanged,
}) => {
  const [searchText, setSearchText] = useState("");

  const filterBySearchtxt = (txt: string) => {
    return txt.includes(searchText);
  };

  const filterBySearchtxtAndIfInFavorites = (txt: string) => {
    return txt.includes(searchText) && !favorites.includes(txt);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchText(e.target.value);
    onCurrentSearchChanged(e.target.value);
  };

  const addToFavorite = (txt: string) => {
    if (!favorites.includes(txt)) setFavorites([...favorites, txt]);
  };

  const removeFromFavorite = (itemToRemove: string) => {
    const updatedFavorites = favorites.filter((item) => item !== itemToRemove);
    setFavorites(updatedFavorites);
  };

  return (
    <Dialog
      sx={{
        "& .MuiDialog-container": {
          "& .MuiPaper-root": {
            width: "100%",
            maxWidth: "500px", // Set your width here
          },
        },
      }}
      open={openDialog}
      onClose={onClose}
    >
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <TextField
          value={searchText}
          onChange={handleChange}
          margin="dense"
          id="outlined-basic"
          label="Search"
          variant="outlined"
          fullWidth
        />
        <List dense={false}>
          {favorites.filter(filterBySearchtxt).map((item, index) => (
            <ListItem
              key={index}
              secondaryAction={
                <IconButton
                  edge="end"
                  aria-label="unfavorite"
                  onClick={() => removeFromFavorite(item)}
                >
                  <StarIcon style={{ fill: "yellow", stroke: "black" }} />
                </IconButton>
              }
              disablePadding
            >
              <ListItemButton key={index}>
                <ListItemText primary={item} />
              </ListItemButton>
            </ListItem>
          ))}
          <Divider />
          {options
            .filter(filterBySearchtxtAndIfInFavorites)
            .map((item, index) => (
              <ListItem
                key={index}
                secondaryAction={
                  <IconButton
                    edge="end"
                    aria-label="favorite"
                    onClick={() => addToFavorite(item)}
                  >
                    <StarIcon
                      style={{ fill: "transparent", stroke: "black" }}
                    />
                  </IconButton>
                }
                disablePadding
              >
                <ListItemButton key={index}>
                  <ListItemText primary={item} />
                </ListItemButton>
              </ListItem>
            ))}
        </List>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="primary">
          Close
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default SearchWithFavoritesDlg;
