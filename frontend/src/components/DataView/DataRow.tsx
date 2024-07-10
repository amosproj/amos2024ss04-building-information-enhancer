import { Fragment } from "react/jsx-runtime";
import { DatasetItem } from "../../types/LocationDataTypes";
import { useState } from "react";
import {
  Box,
  Collapse,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableRow,
} from "@mui/material";
import { CaretDown, CaretUp } from "@phosphor-icons/react";
import "./DataRow.css";

interface RowProps {
  row: DatasetItem;
}

const DataRow: React.FC<RowProps> = ({ row }) => {
  const [open, setOpen] = useState(false);

  return (
    <Fragment>
      <TableRow className="data-row">
        {row.subdata.length > 0 ? (
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
          <TableCell></TableCell>
        )}

        <TableCell
          component="th"
          scope="row"
          className="data-row-title"
          size="small"
        >
          {row.displayName}
        </TableCell>
      </TableRow>
      <TableRow>
        <TableCell style={{ paddingBottom: 0, paddingTop: 0 }} colSpan={6}>
          <Collapse in={open} timeout="auto" unmountOnExit>
            <Box margin={1}>
              <Table size="small" aria-label="subdata">
                <TableBody>
                  {row.subdata.map((subItem) => (
                    <TableRow key={subItem.key}>
                      <TableCell component="th" scope="row" size="small">
                        {subItem.key}
                      </TableCell>
                      <TableCell size="small">{subItem.value}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
          </Collapse>
        </TableCell>
      </TableRow>
    </Fragment>
  );
};

export default DataRow;
