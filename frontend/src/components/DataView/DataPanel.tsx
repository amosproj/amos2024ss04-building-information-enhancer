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
  { field: "key", headerName: "key", width: 250 },
  {
    field: "value",
    headerName: "value",
    type: "number",
    width: 250,
    getApplyQuickFilterFn: undefined,
  },
];

function MyCustomToolbar(props: GridToolbarProps) {
  return <GridToolbar {...props} />;
}

interface DataPanelProps {
  listTitle: string;
  filterValue: string;
  mapRows: object[];
  genericRows: object[];
  extraRows: object[];
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
  extraRows,
}) => {
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
                { field: "key", operator: "contains", value: filterValue },
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
                { field: "key", operator: "contains", value: filterValue },
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
            rows={extraRows}
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
            autoHeight
          />
        </Grid>
      </div>
    </div>
  );
};

export default DataPanel;
