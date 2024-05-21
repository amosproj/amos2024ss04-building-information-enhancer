import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import "./index.css";
import { TabsContextProvider } from "./contexts/TabsContext.tsx";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <TabsContextProvider>
      <App />
    </TabsContextProvider>
  </React.StrictMode>
);
