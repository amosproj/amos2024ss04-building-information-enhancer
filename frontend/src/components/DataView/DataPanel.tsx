import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import Grid from "@mui/material/Grid";
import IconButton from "@mui/material/IconButton";
import { CaretDown, MapTrifold } from "@phosphor-icons/react";
import "./DataPanel.css";
import { Tooltip } from "@mui/material";
import { GridToolbar, GridToolbarProps } from "@mui/x-data-grid";
import { useContext, useState } from "react";
import { DatasetItem } from "../../types/LocationDataTypes";
import { TabsContext } from "../../contexts/TabsContext";
import { fetchDatasets } from "../../services/datasetsService";
import { AlertContext } from "../../contexts/AlertContext";
import { Dataset } from "../../types/DatasetTypes";
import L from "leaflet";
import CustomSvgIcon from "../DatasetsList/CustomSvgIcon";
import { svgIconDefault } from "../DatasetsList/DatasetsList";
import { v4 } from "uuid";

function MyCustomToolbar(props: GridToolbarProps) {
  return <GridToolbar {...props} />;
}

interface DataPanelProps {
  listTitle: string;
  filterValue: string;
  mapRows: DatasetItem[];
  genericRows: DatasetItem[];
}

/*
  This component displays a mui DataGrid.
  Depending on the value of the "button" column, a map icon with the hover "open as map" is shown
*/
const DataPanel: React.FC<DataPanelProps> = ({
  listTitle,
  filterValue,
  mapRows,
  genericRows,
}) => {
  // Keep track of if tabs are hidden
  const [ifSelectionDataTabHidden, toggleSelectionDataHidden] =
    useState<boolean>(false);
  const [ifIndividualDataTabHidden, toggleIndividualDataHidden] =
    useState<boolean>(false);
  useState<boolean>(false);
  const { currentAlertCache, setCurrentAlertCache } = useContext(AlertContext);

  const { openNewTab } = useContext(TabsContext);

  const openDatasetFromMapIcon = async (mapId: string) => {
    const datasetsData = await fetchDatasets();
    if (datasetsData) {
      const datasetToOpen = datasetsData.find(
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

        openNewTab(datasetToOpenTransformed);
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

  // Returns a button if the "button" value is set to 1
  const renderDetailsButton = (params: GridRenderCellParams) => {
    const dataObject = params.row as DatasetItem;
    if (dataObject.datasetID !== "") {
      return (
        <strong>
          <Tooltip arrow title="Open a map ">
            <IconButton
              aria-label="open as map"
              size="small"
              onClick={() => {
                openDatasetFromMapIcon(dataObject.datasetID);
              }}
            >
              <MapTrifold />
            </IconButton>
          </Tooltip>
        </strong>
      );
    } else {
      return null;
    }
  };

  // Defines the columns of the Datagrid
  const columns: GridColDef[] = [
    {
      field: "button",
      headerName: "button",
      width: 60,
      renderCell: renderDetailsButton,
    },
    { field: "key", headerName: "key", width: 250 },
    {
      field: "value",
      headerName: "value",
      type: "number",
      width: 250,
      getApplyQuickFilterFn: undefined,
    },
  ];

  return (
    <div className="datapanels-container">
      <div className="data-panel-container">
        <div
          className="data-panel-title"
          onClick={() => {
            toggleSelectionDataHidden(!ifSelectionDataTabHidden);
          }}
        >
          <CaretDown
            className={`data-panel-toggle-icon ${
              ifSelectionDataTabHidden ? "data-panel-toggle-icon-hidden" : ""
            }`}
          />
          {listTitle}
        </div>
        <Grid
          container
          spacing={2}
          item
          style={{ width: "100%" }}
          className={`data-panel-grid ${
            ifSelectionDataTabHidden ? "data-panel-grid-hidden" : ""
          }`}
        >
          <DataGrid
            getRowId={() => {
              return v4();
            }}
            hideFooter={true}
            disableColumnMenu
            columnHeaderHeight={0}
            rows={mapRows}
            columns={columns}
            slots={{
              toolbar: MyCustomToolbar,
            }}
            slotProps={{
              toolbar: {
                printOptions: { disableToolbarButton: true },
                csvOptions: { disableToolbarButton: true },
              },
            }}
            disableDensitySelector
            disableColumnFilter
            disableColumnSelector
            disableColumnSorting
            initialState={{
              filter: {
                filterModel: {
                  items: [],
                  quickFilterValues: [filterValue],
                  quickFilterExcludeHiddenColumns: true,
                },
              },
            }}
            filterModel={{
              items: [
                {
                  field: "displayName",
                  operator: "contains",
                  value: filterValue,
                },
              ],
            }}
            density="compact"
            disableRowSelectionOnClick
            autoHeight
          />
        </Grid>
      </div>
      <div className="data-panel-container">
        <div
          className="data-panel-title"
          onClick={() => {
            toggleIndividualDataHidden(!ifIndividualDataTabHidden);
          }}
        >
          <CaretDown
            className={`data-panel-toggle-icon ${
              ifIndividualDataTabHidden ? "data-panel-toggle-icon-hidden" : ""
            }`}
          />
          Individual Data
        </div>
        <Grid
          item
          style={{ width: "100%" }}
          container
          spacing={2}
          className={`data-panel-grid ${
            ifIndividualDataTabHidden ? "data-panel-grid-hidden" : ""
          }`}
        >
          <DataGrid
            getRowId={() => {
              return v4();
            }}
            hideFooter={true}
            disableColumnMenu
            columnHeaderHeight={0}
            rows={genericRows}
            columns={columns}
            slots={{
              toolbar: MyCustomToolbar,
            }}
            slotProps={{
              toolbar: {
                printOptions: { disableToolbarButton: true },
                csvOptions: { disableToolbarButton: true },
              },
            }}
            disableDensitySelector
            disableColumnFilter
            disableColumnSelector
            disableColumnSorting
            initialState={{
              filter: {
                filterModel: {
                  items: [],
                  quickFilterValues: [filterValue],
                  quickFilterExcludeHiddenColumns: true,
                },
              },
            }}
            filterModel={{
              items: [
                {
                  field: "displayName",
                  operator: "contains",
                  value: filterValue,
                },
              ],
            }}
            density="compact"
            disableRowSelectionOnClick
            autoHeight
          />
        </Grid>
      </div>
    </div>
  );
};

export default DataPanel;
