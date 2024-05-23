import { Typography } from "@mui/material";
import { Fragment } from "react/jsx-runtime";
import bciLogo from "/bci_logo.png";
import DatasetsList from "../DatasetsList/DatasetsList";

import "./MainMenu.css";

const MainMenu = () => {
  return (
    <Fragment>
      <div className="menu-container">
        <div className="menu-content-container">
          <div className="menu-title">
            <img src={bciLogo} alt="BCI Logo" className="menu-image" />
            <Typography variant="h4" gutterBottom>
              Building Information Enhancer
            </Typography>
          </div>
          <span className="secondary-text">
            Select one of the datasets below to load it on the map
          </span>
          <div className="menu-datasets-list-container">
            <DatasetsList />
          </div>
        </div>
      </div>
      <span className="menu-credits">Created by: Code.ing</span>
    </Fragment>
  );
};

export default MainMenu;
