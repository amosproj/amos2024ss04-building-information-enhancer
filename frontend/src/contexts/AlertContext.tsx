import React, { createContext, useState, ReactNode } from "react";

//// TYPES ////

// Alert Cache Type
export type AlertCacheProps = {
  isAlertOpened: boolean;
  text: string;
};

// Alert Context Type
type AlertContextValue = {
  currentAlertCache: AlertCacheProps;
  setCurrentAlertCache: React.Dispatch<React.SetStateAction<AlertCacheProps>>;
};

// Provider component props type
type AlertContextProviderProps = {
  children: ReactNode;
};

//// CONTEXT ////

// Default Tabs Cache
const defaultAlertCache: AlertCacheProps = {
  isAlertOpened: false,
  text: "",
};

// Actual value of the context
export const AlertContext = createContext<AlertContextValue>({
  currentAlertCache: defaultAlertCache,
  setCurrentAlertCache: () => null,
});

// Provider component
export const AlertContextProvider: React.FC<AlertContextProviderProps> = ({
  children,
}) => {
  const [currentAlertCache, setCurrentAlertCache] =
    useState<AlertCacheProps>(defaultAlertCache);

  const value = {
    currentAlertCache,
    setCurrentAlertCache,
  };

  return (
    <AlertContext.Provider value={value}>{children}</AlertContext.Provider>
  );
};
