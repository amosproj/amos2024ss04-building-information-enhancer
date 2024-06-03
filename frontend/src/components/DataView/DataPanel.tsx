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
    width: 250,
    getApplyQuickFilterFn: undefined,
  },
];

// Data
const mapRows = [
  { id: 1, key: "Pollution Level", value: "1002 (Moore Scale)", button: 0 },
  { id: 2, key: "Resource Efficiency", value: "57%", button: 1 },
  { id: 3, key: "Socio-economic Evaluation", value: "Grade B" },
  { id: 4, key: "Carbon Footprint", value: "7.72 CO2 m^2 (per capita)" },
  { id: 5, key: "Ecosystem Integrity", value: "Grade C", button: 1 },
];

const genericRows = [
  { id: 1, key: "Native vegetation", value: "75%", button: 0 },
  { id: 2, key: "Municipal Waste Recycled", value: "85%", button: 1 },
  { id: 3, key: "Poverty Rate", value: "12%" },
  { id: 4, key: "Energy Consumption", value: "250 kWh per capita", button: 1 },
  { id: 5, key: "Green Space Coverage", value: "30%", button: 1 },
];

// Data
const extraRows = [
  { id: 1, key: "Biodiversity Index", value: "0.8 (Shannon Diversity)" },
  { id: 2, key: "Income Inequality", value: "0.45", button: 1 },
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
          />
        </Grid>
      </div>
    </div>
  );
};

export default DataPanel;
