import _default from "@emotion/styled";
import React, { Dispatch, SetStateAction } from "react";
import {
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Dialog,
  DialogActions,
  DialogTitle,
  DialogContent,
  Button,
} from "@mui/material";
import StarBorderIcon from "@mui/icons-material/StarBorder";

interface SearchWithFavoritesDlgProps {
  openDialog: boolean;
  onClose: () => void;
  title: string;
  favorites: string[];
  setFavorites: Dispatch<SetStateAction<string[]>>;
  options: string[];
  onCurrentSearchChanged: (currentSearch: string) => void;
}

const SearchWithFavoritesDlg: React.FC<SearchWithFavoritesDlgProps> = ({
  openDialog,
  onClose,
  title,
  favorites,
  setFavorites,
  options,
  onCurrentSearchChanged,
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
      open={openDialog}
      onClose={onClose}
    >
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <List dense={false}>
          {favorites.map((item, index) => (
            <ListItem
              key={index}
              secondaryAction={
                <IconButton edge="end" aria-label="unfavorite">
                  <StarBorderIcon />
                </IconButton>
              }
              disablePadding
            >
              <ListItemButton key={index}>
                <ListItemText primary={item} />
              </ListItemButton>
            </ListItem>
          ))}
          {options.map((item, index) => (
            <ListItem
              key={index}
              secondaryAction={
                <IconButton edge="end" aria-label="favorite">
                  <StarBorderIcon />
                </IconButton>
              }
              disablePadding
            >
              <ListItemButton key={index}>
                <ListItemText primary={item} />
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="primary">
          Close
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default SearchWithFavoritesDlg;
