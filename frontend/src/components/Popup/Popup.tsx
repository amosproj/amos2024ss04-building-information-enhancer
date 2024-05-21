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

interface FavorableItem {
  id: string; // Unique identifier for the item
  displayValue: string; // Display value shown in the list
}

interface PopUpProps {
  openDialog: boolean;
  onClose: () => void;
  title: string;
  favorites: FavorableItem[];
  setFavorites: Dispatch<SetStateAction<FavorableItem[]>>;
  options: FavorableItem[];
  onCurrentSearchChanged: (currentSearch: string) => void;
  onItemSelected: (selection: FavorableItem) => void;
}

const PopUp: React.FC<PopUpProps> = ({
  openDialog,
  onClose,
  title,
  favorites,
  setFavorites,
  options,
  onCurrentSearchChanged,
  onItemSelected,
}) => {
  const [searchText, setSearchText] = useState("");

  const filterBySearchText = (item: FavorableItem) => {
    return item.displayValue.toLowerCase().includes(searchText.toLowerCase());
  };

  const filterBySearchtxtAndIfInFavorites = (item: FavorableItem) => {
    return (
      item.displayValue.toLowerCase().includes(searchText.toLowerCase()) &&
      !favorites.some((fav) => fav.id === item.id)
    );
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchText(e.target.value);
    onCurrentSearchChanged(e.target.value);
  };

  const addToFavorite = (item: FavorableItem) => {
    if (!favorites.some((fav) => fav.id === item.id)) {
      setFavorites([...favorites, item]);
    }
  };

  const removeFromFavorite = (itemToRemove: FavorableItem) => {
    const updatedFavorites = favorites.filter(
      (item) => item.id !== itemToRemove.id
    );
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
          {favorites.filter(filterBySearchText).map((item, index) => (
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
              <ListItemButton key={index} onClick={() => onItemSelected(item)}>
                <ListItemText
                  primary={item.displayValue}
                  sx={{
                    "& .MuiDialog-container": {
                      "& .MuiPaper-root": {
                        width: "100%",
                        maxWidth: "500px", // Set your width here
                      },
                    },
                  }}
                />
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
                <ListItemButton
                  key={index}
                  onClick={() => onItemSelected(item)}
                >
                  <ListItemText
                    primary={item.displayValue}
                    sx={{
                      "& .MuiDialog-container": {
                        "& .MuiPaper-root": {
                          width: "100%",
                          maxWidth: "500px", // Set your width here
                        },
                      },
                    }}
                  />
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

export default PopUp;
