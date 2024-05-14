/*
  This component is to be used from KeyValuesList only
*/

import { Portal } from "@mui/base/Portal";
import { GridToolbarQuickFilter, GridToolbar } from "@mui/x-data-grid";

function FilterBar({ filterPanelId }: { filterPanelId: number }) {
  const filterPanelIdString = `filter-panel-${filterPanelId}`;
  return (
    <>
      <Portal container={() => document.getElementById(filterPanelIdString)!}>
        <GridToolbarQuickFilter />
      </Portal>
      <GridToolbar
        printOptions={{ disableToolbarButton: true }}
        csvOptions={{ disableToolbarButton: true }}
      />
    </>
  );
}

export default FilterBar;
