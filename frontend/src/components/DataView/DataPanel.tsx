import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import Grid from "@mui/material/Grid";
import IconButton from "@mui/material/IconButton";
import { CaretDown, MapTrifold } from "@phosphor-icons/react";
import "./DataPanel.css";
import { Tooltip } from "@mui/material";
import { GridToolbar, GridToolbarProps } from "@mui/x-data-grid";
import { useState } from "react";

// Returns a button if the "button" value is set to 1
const renderDetailsButton = (params: GridRenderCellParams) => {
  const value = params.value;
  if (value === 1) {
    return (
      <strong>
        <Tooltip arrow title="Open as a map">
          <IconButton aria-label="open as map" size="small">
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
  { field: "key", headerName: "key", width: 150 },
  {
    field: "value",
    headerName: "value",
    type: "number",
    width: 150,
    getApplyQuickFilterFn: undefined,
  },
];

// Data
const rows = [
  { id: 1, key: "sun hours", value: "20.5", button: 0 },
  { id: 2, key: "height", value: "100", button: 1 },
  { id: 3, key: "distance to X", value: "200" },
  { id: 4, key: "sun hours", value: "21.5" },
  { id: 5, key: "height", value: "300", button: 1 },
  { id: 6, key: "distance to X", value: "400" },
  { id: 7, key: "sun hours", value: "20.5", button: 0 },
  { id: 8, key: "height", value: "100", button: 1 },
  { id: 9, key: "distance to X", value: "200", button: 1 },
  { id: 10, key: "sun hours", value: "21.5" },
  { id: 11, key: "height", value: "300" },
  { id: 12, key: "distance to X", value: "400" },
];

function MyCustomToolbar(props: GridToolbarProps) {
  return <GridToolbar {...props} />;
}

interface DataPanelProps {
  listTitle: string;
  filterValue: string;
}

/*
  This component displays a mui DataGrid.
  Depending on the value of the "button" column, a map icon with the hover "open as map" is shown
*/
const DataPanel: React.FC<DataPanelProps> = ({ listTitle, filterValue }) => {
  // Keep track of if tabs are hidden
  const [ifMapDataTabHidden, toggleMapDataHidden] = useState<boolean>(false);
  const [ifGeneralDataTabHidden, toggleGeneralDataHidden] =
    useState<boolean>(false);
  const [ifExtraDataTabHidden, toggleExtraDataHidden] =
    useState<boolean>(false);

  return (
    <div className="datapanels-container">
      <div className="data-panel-container">
        <div
          className="data-panel-title"
          onClick={() => {
            toggleMapDataHidden(!ifMapDataTabHidden);
          }}
        >
          <CaretDown
            className={`data-panel-toggle-icon ${
              ifMapDataTabHidden ? "data-panel-toggle-icon-hidden" : ""
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
            ifMapDataTabHidden ? "data-panel-grid-hidden" : ""
          }`}
        >
          <DataGrid
            hideFooter={true}
            disableColumnMenu
            columnHeaderHeight={0}
            rows={rows}
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
                { field: "key", operator: "contains", value: filterValue },
              ],
            }}
            density="compact"
            disableRowSelectionOnClick
          />
        </Grid>
      </div>
      <div className="data-panel-container">
        <div
          className="data-panel-title"
          onClick={() => {
            toggleGeneralDataHidden(!ifGeneralDataTabHidden);
          }}
        >
          <CaretDown
            className={`data-panel-toggle-icon ${
              ifGeneralDataTabHidden ? "data-panel-toggle-icon-hidden" : ""
            }`}
          />
          General Data
        </div>
        <Grid
          item
          style={{ width: "100%" }}
          container
          spacing={2}
          className={`data-panel-grid ${
            ifGeneralDataTabHidden ? "data-panel-grid-hidden" : ""
          }`}
        >
          <DataGrid
            hideFooter={true}
            disableColumnMenu
            columnHeaderHeight={0}
            rows={rows}
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
                { field: "key", operator: "contains", value: filterValue },
              ],
            }}
            density="compact"
            disableRowSelectionOnClick
          />
        </Grid>
      </div>
      <div className="data-panel-container">
        <div
          className="data-panel-title"
          onClick={() => {
            toggleExtraDataHidden(!ifExtraDataTabHidden);
          }}
        >
          <CaretDown
            className={`data-panel-toggle-icon ${
              ifExtraDataTabHidden ? "data-panel-toggle-icon-hidden" : ""
            }`}
          />
          Extra Capabilities
        </div>
        <Grid
          item
          style={{ width: "100%" }}
          container
          spacing={2}
          className={`data-panel-grid ${
            ifExtraDataTabHidden ? "data-panel-grid-hidden" : ""
          }`}
        >
          <DataGrid
            hideFooter={true}
            disableColumnMenu
            columnHeaderHeight={0}
            rows={rows}
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
                { field: "key", operator: "contains", value: filterValue },
              ],
            }}
            density="compact"
            disableRowSelectionOnClick
          />
        </Grid>
      </div>
    </div>
  );
};

export default DataPanel;
