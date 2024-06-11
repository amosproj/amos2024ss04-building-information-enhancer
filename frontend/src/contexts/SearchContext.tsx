import L, { LatLng } from "leaflet";
//import { RawResult } from "leaflet-geosearch/dist/providers/openStreetMapProvider.js";
import React, { createContext, useState, ReactNode } from "react";
import { GeoJSON } from 'geojson';
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
      bounds: [[40,50],[50,60]],
      area: false,
      polygon: null,
    },
    // {
    //   coordinates: L.latLng([49.5732, 13.0288]),
    //   displayName: "Munich",
    // },
    // {
    //   coordinates: L.latLng([49.5732, 14.0288]),
    //   displayName: "Andreij Sacharow Platz 1, 90402 Nuremberg",
    // },
    // {
    //   coordinates: L.latLng([49.5732, 15.0288]),
    //   displayName: "Main train station Nuremberg",
    // },
    // {
    //   coordinates: L.latLng([49.5732, 16.0288]),
    //   displayName: "Walter-Meckauer-Street 20",
    // },
    // {
    //   coordinates: L.latLng([49.5732, 17.0288]),
    //   displayName: "49°26'46.6\"N 11°04'33.7\"E",
    // },
    // {
    //   coordinates: L.latLng([41.40338, 2.17403]),
    //   displayName: "41°24'12.2\"N 2°10'26.5\"E",
    // },
    // {
    //   coordinates: L.latLng([34.05223, -118.24368]),
    //   displayName: "34.05223, -118.24368",
    // },
    // {
    //   coordinates: L.latLng([48.858844, 2.294351]),
    //   displayName: "48.858844, 2.294351",
    // },
    // {
    //   coordinates: L.latLng([40.748817, -73.985428]),
    //   displayName: "40°44'55.7\"N 73°59'07.5\"W",
    // },
    // {
    //   coordinates: L.latLng([51.500729, -0.124625]),
    //   displayName: "51°30'02.6\"N 0°07'28.7\"W",
    // },
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
