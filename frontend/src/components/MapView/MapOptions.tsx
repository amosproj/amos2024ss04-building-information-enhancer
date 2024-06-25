import React, { useState } from "react";
import {
  Paper,
  Popover,
  Grid,
  Typography,
  Box,
} from "@mui/material";
import SearchBar from "../PopUp/SearchBar";
import "./MapOptions.css";
import { StackSimple } from "@phosphor-icons/react";

interface MapOptionsProps {
  onMapTypeChange: (type: "normal" | "satellite" | "parzellar") => void;
}

const MapOptions: React.FC<MapOptionsProps> = ({ onMapTypeChange }) => {
  const [anchorEl, setAnchorEl] = useState<HTMLDivElement | null>(null);

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleMapTypeChange = (type: "normal" | "satellite" | "parzellar") => {
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
      <div onClick={handleClick} className="layers-map-icon-container leaflet-touch leaflet-bar leaflet-control leaflet-control-custom">
      <StackSimple aria-describedby={id} >
        
      </StackSimple>
      </div>
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
          <Grid container spacing={2} justifyContent="center" alignItems="center">
            <Grid item>
              <Box textAlign="center" sx={{ marginLeft: 2 }}>
                <img
                  className="image-hover-effect"
                  src="./normalview.png"
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
                  src="./satellite_view.png"
                  alt="Satellite"
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
                  src="./satellite_view.png"
                  alt="Satellite"
                  width="50"
                  height="50"
                  onClick={() => {
                    handleMapTypeChange("parzellar");
                    handleClose();
                  }}
                />
                <Typography variant="body2" sx={{ marginTop: 0.5 }}>
                  Parzellar
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
