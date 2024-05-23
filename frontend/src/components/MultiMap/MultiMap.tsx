import { Fragment, useContext } from "react";
import "./MultiMap.css";
import Tab from "@mui/material/Tab";
import TabContext from "@mui/lab/TabContext";
import TabList from "@mui/lab/TabList";
import TabPanel from "@mui/lab/TabPanel";
import { X, PushPin, PushPinSlash, PushPinSimple } from "@phosphor-icons/react";
import Tooltip from "@mui/material/Tooltip";
import MapView from "../MapView/MapView";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import NewTabButton from "./NewTabButton";
import { AlertContext } from "../../contexts/AlertContext";

const MultiMap = () => {
  // Access the tabs context
  const { currentTabsCache, setCurrentTabsCache } = useContext(TabsContext);
  // Alert
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  // Handles the change of the current tab id
  const handleChange = (_event: React.SyntheticEvent, newTabID: string) => {
    setCurrentTabsCache({ ...currentTabsCache, currentTabID: newTabID });
  };

  // Closes a specific tab
  const closeTab = (id: string) => {
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

  // Toggle pinning of a specific tab
  const toggleTabPinned = (tabId: string) => {
    const updatedOpenedTabs = currentTabsCache.openedTabs.map((tab) =>
      tab.id === tabId ? { ...tab, ifPinned: !tab.ifPinned } : tab
    );

    setCurrentTabsCache({
      ...currentTabsCache,
      openedTabs: updatedOpenedTabs,
    });
  };

  return (
    <div className="multimap-container">
      <TabContext value={currentTabsCache.currentTabID}>
        <div className="tab-list-container">
          <TabList
            variant="scrollable"
            scrollButtons="auto"
            onChange={handleChange}
            aria-label="lab API multimap tabs"
            selectionFollowsFocus
          >
            {currentTabsCache.openedTabs.map((tab) => {
              return (
                <Tab
                  label={
                    <div className="tab-title-container">
                      <span>{tab.dataset.displayName}</span>
                      <div className="tab-icons-container">
                        {tab.ifPinned ? (
                          <PushPinSimple
                            weight="fill"
                            className="pinned-tab-icon"
                            onClick={() => {
                              closeTab(tab.id);
                            }}
                          />
                        ) : (
                          <Fragment />
                        )}

                        <Tooltip title="Close Tab" arrow>
                          <X
                            className="close-tab-icon"
                            onClick={() => {
                              closeTab(tab.id);
                            }}
                          />
                        </Tooltip>
                        <Tooltip
                          title={!tab.ifPinned ? "Pin Tab" : "Unpin Tab"}
                          arrow
                          onClick={() => {
                            toggleTabPinned(tab.id);
                          }}
                        >
                          <div className="pin-tab-container">
                            {!tab.ifPinned ? (
                              <PushPin className="pin-tab-icon" />
                            ) : (
                              <PushPinSlash
                                weight="fill"
                                className="unpin-tab-icon"
                              />
                            )}
                          </div>
                        </Tooltip>
                      </div>
                    </div>
                  }
                  value={tab.id.toString()}
                  key={tab.id}
                ></Tab>
              );
            })}
          </TabList>
          <NewTabButton />
        </div>
        {currentTabsCache.openedTabs.map((tab: TabProps) => {
          return (
            <TabPanel value={tab.id.toString()} className="tab" key={tab.id}>
              <div className="tab-context-container">
                <span className="tab-description-container">
                  {tab.dataset.description}
                </span>
                <MapView />
              </div>
            </TabPanel>
          );
        })}
      </TabContext>
    </div>
  );
};

export default MultiMap;
