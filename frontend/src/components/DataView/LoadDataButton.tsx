import { Button } from "@mui/material";
import { ArrowsClockwise } from "@phosphor-icons/react";
import { MapContext } from "../../contexts/MapContext";
import { useContext } from "react";

interface LoadDataButtonProps {
  ifNeedsReloading: boolean;
  disabled: boolean;
}

/**
 * A reload button for fetching the newest location data.
 */
const LoadDataButton: React.FC<LoadDataButtonProps> = ({
  ifNeedsReloading,
  disabled,
}) => {
  const { currentMapCache } = useContext(MapContext);

  return (
    <Button
      disabled={disabled}
      variant="contained"
      color={
        ifNeedsReloading && currentMapCache.loadedCoordinates !== null
          ? "warning"
          : "info"
      }
    >
      {ifNeedsReloading && currentMapCache.loadedCoordinates !== null ? (
        <div className="load-data-reload">
          <ArrowsClockwise />
          Reload Data
        </div>
      ) : (
        "Load data"
      )}
    </Button>
  );
};

export default LoadDataButton;
