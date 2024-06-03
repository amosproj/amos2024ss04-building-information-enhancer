import React, { useContext, useState, useEffect, useMemo } from "react";
import {
  Autocomplete,
  Box,
  TextField,
  IconButton,
  Grid,
} from "@mui/material";
import StarIcon from "@mui/icons-material/Star";
import { MagnifyingGlass } from "@phosphor-icons/react";
import { debounce } from "@mui/material/utils";
import parse from "autosuggest-highlight/parse";
import { MapSelection, SearchContext } from "../../contexts/SearchContext";
import { OpenStreetMapProvider } from "leaflet-geosearch";
import { LatLng } from "leaflet";
import { MapContext } from "../../contexts/MapContext";


const SearchBar: React.FC = () => {
  const [inputValue, setInputValue] = useState("");
  const [options, setOptions] = useState<Array<MapSelection>>([]);
  const { currentMapCache } = useContext(MapContext);
  const { currentSearchCache, setCurrentSearchCache } = useContext(SearchContext);

  const provider = new OpenStreetMapProvider({
    params: {
      "accept-language": "de",
      countrycodes: "de",
      addressdetails: 1,
    },
  });

  const fetch = useMemo(
    () =>
      debounce(async (query: string, callback: (results: MapSelection[]) => void) => {
        if (query === "") {
          callback([]);
          return;
        }
        const results = await provider.search({ query });
        const transformedResults: MapSelection[] = results.map((result) => ({
          coordinates: new LatLng(result.x, result.y),
          displayName: result.label,
        }));
        callback(transformedResults);
      }, 400),
    [provider]
  );

  useEffect(() => {
    let active = true;
    if (!active) return undefined;
    fetch(inputValue, (results) => {
      setOptions(results);
    });
    return () => {
      active = false;
    };
  }, [inputValue, fetch]);

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
    setTimeout(() => {
      alert(item.displayName);
      flyToLocation(new LatLng(item.coordinates.lat, item.coordinates.lng));
    }, 400);
  };

  const flyToLocation = (targetPosition: LatLng) => {
    const { mapInstance } = currentMapCache;
    if (mapInstance) {
      mapInstance.flyTo(targetPosition, 13, { animate: true, duration: 10 });
    } else console.log("no map instance");
  };

  return (
    <>
    <Autocomplete
      id="search-popup"
      sx={{ width: 400 }}
      getOptionLabel={(option) => option.displayName}
      filterOptions={(x) => x}
      options={[...currentSearchCache.favourites, ...options]}
      autoComplete
      includeInputInList
      filterSelectedOptions
      value={null}
      noOptionsText="No locations"
      onChange={(_event, newValue) => {
        if (newValue) {
          const selectedLocation = {
            coordinates: newValue.coordinates,
            displayName: newValue.displayName,
          };
          onItemSelected(selectedLocation);
        }
      }}
      onInputChange={(_event, newInputValue) => {
        setInputValue(newInputValue);
      }}
      renderInput={(params) => (


        <TextField {...params} 
        label={
            <div className="search-box-label">
              <MagnifyingGlass size={20} /> Search…
            </div>
          }
        size="small"
        //fullWidth 
        sx={{
            width: 150,
            '&:focus-within':{
              width:'100%',
            },
            transition: 'width 0s',
            backgroundColor:'white',
          }}
    />



      )}
      renderOption={(props, option) => {
        const matches = option.displayName.split(" ").map((word) => ({
          offset: option.displayName.indexOf(word),
          length: word.length,
        }));

        const parts = parse(
          option.displayName,
          matches.map((match) => [match.offset, match.offset + match.length])
        );

        const isFavorite = currentSearchCache.favourites.some(
          (fav) => fav.displayName === option.displayName
        );

        return (
          <li {...props}>
            <Grid container alignItems="center" flexWrap="nowrap" sx={{ height: "40px", transition:"none", width:'400'}}>
              <Grid item sx={{ flexGrow: 1, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                {parts.map((part, index) => (
                  <Box
                    key={index}
                    component="span"
                    sx={{ fontWeight: part.highlight ? "bold" : "regular", fontSize: "0.875rem" }}
                  >
                    {part.text}
                  </Box>
                ))}
              </Grid>
              <Grid item>
                <IconButton
                  edge="end"
                  aria-label={isFavorite ? "unfavorite" : "favorite"}
                  onClick={() =>
                    isFavorite
                      ? removeFromFavourites({
                          coordinates: option.coordinates,
                          displayName: option.displayName,
                        })
                      : addToFavourites({
                          coordinates: option.coordinates,
                          displayName: option.displayName,
                        })
                  }
                >
                  <StarIcon style={{ fill: isFavorite ? "yellow" : "transparent", stroke: "black" }} />
                </IconButton>
              </Grid>
            </Grid>
          </li>
        );
      }}
    />
    </>
  );
  };

export default SearchBar;
