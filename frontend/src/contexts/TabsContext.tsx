import React, { createContext, useState, ReactNode } from "react";

//// TYPES ////

// Tab Type
export type TabProps = {
  id: string;
  title: string;
  description: string;
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
};

// Provider component props type
type TabsContextProviderProps = {
  children: ReactNode;
};

//// CONTEXT ////

// Default Tabs Cache
const defaultTabsCache: TabsCacheProps = {
  currentTabID: "1",
  openedTabs: [
    {
      id: "1",
      title: "Charging Stations",
      description:
        "Here is the short description of the Charging Stations Map.",
      ifPinned: false,
    },
  ],
};

// Actual value of the context
export const TabsContext = createContext<TabsContextValue>({
  currentTabsCache: defaultTabsCache,
  setCurrentTabsCache: () => null,
});

// Provider component
export const TabsContextProvider: React.FC<TabsContextProviderProps> = ({
  children,
}) => {
  const [currentTabsCache, setCurrentTabsCache] =
    useState<TabsCacheProps>(defaultTabsCache);

  const value = {
    currentTabsCache,
    setCurrentTabsCache,
  };

  return <TabsContext.Provider value={value}>{children}</TabsContext.Provider>;
};
