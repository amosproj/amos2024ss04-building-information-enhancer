import { Menu, MenuItem } from "@mui/material";
import { Fragment, useContext, useEffect, useState } from "react";
import { AlertContext } from "../../contexts/AlertContext";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import { Info, PushPin, PushPinSlash, X } from "@phosphor-icons/react";
import "./TabOptions.css";
import TabInfoPopUp from "../PopUp/TabInfoPopup";

interface TabOptionsProps {
  anchorElementTabOptions: null | HTMLElement;
  handleClose: () => void;
  currentTab: TabProps | undefined;
}

const TabOptions: React.FC<TabOptionsProps> = ({
  anchorElementTabOptions,
  handleClose,
  currentTab,
}) => {
  // Tab Info Popup
  // Stores the state of if the search popup is open
  const [ifOpenedDialog, setIfOpenedDialog] = useState(false);
  const { getOrFetchMetadata } = useContext(TabsContext);

  /**
   * Fetch the metadata of the tab
   */
  useEffect(() => {
    if (currentTab) {
      getOrFetchMetadata(currentTab.dataset.id);
    }
  }, [currentTab, getOrFetchMetadata]);

  const toggleIfOpenedDialog = () => {
    if (ifOpenedDialog === true) {
      // Handle the closing of the menu
      handleClose();
    }
    setIfOpenedDialog(!ifOpenedDialog);
  };

  // Access the tabs context
  const { currentTabsCache, setCurrentTabsCache } = useContext(TabsContext);
  // Alert
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  // Toggle pinning of a specific tab
  const toggleTabPinned = (tabId: string) => {
    // Toggle the pin
    const updatedOpenedTabs = currentTabsCache.openedTabs.map((tab) =>
      tab.id === tabId ? { ...tab, ifPinned: !tab.ifPinned } : tab
    );
    setCurrentTabsCache({
      ...currentTabsCache,
      openedTabs: updatedOpenedTabs,
    });
    // Handle close of the menu
    handleClose();
  };

  // Closes a specific tab
  const closeTab = (id: string) => {
    // Handle the close of the menu
    handleClose();
    // Check if the tab is pinned
    if (
      currentTabsCache.openedTabs.find((tab) => tab.id === id)?.ifPinned ===
      true
    ) {
      setCurrentAlertCache({
        ...currentAlertCache,
        isAlertOpened: true,
        text: "A pinned tab cannot be closed.",
      });
      return;
    }
    // Close the tab
    const newOpenedTabs = currentTabsCache.openedTabs.filter(
      (tab) => tab.id !== id
    );
    // Reorder the tabs IDs
    let tempID = 0;
    const updatedOpenedTabs = newOpenedTabs.map((tab) => {
      tempID = tempID + 1;
      return {
        ...tab,
        id: tempID.toString(),
      };
    });
    // Check if the current tab ID does not have to change
    let newCurrentTabID =
      Number(currentTabsCache.currentTabID) > updatedOpenedTabs.length - 1
        ? updatedOpenedTabs.length.toString()
        : currentTabsCache.currentTabID;
    // Check if it is the last tab
    if (currentTabsCache.openedTabs.length === 1) {
      newCurrentTabID = "1";
    }
    setCurrentTabsCache({
      ...currentTabsCache,
      openedTabs: updatedOpenedTabs,
      currentTabID: newCurrentTabID,
    });
  };

  return (
    <Fragment>
      {currentTab ? (
        <Fragment>
          <Menu
            id="basic-menu"
            anchorEl={anchorElementTabOptions}
            open={Boolean(anchorElementTabOptions)}
            onClose={handleClose}
            MenuListProps={{
              "aria-labelledby": "basic-button",
            }}
          >
            <MenuItem
              className="tab-options-item-container"
              onClick={() => {
                toggleIfOpenedDialog();
              }}
            >
              <Info size={18} />
              Info
            </MenuItem>
            <MenuItem
              className="tab-options-item-container"
              onClick={() => {
                toggleTabPinned(currentTab.id);
              }}
            >
              {currentTab.ifPinned ? (
                <Fragment>
                  <PushPinSlash size={18} weight="fill" />
                  Unpin Tab
                </Fragment>
              ) : (
                <Fragment>
                  <PushPin size={18} /> Pin Tab
                </Fragment>
              )}
            </MenuItem>
            <MenuItem
              className="tab-options-item-container"
              onClick={() => {
                closeTab(currentTab.id);
              }}
            >
              <X size={18} />
              Close Tab
            </MenuItem>
          </Menu>
          <TabInfoPopUp
            onToggleIfOpenedDialog={toggleIfOpenedDialog}
            ifOpenedDialog={ifOpenedDialog}
            currentTab={currentTab}
          />
        </Fragment>
      ) : (
        <Fragment />
      )}
    </Fragment>
  );
};

export default TabOptions;
