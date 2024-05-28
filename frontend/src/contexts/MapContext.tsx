import L, { LatLng, LatLngBounds } from "leaflet";
import React, { createContext, useState, ReactNode } from "react";

//// TYPES ////

// Map Cache Type
export type MapCacheProps = {
  mapInstance: L.Map | null;
  selectedCoordinates: LatLng;
  mapCenter: LatLng;
  mapBounds: LatLngBounds;
  zoom: number;
};

// Map Context Type
type MapContextValue = {
  currentMapCache: MapCacheProps;
  setCurrentMapCache: React.Dispatch<React.SetStateAction<MapCacheProps>>;
};

// Provider component props type
type MapContextProviderProps = {
  children: ReactNode;
};

//// CONTEXT ////

// Default Map Cache
const defaultMapCache: MapCacheProps = {
  mapInstance: null,
  selectedCoordinates: L.latLng([49.5732, 11.0288]),
  mapCenter: L.latLng([49.5732, 11.0288]),
  mapBounds: L.latLngBounds([49.5732, 11.0288], [49.5732, 11.0288]),
  zoom: 13,
};

// Actual value of the context
export const MapContext = createContext<MapContextValue>({
  currentMapCache: defaultMapCache,
  setCurrentMapCache: () => null,
});

// Provider component
export const MapContextProvider: React.FC<MapContextProviderProps> = ({
  children,
}) => {
  const [currentMapCache, setCurrentMapCache] =
    useState<MapCacheProps>(defaultMapCache);

  const value = {
    currentMapCache,
    setCurrentMapCache,
  };

  return <MapContext.Provider value={value}>{children}</MapContext.Provider>;
};