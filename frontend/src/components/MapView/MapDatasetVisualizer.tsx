import { useMap } from "react-leaflet";
import { Dataset } from "../DatasetsList/DatasetsList";
import { useCallback, useContext, useEffect } from "react";
import { FeatureCollection } from "geojson";
import { MapContext } from "../../contexts/MapContext";
import { TabsContext } from "../../contexts/TabsContext";
import useGeoData from "./DataFetch";
import L from "leaflet";
import { LatLngBounds } from "leaflet";
import "proj4leaflet";
import "proj4";

interface MapDatasetVisualizerProps {
  dataset: Dataset;
}

const MapDatasetVisualizer: React.FC<MapDatasetVisualizerProps> = ({
  dataset,
}) => {
  const map = useMap();
  const { currentMapCache } = useContext(MapContext);
  const { setCurrentTabsCache } = useContext(TabsContext);

  const updateDatasetData = useCallback(
    (newData: FeatureCollection, bounds: LatLngBounds) => {
      setCurrentTabsCache((prevCache) => {
        const updatedTabs = prevCache.openedTabs.map((tab) => {
          if (tab.dataset.id === dataset.id) {
            return {
              ...tab,
              dataset: {
                ...tab.dataset,
                lastDataRequestBounds: bounds,
                data: newData,
              },
            };
          }
          return tab;
        });

        return {
          ...prevCache,
          openedTabs: updatedTabs,
        };
      });
    },
    [dataset.id, setCurrentTabsCache]
  );
  const geoData = useGeoData(
    dataset.id,
    currentMapCache.mapBounds,
    currentMapCache.zoom,
    updateDatasetData
  );
  const myCRS = new L.Proj.CRS(
    "EPSG:25832",
    "+proj=utm +zone=32 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs"
  );
  const coordsToLatLngWithCRS = (coords: number[]) => {
    const latLng = myCRS.unproject(L.point(coords[0], coords[1]));
    return latLng;
  };

  useEffect(() => {
    if (!geoData) return;

    if (dataset.id === "house_footprints") {
      console.log("hi from data visualizer");

      if (currentMapCache.zoom < 13) {
        const markerClusterGroup = L.markerClusterGroup();
        L.geoJson(geoData, {
          coordsToLatLng: coordsToLatLngWithCRS,
          onEachFeature: function (feature, featureLayer) {
            if (feature.geometry.type === "Polygon") {
              const bounds = (
                featureLayer as unknown as { getBounds: () => L.LatLngBounds }
              ).getBounds();
              const center = bounds.getCenter();
              const marker = L.marker(center);
              marker.addTo(markerClusterGroup);
            }
          },
        });

        //map.fitBounds(g.getBounds());
        map.addLayer(markerClusterGroup);
        return () => {
          map.removeLayer(markerClusterGroup);
        };
      } else {
        const geojsonLayer = L.geoJson(geoData, {
          coordsToLatLng: coordsToLatLngWithCRS,
        });
        geojsonLayer.addTo(map);

        return () => {
          map.removeLayer(geojsonLayer);
        };
      }
    } else {
      const geojsonLayer = L.geoJson(geoData, {
        pointToLayer: function (_feature, latlng) {
          return L.marker(latlng, { icon: dataset.markerIcon });
        },

        style: { fillOpacity: 0.1 },
      });
      const markerClusterGroup = L.markerClusterGroup();

      markerClusterGroup.addLayer(geojsonLayer);
      map.addLayer(markerClusterGroup);

      return () => {
        map.removeLayer(markerClusterGroup);
      };
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dataset, currentMapCache.zoom, map, geoData]);

  return null;
};

export default MapDatasetVisualizer;
