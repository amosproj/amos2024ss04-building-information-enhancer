import React, { useContext, useState } from "react";
import { Paper, Popover, Grid, Typography, Box, Tooltip } from "@mui/material";
import "./MapOptions.css";
import { Polygon, StackSimple } from "@phosphor-icons/react";
import SearchBar from "../SearchBar/SearchBar";
import { MapContext } from "../../contexts/MapContext";

interface MapOptionsProps {
  onMapTypeChange: (type: "normal" | "satellite" | "parcel" | "aerial") => void;
}

const MapOptions: React.FC<MapOptionsProps> = ({ onMapTypeChange }) => {
  const [anchorEl, setAnchorEl] = useState<HTMLDivElement | null>(null);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleMapTypeChange = (
    type: "normal" | "satellite" | "parcel" | "aerial"
  ) => {
    onMapTypeChange(type);
    handleClose();
  };

  const open = Boolean(anchorEl);
  const id = open ? "simple-popover" : undefined;

  return (
    <div className="map-options-container">
      <div className="search-bar">
        <SearchBar />
      </div>
      <Tooltip title="Change map layers" arrow placement="right">
        <div
          onClick={handleClick}
          className="layers-map-icon-container leaflet-touch leaflet-bar leaflet-control leaflet-control-custom"
        >
          <StackSimple aria-describedby={id} className="options-icons" />
        </div>
      </Tooltip>
      <Tooltip title="Select a polygon" arrow placement="right">
        <div
          onClick={() => {
            setCurrentMapCache({
              ...currentMapCache,
              isDrawing: !currentMapCache.isDrawing,
            });
          }}
          className="draw-polygon-icon-container leaflet-touch leaflet-bar leaflet-control leaflet-control-custom"
        >
          <Polygon className="options-icons" />
        </div>
      </Tooltip>
      <Popover
        id={id}
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{
          vertical: "center",
          horizontal: "left",
        }}
        transformOrigin={{
          vertical: "center",
          horizontal: "right",
        }}
      >
        <Paper
          elevation={3}
          sx={{
            padding: "25px",
            borderRadius: "8px",
            backgroundColor: "#f9f9f9",
            width: "250px",
            height: "100px",
          }}
        >
          <Grid
            container
            spacing={2}
            justifyContent="center"
            alignItems="center"
          >
            <Grid item>
              <Box textAlign="center" sx={{ marginLeft: 2 }}>
                <img
                  className="image-hover-effect"
                  src="/normal_view.png"
                  alt="Normal"
                  width="50"
                  height="50"
                  onClick={() => {
                    handleMapTypeChange("normal");
                    handleClose();
                  }}
                />
                <Typography variant="body2" sx={{ marginTop: 0.5 }}>
                  Normal
                </Typography>
              </Box>
            </Grid>
            <Grid item>
              <Box textAlign="center" sx={{ marginLeft: 2 }}>
                <img
                  className="image-hover-effect"
                  src="/satellite_view.png"
                  alt="Satellite"
                  width="50"
                  height="50"
                  onClick={() => {
                    handleMapTypeChange("satellite");
                    handleClose();
                  }}
                />
                <Typography variant="body2" sx={{ marginTop: 0.5 }}>
                  Satellite
                </Typography>
              </Box>
            </Grid>
            <Grid item>
              <Box textAlign="center" sx={{ marginLeft: 2 }}>
                <img
                  className="image-hover-effect"
                  src="/aerial_view.png"
                  alt="Aerial"
                  width="50"
                  height="50"
                  onClick={() => {
                    handleMapTypeChange("aerial");
                    handleClose();
                  }}
                />
                <Typography variant="body2" sx={{ marginTop: 0.5 }}>
                  Aerial
                </Typography>
              </Box>
            </Grid>
          </Grid>
        </Paper>
      </Popover>
    </div>
  );
};

export default MapOptions;
