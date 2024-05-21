import React, { useContext, useState } from "react";
import {
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Divider,
  TextField,
  Box,
  InputAdornment,
} from "@mui/material";
import StarIcon from "@mui/icons-material/Star";
import EditLocationAltIcon from "@mui/icons-material/EditLocationAlt";

import { MapSelection, SearchContext } from "../../contexts/SearchContext";
import PopUp from "./PopUp";

import { OpenStreetMapProvider } from "leaflet-geosearch";
import { LatLng } from "leaflet";

interface SearchPopUpProps {
  onToggleIfOpenedDialog: () => void;
  ifOpenedDialog: boolean;
}

interface GeoSearchResult {
  x: number;
  y: number;
  label: string;
}

const SearchPopUp: React.FC<SearchPopUpProps> = ({
  onToggleIfOpenedDialog,
  ifOpenedDialog,
}) => {
  const [searchText, setSearchText] = useState("");
  const [searchMode, setSearchMode] = useState("single"); // 'single' or 'coordinates'
  const [latitude, setLatitude] = useState("");
  const [longitude, setLongitude] = useState("");
  const [latitudeError, setLatitudeError] = useState(false);
  const [longitudeError, setLongitudeError] = useState(false);
  const [suggestions, setSuggestions] = useState<Array<GeoSearchResult>>([]);

  const handleSearchSuggestions = async (input: string) => {
    console.log(input);
    if (input === "") {
      setSuggestions([]);
      return;
    }
    const provider = new OpenStreetMapProvider();
    const results = await provider.search({ query: input });
    const transformedResults: GeoSearchResult[] = results.map((result) => ({
      x: result.x,
      y: result.y,
      label: result.label,
    }));
    setSuggestions(transformedResults);
  };

  const handleModeSwitch = () => {
    if (searchMode === "single") {
      setSuggestions([]);
      setSearchMode("coordinates");
    } else {
      setSearchMode("single");
    }
  };

  const { currentSearchCache, setCurrentSearchCache } =
    useContext(SearchContext);

  const filterBySearchText = (item: MapSelection) => {
    return item.displayName.toLowerCase().includes(searchText.toLowerCase());
  };

  const filterByCoordinates = (item: MapSelection) => {
    if (searchMode === "coordinates") {
      if (searchText === "" || searchText === " ") {
        return item.displayName;
      }

      const [latitudeTerm, longitudeTerm] = searchText.split(" ");
      const regex = new RegExp(
        `^-?${latitudeTerm}[\\S\\d°'\\\\".]*\\s-?${longitudeTerm}[\\S\\d°'\\\\".]*$`
      );

      return item.displayName.toLowerCase().match(regex);
    } else {
      return filterBySearchText(item);
    }
  };

  const filterBySearchtxtAndIfInFavorites = (item: MapSelection) => {
    return (
      filterByCoordinates(item) &&
      !currentSearchCache.favourites.some((fav) =>
        fav.coordinates.equals(item.coordinates)
      )
    );
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    setSearchText(e.target.value);
    handleSearchSuggestions(e.target.value);
  };

  const validateLongitude = (value: string) => {
    const lng = parseFloat(value);
    return isFinite(lng) && Math.abs(lng) <= 180;
  };

  const validateLatitude = (value: string) => {
    const lat = parseFloat(value);
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
      {searchMode === "single" ? (
        <TextField
          value={searchText}
          onChange={(e) => {
            handleChange(e);
          }}
          margin="dense"
          id="outlined-basic"
          label="Search"
          variant="outlined"
          fullWidth
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
                  handleLatitudeChange(e);
                }}
                fullWidth
                error={latitudeError}
                helperText={latitudeError ? "Must be between -90 and 90." : ""}
              />
              <TextField
                label="Longitude"
                value={longitude}
                onChange={(e) => {
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
        {suggestions.map((item, index) => (
          <ListItem
            key={index}
            disablePadding
            sx={{ fontSize: "0.2rem", backgroundColor: "#f5f5f5" }}
          >
            <ListItemButton
              key={index}
              onClick={() =>
                onItemSelected({
                  coordinates: new LatLng(item.x, item.y),
                  displayName: item.label,
                })
              }
            >
              <ListItemText
                primary={item.label}
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
