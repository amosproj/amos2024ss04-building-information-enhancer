import L, { LatLng } from "leaflet";
import React, { createContext, useState, ReactNode } from "react";
import { GeoJSON } from "geojson";
export declare type PointTuple = [number, number];
export declare type LatLngBoundsLiteralBoundsTuple = [PointTuple, PointTuple];

export type LatLngTuple = [number, number, number?];
export type LatLngBoundsLiteral = [LatLngTuple, LatLngTuple];

//// TYPES ////

// Map Selection Type
export type MapSelection = {
  coordinates: LatLng;
  displayName: string;
  bounds: LatLngBoundsLiteral | null;
  area: boolean; // everything that is not a building is an area
  polygon: null | GeoJSON;
};

// Search Cache Type
export type SearchCacheProps = {
  searchHistory: MapSelection[];
  favourites: MapSelection[];
};

// Search Context Type
type SearchContextValue = {
  currentSearchCache: SearchCacheProps;
  setCurrentSearchCache: React.Dispatch<React.SetStateAction<SearchCacheProps>>;
};

// Provider component props type
type SearchContextProviderProps = {
  children: ReactNode;
};

//// CONTEXT ////

// Default Search Cache
const defaultSearchCache: SearchCacheProps = {
  searchHistory: [
    {
      coordinates: L.latLng([49.5732, 12.0288]),
      displayName: "Nuremberg",
      bounds: [
        [40, 50],
        [50, 60],
      ],
      area: false,
      polygon: null,
    },
  ],
  favourites: [],
};

// Actual value of the context
export const SearchContext = createContext<SearchContextValue>({
  currentSearchCache: defaultSearchCache,
  setCurrentSearchCache: () => null,
});

// Provider component
export const SearchContextProvider: React.FC<SearchContextProviderProps> = ({
  children,
}) => {
  const [currentSearchCache, setCurrentSearchCache] =
    useState<SearchCacheProps>(defaultSearchCache);

  const value = {
    currentSearchCache,
    setCurrentSearchCache,
  };

  return (
    <SearchContext.Provider value={value}>{children}</SearchContext.Provider>
  );
};
