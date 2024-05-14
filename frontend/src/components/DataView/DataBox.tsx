/*
  This component was used for working on the KeyValuesList. 
  It can be deleted if the following code part is moved to its final place:

  <Container maxWidth="sm"> //optional
        <Box sx={{ bgcolor: "#e6e6e6", p: 2 }}>
          <Typography align="left" variant="h4" gutterBottom>
            Location
          </Typography>
          <Stack spacing={2}>
            <Item>
              <KeyValuesList filterPanelId={1} />
            </Item>
            <Item>
              <KeyValuesList filterPanelId={2} />
            </Item>
            <Item>
              <KeyValuesList filterPanelId={3} />
            </Item>
          </Stack>
        </Box>
      </Container>

  ! The line "import CssBaseline from "@mui/material/CssBaseline";" 
  has to be placed before the other import statements


*/

import CssBaseline from "@mui/material/CssBaseline";
import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import KeyValuesList from "./KeyValuesList";
import Typography from "@mui/material/Typography";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import { styled } from "@mui/material/styles";

const Item = styled(Paper)(({ theme }) => ({
  backgroundColor: theme.palette.mode === "dark" ? "#1A2027" : "#fff",
  ...theme.typography.body2,
  padding: theme.spacing(1),
  textAlign: "center",
  color: theme.palette.text.secondary,
}));

function DataBox() {
  return (
    <>
      <CssBaseline />
      <h1>DataBox</h1>
      <Container maxWidth="sm">
        <Box sx={{ bgcolor: "#e6e6e6", p: 2 }}>
          <Typography align="left" variant="h4" gutterBottom>
            Location
          </Typography>
          <Stack spacing={2}>
            <Item>
              <KeyValuesList filterPanelId={1} />
            </Item>
            <Item>
              <KeyValuesList filterPanelId={2} />
            </Item>
            <Item>
              <KeyValuesList filterPanelId={3} />
            </Item>
          </Stack>
        </Box>
      </Container>
    </>
  );
}

export default DataBox;
