import React, { Fragment } from "react";
import {
  Dialog,
  DialogActions,
  DialogTitle,
  DialogContent,
  Button,
} from "@mui/material";

import "./PopUp.css";

interface PopUpProps {
  ifOpenedDialog: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  titleIcon: JSX.Element | undefined;
}

/**
 * This is the partent component for all PopUps.
 * @param ifOpenedDialog - a boolean storing if the popup is currently opened.
 * @param onClose - on close function handler
 * @param title - the title of the PopUp
 * @param children - all children JSX components
 * @param titleIcon - the icon displayed in front of the title
 * @returns
 */
const PopUp: React.FC<PopUpProps> = ({
  ifOpenedDialog,
  onClose,
  title,
  children,
  titleIcon,
}) => {
  return (
    <Dialog
      sx={{
        "& .MuiDialog-container": {
          "& .MuiPaper-root": {
            width: "100%",
            maxWidth: "500px", // Set your width here
          },
        },
      }}
      open={ifOpenedDialog}
      onClose={onClose}
    >
      <DialogTitle className="popup-name" style={{ paddingBottom: 8 }}>
        {titleIcon ? (
          <span style={{ marginRight: 8 }}>{titleIcon}</span>
        ) : (
          <Fragment />
        )}
        {title}
      </DialogTitle>
      <DialogContent>{children}</DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="primary">
          Close
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default PopUp;
