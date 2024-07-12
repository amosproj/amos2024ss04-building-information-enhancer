import React, { Fragment, useContext, useState } from "react";
import { Paper, Popover, Grid, Typography, Box, Tooltip } from "@mui/material";
import "./MapOptions.css";
import { Polygon, StackSimple, ThreeD } from "@phosphor-icons/react";
import SearchBar from "../SearchBar/SearchBar";
import { MapContext } from "../../contexts/MapContext";
import { MapTypes } from "../../types/MapTypes";

interface MapOptionsProps {
  onMapTypeChange: (type: MapTypes) => void;
  if3D: boolean;
  toggle3D: () => void;
}

const MapOptions: React.FC<MapOptionsProps> = ({
  onMapTypeChange,
  if3D,
  toggle3D,
}) => {
  const [anchorEl, setAnchorEl] = useState<HTMLDivElement | null>(null);
  const { currentMapCache, setCurrentMapCache } = useContext(MapContext);

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleMapTypeChange = (type: MapTypes) => {
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
      <Tooltip title="Toggle 3D view" arrow placement="right">
        <div
          onClick={toggle3D}
          className="threed-map-icon-container leaflet-touch leaflet-bar leaflet-control leaflet-control-custom"
        >
          <ThreeD className="options-icons" />
        </div>
      </Tooltip>
      <Tooltip title="Select a polygon" arrow placement="right">
        <div
          onClick={() => {
            if (!if3D) {
              setCurrentMapCache({
                ...currentMapCache,
                isDrawing: !currentMapCache.isDrawing,
              });
            }
          }}
          className={`draw-polygon-icon-container ${
            if3D ? "draw-polygon-icon-disabled" : ""
          } leaflet-touch leaflet-bar leaflet-control leaflet-control-custom`}
        >
          <Polygon className="options-icons" />
        </div>
      </Tooltip>
      {if3D ? (
        <Fragment>
          <div className="zoom-in-icon-container leaflet-touch leaflet-bar leaflet-control leaflet-control-custom">
            +
          </div>
          <div className="zoom-out-icon-container leaflet-touch leaflet-bar leaflet-control leaflet-control-custom">
            -
          </div>
        </Fragment>
      ) : (
        <Fragment />
      )}
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
                    handleMapTypeChange(MapTypes.Normal);
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
                    handleMapTypeChange(MapTypes.Satellite);
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
                    handleMapTypeChange(MapTypes.Aerial);
                    handleClose();
                  }}
                />
                <Typography variant="body2" sx={{ marginTop: 0.5 }}>
                  dop40c
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
