import React, { useContext, useState } from "react";
import {
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Divider,
  TextField,
} from "@mui/material";
import StarIcon from "@mui/icons-material/Star";
import { MapSelection, SearchContext } from "../../contexts/SearchContext";
import PopUp from "./PopUp";

interface SearchPopUpProps {
  onToggleIfOpenedDialog: () => void;
  ifOpenedDialog: boolean;
}

const SearchPopUp: React.FC<SearchPopUpProps> = ({
  onToggleIfOpenedDialog,
  ifOpenedDialog,
}) => {
  const [searchText, setSearchText] = useState("");
  const { currentSearchCache, setCurrentSearchCache } =
    useContext(SearchContext);

  const filterBySearchText = (item: MapSelection) => {
    return item.displayName.toLowerCase().includes(searchText.toLowerCase());
  };

  const filterBySearchtxtAndIfInFavorites = (item: MapSelection) => {
    return (
      item.displayName.toLowerCase().includes(searchText.toLowerCase()) &&
      !currentSearchCache.favourites.some((fav) =>
        fav.coordinates.equals(item.coordinates)
      )
    );
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchText(e.target.value);
  };

  // Adds an item to the favourites
  const addToFavourites = (newLocation: MapSelection) => {
    if (
      !currentSearchCache.favourites.some((fav) =>
        fav.coordinates.equals(newLocation.coordinates)
      )
    ) {
      const newFav = [...currentSearchCache.favourites, newLocation];
      setCurrentSearchCache({
        ...currentSearchCache,
        favourites: newFav,
      });
    }
  };

  // Removes an item from the favourites
  const removeFromFavourites = (locationToRemove: MapSelection) => {
    const updatedFavorites = currentSearchCache.favourites.filter(
      (item) => !item.coordinates.equals(locationToRemove.coordinates)
    );
    setCurrentSearchCache({
      ...currentSearchCache,
      favourites: updatedFavorites,
    });
  };

  const onItemSelected = (item: MapSelection) => {
    onToggleIfOpenedDialog();
    setTimeout(() => {
      alert(item.displayName);
    }, 400);
  };

  return (
    <PopUp
      title="Locations"
      onClose={onToggleIfOpenedDialog}
      ifOpenedDialog={ifOpenedDialog}
    >
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
        {currentSearchCache.favourites
          .filter(filterBySearchText)
          .map((item, index) => (
            <ListItem
              key={index}
              secondaryAction={
                <IconButton
                  edge="end"
                  aria-label="unfavorite"
                  onClick={() => removeFromFavourites(item)}
                >
                  <StarIcon style={{ fill: "yellow", stroke: "black" }} />
                </IconButton>
              }
              disablePadding
            >
              <ListItemButton key={index} onClick={() => onItemSelected(item)}>
                <ListItemText
                  primary={item.displayName}
                  sx={{
                    "& .MuiDialog-container": {
                      "& .MuiPaper-root": {
                        width: "100%",
                        maxWidth: "500px",
                      },
                    },
                  }}
                />
              </ListItemButton>
            </ListItem>
          ))}
        <Divider />
        {currentSearchCache.searchHistory
          .filter(filterBySearchtxtAndIfInFavorites)
          .map((item, index) => (
            <ListItem
              key={index}
              secondaryAction={
                <IconButton
                  edge="end"
                  aria-label="favorite"
                  onClick={() => addToFavourites(item)}
                >
                  <StarIcon style={{ fill: "transparent", stroke: "black" }} />
                </IconButton>
              }
              disablePadding
            >
              <ListItemButton key={index} onClick={() => onItemSelected(item)}>
                <ListItemText
                  primary={item.displayName}
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
    </PopUp>
  );
};

export default SearchPopUp;
