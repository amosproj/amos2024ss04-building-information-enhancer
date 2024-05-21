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
  Box,
  InputAdornment,
} from "@mui/material";
import StarIcon from "@mui/icons-material/Star";
import EditLocationAltIcon from "@mui/icons-material/EditLocationAlt";

interface FavorableItem {
  id: string; // Unique identifier for the item
  displayValue: string; // Display value shown in the list
}

interface SearchWithFavoritesDlgProps {
  openDialog: boolean;
  onClose: () => void;
  title: string;
  favorites: FavorableItem[];
  setFavorites: Dispatch<SetStateAction<FavorableItem[]>>;
  options: FavorableItem[];
  onCurrentSearchChanged: (currentSearch: string) => void;
  onItemSelected: (selection: FavorableItem) => void;
}

const SearchWithFavoritesDlg: React.FC<SearchWithFavoritesDlgProps> = ({
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
  const [searchMode, setSearchMode] = useState("single"); // 'single' or 'coordinates'
  const [location, setLocation] = useState("");
  const [latitude, setLatitude] = useState("");
  const [longitude, setLongitude] = useState("");
  const [latitudeError, setLatitudeError] = useState(false);
  const [longitudeError, setLongitudeError] = useState(false);

  const handleModeSwitch = () => {
    if (searchMode === "single") {
      setSearchMode("coordinates");
    } else {
      setSearchMode("single");
    }
  };

  const filterBySearchText = (item: FavorableItem) => {
    return item.displayValue.toLowerCase().includes(searchText.toLowerCase());
  };

  const filterByCoordinates = (item: FavorableItem) => {
    if (searchMode === "coordinates") {

      if (searchText === "" || searchText === " ") {
        return item.displayValue;
      }

      const [latitudeTerm, longitudeTerm] = searchText.split(' ');
      const regex = new RegExp(`^-?${latitudeTerm}[\\S\\d°\'\\\\\".]*\\s-?${longitudeTerm}[\\S\\d°\'\\\\\".]*$`);

      return item.displayValue.toLowerCase().match(regex);
    } else {
      return filterBySearchText(item);
    }
  };

  const filterBySearchtxtAndIfInFavorites = (item: FavorableItem) => {
    return (
      filterByCoordinates(item) && !favorites.some((fav) => fav.id === item.id)
    );
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    setSearchText(e.target.value);
    onCurrentSearchChanged(e.target.value);
  };

  const validateLongitude = (value: string) => {
    var lng = parseFloat(value);
    return isFinite(lng) && Math.abs(lng) <= 180;
  };

  const validateLatitude = (value: string) => {
    var lat = parseFloat(value);
    return isFinite(lat) && Math.abs(lat) <= 90;
  };

  const handleLatitudeChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const value = e.target.value;
    setLatitude(value);
    const isValid = validateLatitude(value);
    setLatitudeError(!isValid);
    const combinedValue = `${value} ${longitude}`;
    setSearchText(combinedValue);
    onCurrentSearchChanged(combinedValue);
  };

  const handleLongitudeChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const value = e.target.value;
    setLongitude(value);
    const isValid = validateLongitude(value);
    setLongitudeError(!isValid);
    const combinedValue = `${latitude} ${value}`;
    setSearchText(combinedValue);
    onCurrentSearchChanged(combinedValue);
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
        {searchMode === "single" ? (
          <TextField
            label="Search Location"
            value={location}
            onChange={(e) => {
              setLocation(e.target.value);
              handleChange(e); // Also update the searchText state
            }}
            fullWidth
            margin="dense"
            sx={{ py: 0.25 }}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton onClick={handleModeSwitch} edge="end">
                    <EditLocationAltIcon />
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
        ) : (
          <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <Box
              sx={{
                display: "flex",
                gap: 2,
                alignItems: "center",
                justifyContent: "space-between",
                padding: "8px",
              }}
            >
              <Box sx={{ display: "flex", gap: 2 }}>
                <TextField
                  label="Latitude"
                  value={latitude}
                  onChange={(e) => {
                    setLocation(e.target.value);
                    handleLatitudeChange(e); 
                  }}
                  fullWidth
                  error={latitudeError}
                  helperText={
                    latitudeError ? "Must be between -90 and 90." : ""
                  }
                />
                <TextField
                  label="Longitude"
                  value={longitude}
                  onChange={(e) => {
                    setLocation(e.target.value);
                    handleLongitudeChange(e); 
                  }}
                  fullWidth
                  error={longitudeError}
                  helperText={
                    latitudeError ? "Must be between -180 and 180." : ""
                  }
                />
              </Box>
              <IconButton onClick={handleModeSwitch} edge="end">
                <EditLocationAltIcon />
              </IconButton>
            </Box>
          </Box>
        )}

        <List dense={false}>
          {favorites.filter(filterByCoordinates).map((item, index) => (
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

export default SearchWithFavoritesDlg;
