import React, { useEffect } from "react";
import { useMap } from "react-leaflet";
import { Box, Typography } from "@mui/material";

interface CenterLabelProps {
  label: string;
  minZoom: number;
}

const ZoomWarningLabel: React.FC<CenterLabelProps> = ({ label, minZoom }) => {
  const map = useMap();

  useEffect(() => {
    const onZoomEnd = () => {
      const currentZoom = map.getZoom();
      const labelElement = document.getElementById("center-label");

      if (labelElement) {
        if (currentZoom <= minZoom) {
          labelElement.style.display = "block";
        } else {
          labelElement.style.display = "none";
        }
      }
    };

    map.on("zoomend", onZoomEnd);

    // Initial check
    onZoomEnd();

    return () => {
      map.off("zoomend", onZoomEnd);
    };
  }, [map, minZoom]);

  return (
    <Box
      id="center-label"
      sx={{
        position: "absolute",
        top: "50%",
        left: "50%",
        transform: "translate(-50%, -50%)",
        backgroundColor: "rgba(255, 255, 255, 0.8)",
        padding: "8px 16px",
        borderRadius: "8px",
        boxShadow: "0px 2px 4px rgba(0,0,0,0.1)",
        zIndex: 1000, // Ensure it is above the map
        pointerEvents: "none", // Make sure it doesn't interfere with map interactions
      }}
    >
      <Typography variant="h6" color="textPrimary">
        {label}
      </Typography>
    </Box>
  );
};

export default ZoomWarningLabel;
