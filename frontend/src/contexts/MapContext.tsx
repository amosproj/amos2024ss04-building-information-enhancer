import L, { LatLng, LatLngBounds } from "leaflet";
import React, { createContext, useState, ReactNode } from "react";
import { MarkerSelection, PolygonSelection } from "../types/MapSelectionTypes";

//// TYPES ////

// Map Cache Type
export type MapCacheProps = {
  mapInstance: L.Map | null;
  selectedCoordinates: PolygonSelection | MarkerSelection | null;
  loadedCoordinates: PolygonSelection | MarkerSelection | null;
  currentTabID: null | string;
  mapCenter: LatLng;
  mapBounds: LatLngBounds;
  zoom: number;
  isDrawing: boolean;
  drawnItems: L.FeatureGroup | null;
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
  selectedCoordinates: null, // The coordinates selected by the use on the map
  loadedCoordinates: null, // The last coordinates loaded in the dataview panel
  currentTabID: null, // The currently loaded tab ID
  mapCenter: L.latLng([49.5732, 11.0288]),
  mapBounds: L.latLngBounds([49.5732, 11.0288], [49.5732, 11.0288]),
  zoom: 13,
  isDrawing: false,
  drawnItems: null,
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
