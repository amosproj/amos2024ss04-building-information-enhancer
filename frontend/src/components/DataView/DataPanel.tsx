import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import Grid from "@mui/material/Grid";
import IconButton from "@mui/material/IconButton";
import { MapTrifold } from "@phosphor-icons/react";
import "./DataPanel.css";
import { Tooltip } from "@mui/material";
import { GridToolbar, GridToolbarProps } from "@mui/x-data-grid";

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
  { id: 13, key: "sun hours", value: "20.5", button: 1 },
  { id: 14, key: "height", value: "100", button: 1 },
  { id: 15, key: "distance to X", value: "200", button: 1 },
  { id: 16, key: "sun hours", value: "21.5" },
  { id: 17, key: "height", value: "300" },
  { id: 18, key: "distance to X", value: "400", button: 1 },
  { id: 19, key: "sun hours", value: "20.5", button: 0 },
  { id: 20, key: "height", value: "100", button: 1 },
  { id: 21, key: "distance to X", value: "200" },
  { id: 22, key: "sun hours", value: "21.5" },
  { id: 23, key: "height", value: "300" },
  { id: 24, key: "distance to X", value: "400" },
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
  return (
    <div className="datapanels-container">
      <div className="data-panel-container">
        <div className="data-panel-title">{listTitle}</div>
        <Grid container spacing={2} className="data-panel-grid">
          <Grid item style={{ width: "100%" }}>
            <DataGrid
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
                pagination: {
                  paginationModel: { page: 0, pageSize: 5 },
                },
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
              pageSizeOptions={[5, 10]}
              density="compact"
              disableRowSelectionOnClick
              autoHeight
            />
          </Grid>
        </Grid>
      </div>
      <div className="data-panel-container">
        <div className="data-panel-title">General Data</div>
        <Grid container spacing={2} className="data-panel-grid">
          <Grid item style={{ width: "100%" }}>
            <DataGrid
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
                pagination: {
                  paginationModel: { page: 0, pageSize: 5 },
                },
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
              pageSizeOptions={[5, 10]}
              density="compact"
              disableRowSelectionOnClick
              autoHeight
            />
          </Grid>
        </Grid>
      </div>
      <div className="data-panel-container">
        <div className="data-panel-title">Extra Capabilities</div>
        <Grid container spacing={2} className="data-panel-grid">
          <Grid item style={{ width: "100%" }}>
            <DataGrid
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
                pagination: {
                  paginationModel: { page: 0, pageSize: 5 },
                },
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
              pageSizeOptions={[5, 10]}
              density="compact"
              disableRowSelectionOnClick
              autoHeight
            />
          </Grid>
        </Grid>
      </div>
    </div>
  );
};

export default DataPanel;
