import { Typography } from "@mui/material";
import { Fragment } from "react/jsx-runtime";
import bciLogo from "/bci_logo.png";
import codeingLogo from "/codeing-logo.png";
import DatasetsList from "../DatasetsList/DatasetsList";

import "./MainMenu.css";

/**
 * The main menu of the website.
 */
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
          <span className="secondary-text menu-subtitle">
            Select one of the datasets below to load it on the map
          </span>
          <div className="menu-datasets-list-container">
            <DatasetsList closeDialog={() => {}} />
          </div>
        </div>
      </div>
      <span className="menu-credits">
        Created by:{" "}
        <img
          className="codeing-logo"
          src={codeingLogo}
          alt="Code.ing Logo"
        ></img>
      </span>
    </Fragment>
  );
};

export default MainMenu;
