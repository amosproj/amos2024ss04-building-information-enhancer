import React, { useContext, useEffect } from "react";
import { useMap } from "react-leaflet";
import { Box, Typography } from "@mui/material";
import { TabsContext } from "../../contexts/TabsContext";
import "./ZoomWarningLabel.css";
import { Warning } from "@phosphor-icons/react";

/**
 * Displays a zoom warning label if the user zoomed out too much to see the data.
 */
const ZoomWarningLabel: React.FC = () => {
  const map = useMap();
  const { getCurrentTab } = useContext(TabsContext);

  const currentTab = getCurrentTab();

  useEffect(() => {
    const onZoomEnd = () => {
      const currentZoom = map.getZoom();
      const labelElement = document.getElementById("center-label");
      if (currentTab && currentTab.dataset.metaData && labelElement) {
        if (currentZoom <= currentTab.dataset.metaData.minZoomLevel) {
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
  }, [currentTab, map]);

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
        display: "none",
      }}
    >
      <Typography variant="h6" color="textPrimary">
        <div className="warning-label">
          <Warning size={40} /> Zoom in to see the markers
        </div>
      </Typography>
    </Box>
  );
};

export default ZoomWarningLabel;
