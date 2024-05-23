import Box from "@mui/material/Box";
import Alert from "@mui/material/Alert";
import IconButton from "@mui/material/IconButton";
import Collapse from "@mui/material/Collapse";
import CloseIcon from "@mui/icons-material/Close";

import "./Alerts.css";
import { useContext, useEffect } from "react";
import { AlertContext } from "../../contexts/AlertContext";

const ErrorAlert: React.FC = () => {
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  // Auto-hide
  useEffect(() => {
    const timeId = setTimeout(() => {
      // After 3 seconds set the show value to false
      setCurrentAlertCache({
        ...currentAlertCache,
        isAlertOpened: false,
      });
    }, 3000);

    return () => {
      clearTimeout(timeId);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentAlertCache]);

  return (
    <div className="alert-container">
      <Box sx={{ width: "100%" }} className="alert">
        <Collapse in={currentAlertCache.isAlertOpened}>
          <Alert
            severity="error"
            style={{ border: "1px solid #723a3a" }}
            action={
              <IconButton
                aria-label="close"
                color="inherit"
                size="small"
                onClick={() => {
                  setCurrentAlertCache({
                    ...currentAlertCache,
                    isAlertOpened: false,
                  });
                }}
              >
                <CloseIcon fontSize="inherit" />
              </IconButton>
            }
            sx={{ mb: 2 }}
          >
            {currentAlertCache.text}
          </Alert>
        </Collapse>
      </Box>
    </div>
  );
};

export default ErrorAlert;
