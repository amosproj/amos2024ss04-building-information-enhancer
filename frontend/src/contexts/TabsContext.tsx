import React, { createContext, useState, ReactNode } from "react";
import { fetchMetadataForDataset } from "../services/metadataService";
import { Dataset, DatasetMetaData } from "../types/DatasetTypes";

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
});

// Provider component
export const TabsContextProvider: React.FC<TabsContextProviderProps> = ({
  children,
}) => {
  const [currentTabsCache, setCurrentTabsCache] =
    useState<TabsCacheProps>(defaultTabsCache);

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
      setCurrentTabsCache((prevTabsCache) => {
        const updatedTabs = prevTabsCache.openedTabs.map((tab) =>
          tab.dataset.id === datasetID
            ? { ...tab, dataset: { ...tab.dataset, metaData: metadata } }
            : tab
        );
        return { ...prevTabsCache, openedTabs: updatedTabs };
      });
      const updatedTab = currentTabsCache.openedTabs.find(
        (tab) => tab.dataset.id === datasetID
      );
      return updatedTab?.dataset.metaData;
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
      return fetchAndSetMetadata(datasetID);
    }
  };

  const value = {
    currentTabsCache,
    setCurrentTabsCache,
    getCurrentTab,
    getOrFetchMetadata,
  };

  return <TabsContext.Provider value={value}>{children}</TabsContext.Provider>;
};
