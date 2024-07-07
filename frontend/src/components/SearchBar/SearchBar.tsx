import React, { useContext, useState, useEffect, useMemo } from "react";
import { Autocomplete, Box, TextField, IconButton, Grid } from "@mui/material";
import StarIcon from "@mui/icons-material/Star";
import { MagnifyingGlass } from "@phosphor-icons/react";
import { debounce } from "@mui/material/utils";
import parse from "autosuggest-highlight/parse";
import { MapSelection, SearchContext } from "../../contexts/SearchContext";
import { OpenStreetMapProvider } from "leaflet-geosearch";
import { LatLng } from "leaflet";
import { MapContext } from "../../contexts/MapContext";
import "./SearchBar.css";
import { GeoJSON, MultiPolygon } from "geojson";
import {
  MarkerSelection,
  PolygonSelection,
} from "../../types/MapSelectionTypes";
import L from "leaflet";

declare module "leaflet-geosearch/dist/providers/openStreetMapProvider.js" {
  interface RawResult {
    geojson: GeoJSON;
  }
}

const SearchBar: React.FC = () => {
  const [inputValue, setInputValue] = useState("");
  const [options, setOptions] = useState<Array<MapSelection>>([]);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);
  const { currentSearchCache, setCurrentSearchCache } =
    useContext(SearchContext);
  const [loading, setLoading] = useState(false);

  const fetch = useMemo(
    () =>
      debounce(
        async (query: string, callback: (results: MapSelection[]) => void) => {
          const provider = new OpenStreetMapProvider({
            params: {
              "accept-language": "de",
              countrycodes: "de",
              addressdetails: 1,
              polygon_geojson: 1,
            },
          });
          if (query === "") {
            callback([]);
            return;
          }
          const results = await provider.search({ query });
          const transformedResults: MapSelection[] = results.map((result) => ({
            coordinates: new LatLng(result.y, result.x),
            displayName: result.label,
            bounds: result.bounds,
            area: result.raw.class === "boundary",
            polygon: result.raw.geojson,
          }));
          callback(transformedResults);
        },
        400
      ),
    []
  );

  useEffect(() => {
    let active = true;
    if (!active) return undefined;
    setLoading(true);
    fetch(inputValue, (results) => {
      setOptions(results);
      setLoading(false);
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
      flyToLocation(item);
    }, 400);
  };

  const flyToLocation = (item: MapSelection) => {
    const targetPosition = new LatLng(
      item.coordinates.lat,
      item.coordinates.lng
    );

    const { mapInstance } = currentMapCache;

    if (mapInstance) {
      if (item.area && item.bounds) {
        mapInstance.flyToBounds(item.bounds, { animate: true, duration: 5 });
        if (currentMapCache.drawnItems) {
          currentMapCache.drawnItems.clearLayers();
        }
        if (item.polygon) {
          const drawPolygon = L.geoJSON(item.polygon, {
            style: {
              color: "#ff0000",
              weight: 2,
              fillOpacity: 0.06,
            },
          });
          drawPolygon.addTo(currentMapCache.drawnItems!);
          const polygonSelection = new PolygonSelection(
            item.polygon as MultiPolygon,
            item.displayName,
            false
          );
          setCurrentMapCache({
            ...currentMapCache,
            selectedCoordinates: polygonSelection,
          });
        }
      } else {
        // Select a marker on the map
        const markerSelection = new MarkerSelection(
          targetPosition,
          item.displayName,
          false
        );
        currentMapCache.selectedCoordinates = markerSelection;
        mapInstance.flyTo(targetPosition, currentMapCache.zoom, {
          animate: true,
          duration: 5,
        });
      }
    } else console.log("No map instance");
  };

  const getUniqueOptions = (options: MapSelection[]) => {
    const uniqueOptions = new Map<string, MapSelection>();
    options.forEach((option) => {
      uniqueOptions.set(option.displayName, option);
    });
    return Array.from(uniqueOptions.values());
  };

  const handleSearchIconClick = () => {
    if (options.length > 0) {
      onItemSelected(options[0]);
    } else {
      console.log("No suggestions available.");
    }
  };

  return (
    <>
      <Autocomplete
        id="search-popup"
        noOptionsText="No locations"
        sx={{ width: 400 }}
        getOptionLabel={(option) =>
          typeof option === "string" ? option : option.displayName
        }
        freeSolo={true}
        loading={loading}
        forcePopupIcon={false}
        filterOptions={(x) => x}
        options={getUniqueOptions([
          ...currentSearchCache.favourites,
          ...options,
        ])}
        autoComplete
        includeInputInList
        filterSelectedOptions
        onChange={(_event, newValue) => {
          if (typeof newValue === "string") {
            return;
          }
          if (newValue) {
            const selectedLocation = {
              coordinates: newValue.coordinates,
              displayName: newValue.displayName,
              bounds: newValue.bounds,
              area: newValue.area,
              polygon: newValue.polygon,
            };
            onItemSelected(selectedLocation);
          }
        }}
        onInputChange={(_event, newInputValue) => {
          setInputValue(newInputValue);
        }}
        renderInput={(params) => (
          <TextField
            {...params}
            placeholder="Search..."
            size="small"
            sx={{
              width: inputValue.length > 0 ? "100%" : 150,
              "&:focus-within": {
                width: "100%",
              },
              transition: "width 0s",
              backgroundColor: "white",
            }}
            InputProps={{
              ...params.InputProps,
              endAdornment: (
                <>
                  <IconButton
                    onClick={handleSearchIconClick}
                    className="search-icon"
                  >
                    <MagnifyingGlass size={20} />
                  </IconButton>
                  {params.InputProps.endAdornment}
                </>
              ),
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
            <li {...props} key={option.displayName}>
              <Grid
                container
                alignItems="center"
                flexWrap="nowrap"
                sx={{ height: "40px", transition: "none", width: "400" }}
              >
                <Grid
                  item
                  sx={{
                    flexGrow: 1,
                    overflow: "hidden",
                    textOverflow: "ellipsis",
                    whiteSpace: "nowrap",
                  }}
                >
                  {parts.map((part, index) => (
                    <Box
                      key={index}
                      component="span"
                      sx={{
                        fontWeight: part.highlight ? "bold" : "regular",
                        fontSize: "0.875rem",
                      }}
                    >
                      {part.text}
                    </Box>
                  ))}
                </Grid>
                <Grid item>
                  <IconButton
                    edge="end"
                    aria-label={isFavorite ? "unfavorite" : "favorite"}
                    onClick={(event: React.MouseEvent<HTMLButtonElement>) => {
                      event.stopPropagation();
                      isFavorite
                        ? removeFromFavourites({
                            coordinates: option.coordinates,
                            displayName: option.displayName,
                            bounds: option.bounds,
                            area: option.area,
                            polygon: option.polygon,
                          })
                        : addToFavourites({
                            coordinates: option.coordinates,
                            displayName: option.displayName,
                            bounds: option.bounds,
                            area: option.area,
                            polygon: option.polygon,
                          });
                    }}
                  >
                    <StarIcon
                      style={{
                        fill: isFavorite ? "yellow" : "transparent",
                        stroke: "black",
                      }}
                    />
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
