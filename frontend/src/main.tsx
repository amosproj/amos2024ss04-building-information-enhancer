import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import "./index.css";
import { TabsContextProvider } from "./contexts/TabsContext.tsx";
import { MapContextProvider } from "./contexts/MapContext.tsx";
import { SearchContextProvider } from "./contexts/SearchContext.tsx";
import { AlertContextProvider } from "./contexts/AlertContext.tsx";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <AlertContextProvider>
      <TabsContextProvider>
        <MapContextProvider>
          <SearchContextProvider>
            <App />
          </SearchContextProvider>
        </MapContextProvider>
      </TabsContextProvider>
    </AlertContextProvider>
  </React.StrictMode>
);
