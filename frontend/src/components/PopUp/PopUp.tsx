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

// This is the partent component for all PopUps.
// - ifOpenedDialog - a boolean storing if the popup is currently opened.
// - onClose - on close function handler
// - title - the title of the PopUp
// - children - all children JSX components
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
