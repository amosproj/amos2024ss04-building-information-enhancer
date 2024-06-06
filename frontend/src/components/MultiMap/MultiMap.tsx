import { Fragment, useContext, useState } from "react";
import "./MultiMap.css";
import Tab from "@mui/material/Tab";
import TabContext from "@mui/lab/TabContext";
import TabList from "@mui/lab/TabList";
import TabPanel from "@mui/lab/TabPanel";
import { DotsThreeOutline, PushPinSimple } from "@phosphor-icons/react";
import Tooltip from "@mui/material/Tooltip";
import MapView from "../MapView/MapView";
import { TabProps, TabsContext } from "../../contexts/TabsContext";
import NewTabButton from "./NewTabButton";
import TabOptions from "./TabOptions";

const MultiMap = () => {
  // Access the tabs context
  const { currentTabsCache, setCurrentTabsCache } = useContext(TabsContext);

  // Dropdown menu for the tab options
  const [anchorElementTabOptions, setAnchorElementTabOptions] =
    useState<null | HTMLElement>(null);

  const handleMenuClick = (
    event: React.MouseEvent<SVGSVGElement, MouseEvent>,   tabId: string
  ) => {
    event.stopPropagation();
    setAnchorElementTabOptions(event.currentTarget as unknown as HTMLElement);
    setSelectedTabId(tabId);
  };

  const handleMenuClose = () => {
    setAnchorElementTabOptions(null);
    setSelectedTabId(null);
  };

  const [selectedTabId, setSelectedTabId] = useState<string | null>(null);


  // Handles the change of the current tab id
  const handleChange = (event: React.SyntheticEvent, newTabID: string) => {

    if ((event.target as HTMLInputElement).type !== "button") {
      console.log("inside if");
      setCurrentTabsCache({ ...currentTabsCache, currentTabID: newTabID });
    }

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
              const isMenuOpen = anchorElementTabOptions && tab.id === selectedTabId;
              return (
                <Tab
                  label={
                    <div className="tab-title-container">
                      <div className="tab-title">
                        <tab.dataset.datasetIcon size={20} />
                        <span>{tab.dataset.displayName}</span>
                      </div>
                      <div className="tab-icons-container">
                        {tab.ifPinned && !isMenuOpen ? (
                          <PushPinSimple
                            weight="fill"
                            className="pinned-tab-icon"
                          />
                        ) : (
                          <Fragment />
                        )}
                        {isMenuOpen ? (
                          <DotsThreeOutline
                            weight="fill"
                            className="options-tab-icon-inverted"
                          />
                        ) : (
                          <Fragment />
                        )}
                        <Tooltip title="Tab Options" arrow>
                          <DotsThreeOutline
                            weight="fill"
                            className={`options-tab-icon ${isMenuOpen ? 'options-tab-icon-inverted' : ''}`}
                            onClick={(event) => {
                                handleMenuClick(event, tab.id);
                              }
                            }
                          />
                        </Tooltip>
                      </div>
                    </div>
                  }
                  value={tab.id.toString()}
                  key={tab.id}
                  disableRipple
                  disableFocusRipple
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
                <MapView datasetId={tab.dataset.id} />
              </div>
            </TabPanel>
          );
        })}
      </TabContext>
      <TabOptions
        anchorElementTabOptions={anchorElementTabOptions}
        handleClose={handleMenuClose}
        currentTab={currentTabsCache.openedTabs.find(
          (tab) => tab.id === selectedTabId
        )}
      />
    </div>
  );
};

export default MultiMap;
