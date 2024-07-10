import React, { createContext, useState, ReactNode, useContext } from "react";
import { fetchMetadataForDataset } from "../services/metadataService";
import { Dataset, DatasetMetaData } from "../types/DatasetTypes";
import { AlertContext } from "./AlertContext";

//// TYPES ////

// Tab Type
export type TabProps = {
  id: string;
  dataset: Dataset;
  ifPinned: boolean;
};

// Tabs Cache Type
export type TabsCacheProps = {
  currentTabID: string;
  openedTabs: TabProps[];
};

// Tabs Context Type
type TabsContextValue = {
  currentTabsCache: TabsCacheProps;
  setCurrentTabsCache: React.Dispatch<React.SetStateAction<TabsCacheProps>>;
  getCurrentTab: () => TabProps | undefined;
  getOrFetchMetadata: (
    datasetID: string
  ) => Promise<DatasetMetaData | undefined>;
  openNewTab: (datasetID: Dataset) => boolean;
};

// Provider component props type
type TabsContextProviderProps = {
  children: ReactNode;
};

//// CONTEXT ////

// Default Tabs Cache
const defaultTabsCache: TabsCacheProps = {
  currentTabID: "1",
  openedTabs: [],
};

// Actual value of the context
export const TabsContext = createContext<TabsContextValue>({
  currentTabsCache: defaultTabsCache,
  setCurrentTabsCache: () => null,
  getCurrentTab: () => undefined,
  getOrFetchMetadata: async () => undefined,
  openNewTab: () => false,
});

// Provider component
export const TabsContextProvider: React.FC<TabsContextProviderProps> = ({
  children,
}) => {
  const [currentTabsCache, setCurrentTabsCache] =
    useState<TabsCacheProps>(defaultTabsCache);
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  /**
   * Returns the currently opened tab
   * @returns current tab
   */
  const getCurrentTab = (): TabProps | undefined => {
    return currentTabsCache.openedTabs.find(
      (tab) => tab.id === currentTabsCache.currentTabID
    );
  };

  /**
   * Fetches and sets the metadata in the context. Should not be used explicitly (use getOrFetchMetadata() instead).
   * @param datasetID The dataset for which to fetch the metadata
   * @returns the metadata
   */
  const fetchAndSetMetadata = async (
    datasetID: string
  ): Promise<DatasetMetaData | undefined> => {
    // Try to fetch the metadata
    try {
      const metadata = await fetchMetadataForDataset(datasetID);
      // Update the state with the fetched metadata
      setCurrentTabsCache((prevTabsCache) => {
        const updatedTabs = prevTabsCache.openedTabs.map((tab) =>
          tab.dataset.id === datasetID
            ? { ...tab, dataset: { ...tab.dataset, metaData: metadata } }
            : tab
        );
        return { ...prevTabsCache, openedTabs: updatedTabs };
      });
      // Return the fetched metadata directly
      return metadata;
    } catch (error) {
      console.error("Failed to fetch metadata:", error);
      return undefined;
    }
  };

  /**
   * Returns the metadata for a dataset. If it is not present, fetches it.
   * @param datasetID the specified dataset
   * @returns the metadata
   */
  const getOrFetchMetadata = async (
    datasetID: string
  ): Promise<DatasetMetaData | undefined> => {
    // Find the tab related to the datasetID
    const tab = currentTabsCache.openedTabs.find(
      (tab) => tab.dataset.id === datasetID
    );
    // Return or fetch and return the metadata
    if (tab?.dataset.metaData) {
      return tab.dataset.metaData;
    } else {
      return await fetchAndSetMetadata(datasetID);
    }
  };

  /**
   * Opens a new tab
   * @param dataset a dataset id to open
   */
  const openNewTab = (dataset: Dataset) => {
    if (
      currentTabsCache.openedTabs.some((tab) => tab.dataset.id === dataset.id)
    ) {
      setCurrentAlertCache({
        ...currentAlertCache,
        isAlertOpened: true,
        text: "This dataset was already added.",
      });
      return false;
    }

    const newTabID = currentTabsCache.openedTabs.length + 1;
    const newTab: TabProps = {
      id: newTabID.toString(),
      dataset: dataset,
      ifPinned: false,
    };

    setCurrentTabsCache({
      ...currentTabsCache,
      currentTabID: newTab.id,
      openedTabs: [...currentTabsCache.openedTabs, newTab],
    });
    return true;
  };

  const value = {
    currentTabsCache,
    setCurrentTabsCache,
    getCurrentTab,
    getOrFetchMetadata,
    openNewTab,
  };

  return <TabsContext.Provider value={value}>{children}</TabsContext.Provider>;
};
