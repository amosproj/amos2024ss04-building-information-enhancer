import {
  Feature,
  FeatureCollection,
  GeoJsonProperties,
  Point,
  Position,
} from "geojson";

// Function to convert polygon GeoJSON to marker GeoJSON
export const convertPolygonsToMarkers = (
  geoData: FeatureCollection
): FeatureCollection<Point, GeoJsonProperties> => {
  const markerFeatures: Feature<Point, GeoJsonProperties>[] =
    geoData.features.map((feature) => {
      if (feature.geometry.type === "Polygon") {
        const firstCoord = feature.geometry.coordinates[0][0];
        return {
          type: "Feature",
          geometry: {
            type: "Point",
            coordinates: firstCoord as Position,
          },
          properties: feature.properties,
        } as Feature<Point, GeoJsonProperties>;
      }
      return feature as Feature<Point, GeoJsonProperties>;
    });

  return {
    type: "FeatureCollection",
    features: markerFeatures,
  };
};
