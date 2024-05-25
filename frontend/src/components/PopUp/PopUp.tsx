import React from "react";
import {
  Dialog,
  DialogActions,
  DialogTitle,
  DialogContent,
  Button,
} from "@mui/material";

interface PopUpProps {
  ifOpenedDialog: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
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
      <DialogTitle style={{ paddingBottom: 8 }}>{title}</DialogTitle>
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
