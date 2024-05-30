import { useMap } from "react-leaflet";
import { Dataset } from "../DatasetsList/DatasetsList";
import { useCallback, useContext, useEffect } from "react";
import { FeatureCollection } from "geojson";
import { MapContext } from "../../contexts/MapContext";
import { TabsContext } from "../../contexts/TabsContext";
import useGeoData from "./DataFetch";
import L from "leaflet";
import { LatLngBounds } from "leaflet";

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

  useEffect(() => {
    if (!geoData) return;

    // todo: if type is marker do this, dont do this with areas
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
  }, [dataset.markerIcon, map, geoData]);

  return null;
};

export default MapDatasetVisualizer;
