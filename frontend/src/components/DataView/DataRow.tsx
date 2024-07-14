import { Fragment, useState, useEffect } from "react";
import { DatasetItem } from "../../types/LocationDataTypes";
import { useContext } from "react";
import {
  Collapse,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableRow,
  Tooltip,
} from "@mui/material";
import {
  CaretDown,
  CaretUp,
  MapPinSimpleArea,
  MapPin,
} from "@phosphor-icons/react";
import "./DataRow.css";
import { Dataset, DatasetBasicData } from "../../types/DatasetTypes";
import CustomSvgIcon from "../DatasetsList/CustomSvgIcon";
import { svgIconDefault } from "../DatasetsList/DatasetsList";
import { AlertContext } from "../../contexts/AlertContext";
import L, { LatLng } from "leaflet";
import { TabsContext } from "../../contexts/TabsContext";
import { MapContext } from "../../contexts/MapContext";
import { MarkerSelection } from "../../types/MapSelectionTypes";

interface RowProps {
  row: DatasetItem;
  currentDatasets: DatasetBasicData[];
}

/**
 * A single data row for the data view.
 */
const DataRow: React.FC<RowProps> = ({ row, currentDatasets }) => {
  const [open, setOpen] = useState(false);
  const [shouldFlyTo, setShouldFlyTo] = useState<LatLng | null>(null);
  const [shouldFlyToName, setShouldFlyToName] = useState<string | null>(null);
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);
  const { changeToOrOpenNewTab } = useContext(TabsContext);
  const { setCurrentMapCache, currentMapCache } = useContext(MapContext);

  /**
   * Triggers fly to on the next map change
   */
  useEffect(() => {
    if (shouldFlyTo && shouldFlyToName && currentMapCache.mapInstance) {
      currentMapCache.mapInstance.flyTo(shouldFlyTo, 16, {
        animate: true,
        duration: 5,
      });
      // Change the selection
      setCurrentMapCache({
        ...currentMapCache,
        selectedCoordinates: new MarkerSelection(
          shouldFlyTo,
          shouldFlyToName,
          false
        ),
      });
      setShouldFlyTo(null);
      setShouldFlyToName(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentMapCache.mapInstance]);

  const openDatasetFromMapIcon = async (
    mapId: string | null,
    coordinates: number[] | null,
    displayName: string
  ) => {
    if (currentDatasets) {
      const datasetToOpen = currentDatasets.find(
        (dataset) => dataset.datasetId === mapId
      );
      if (datasetToOpen) {
        const datasetToOpenTransformed: Dataset = {
          id: datasetToOpen.datasetId,
          displayName: datasetToOpen.name,
          shortDescription: datasetToOpen.shortDescription,
          datasetIcon: datasetToOpen.icon ? (
            <CustomSvgIcon svgString={datasetToOpen.icon} size={24} />
          ) : (
            <CustomSvgIcon svgString={svgIconDefault} size={24} />
          ),
          metaData: undefined,
          data: {
            type: "FeatureCollection",
            features: [],
          },
          lastDataRequestBounds: L.latLngBounds(L.latLng(0, 0), L.latLng(0, 0)),
        };
        // Open the map
        const ifSwitched = changeToOrOpenNewTab(datasetToOpenTransformed);
        // If provided fly to the coordinates
        if (coordinates && coordinates.length === 2) {
          const latLng = new LatLng(coordinates[0], coordinates[1]);
          if (ifSwitched) {
            setShouldFlyTo(latLng);
            setShouldFlyToName(displayName);
          } else {
            if (currentMapCache.mapInstance) {
              currentMapCache.mapInstance.flyTo(latLng, 16, {
                animate: true,
                duration: 5,
              });
              // Change the selection
              setCurrentMapCache({
                ...currentMapCache,
                selectedCoordinates: new MarkerSelection(
                  latLng,
                  displayName,
                  false
                ),
              });
            }
          }
        }
      } else {
        // Display alert
        setCurrentAlertCache({
          ...currentAlertCache,
          isAlertOpened: true,
          text: "Dataset with provided ID does not exist.",
        });
        console.error("Dataset with provided ID does not exist.");
      }
    }
  };

  /**
   * Returns an icon for a specific dataset
   * @param datasetID the dataset for the icon
   * @returns
   */
  const getDatasetIcon = (datasetID: string | null) => {
    if (datasetID) {
      const dataset = currentDatasets.find((ds) => ds.datasetId === datasetID);
      if (dataset && dataset.icon) {
        return <CustomSvgIcon svgString={dataset.icon} size={18} />;
      }
    }
    return <MapPinSimpleArea size={18} />;
  };

  return (
    <Fragment>
      <TableRow className="data-row">
        {row.subdata && row.subdata.length > 0 ? (
          <TableCell className="toggle-column" size="small">
            <IconButton
              aria-label="expand row"
              size="small"
              onClick={() => setOpen(!open)}
            >
              {open ? <CaretUp /> : <CaretDown />}
            </IconButton>
          </TableCell>
        ) : (
          <TableCell />
        )}
        <TableCell
          component="th"
          scope="row"
          size="small"
          className="data-row-title-container"
        >
          <div className="data-row-title-flex">
            <div className="data-row-title-icon">
              {getDatasetIcon(row.datasetId)}
            </div>
            <div>{row.displayName}</div>
          </div>
        </TableCell>
        {row.value && row.value !== "" ? (
          <TableCell
            component="th"
            scope="row"
            size="small"
            className="data-row-value"
          >
            {row.value}
          </TableCell>
        ) : (
          <TableCell />
        )}
        {row.datasetId &&
        row.datasetId !== "" &&
        row.coordinate &&
        row.coordinate.length === 2 ? (
          <TableCell className="toggle-column" size="small">
            <Tooltip title="Locate on the map" arrow placement="left">
              <IconButton
                aria-label="open on the map"
                size="small"
                onClick={() => {
                  openDatasetFromMapIcon(
                    row.datasetId,
                    row.coordinate,
                    row.displayName
                  );
                }}
              >
                <MapPin />
              </IconButton>
            </Tooltip>
          </TableCell>
        ) : (
          <TableCell />
        )}
      </TableRow>
      <TableRow>
        <TableCell style={{ paddingBottom: 0, paddingTop: 0 }} colSpan={6}>
          <Collapse in={open} timeout="auto" unmountOnExit>
            <Table size="small" aria-label="subdata">
              <TableBody className="subdata-rows-container">
                {row.subdata ? (
                  <Fragment>
                    {row.subdata.map((subItem) => (
                      <TableRow key={subItem.key}>
                        <TableCell className="subrow-filler" />
                        <TableCell
                          component="th"
                          scope="row"
                          size="small"
                          className="subrow-key"
                        >
                          {subItem.key}
                        </TableCell>
                        <TableCell size="small">
                          <div className="subrow-value">{subItem.value}</div>
                        </TableCell>
                        <TableCell className="subrow-filler" />
                      </TableRow>
                    ))}
                  </Fragment>
                ) : (
                  <div></div>
                )}
              </TableBody>
            </Table>
          </Collapse>
        </TableCell>
      </TableRow>
    </Fragment>
  );
};

export default DataRow;
