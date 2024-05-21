import L, { LatLng } from "leaflet";
import React, { createContext, useState, ReactNode } from "react";

//// TYPES ////

// Map Selection Type
export type MapSelection = {
  id: string;
  coordinates: LatLng;
  displayName: string;
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
      id: "1",
      coordinates: L.latLng([49.5732, 11.0288]),
      displayName: "Nuremberg",
    },
    {
      id: "2",
      coordinates: L.latLng([49.5732, 11.0288]),
      displayName: "Munich",
    },
    {
      id: "3",
      coordinates: L.latLng([49.5732, 11.0288]),
      displayName: "Andreij Sacharow Platz 1, 90402 Nuremberg",
    },
    {
      id: "4",
      coordinates: L.latLng([49.5732, 11.0288]),
      displayName: "Main train station Nuremberg",
    },
    {
      id: "5",
      coordinates: L.latLng([49.5732, 11.0288]),
      displayName: "Walter-Meckauer-Street 20",
    },
    {
      id: "6",
      coordinates: L.latLng([49.5732, 11.0288]),
      displayName: "49°26'46.6\"N 11°04'33.7\"E",
    },
  ],
  favourites: [
    {
      id: "1",
      coordinates: L.latLng([49.5732, 11.0288]),
      displayName: "Nuremberg",
    },
    {
      id: "2",
      coordinates: L.latLng([49.5732, 11.0288]),
      displayName: "Munich",
    },
  ],
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
