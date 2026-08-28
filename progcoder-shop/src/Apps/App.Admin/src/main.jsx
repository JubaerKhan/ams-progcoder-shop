import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "simplebar-react/dist/simplebar.min.css";
import "flatpickr/dist/themes/light.css";
import "../src/assets/css/app.css";
import { BrowserRouter } from "react-router-dom";
import "react-toastify/dist/ReactToastify.css";
import { Provider } from "react-redux";
import store from "./store";
import "./i18n/config";
import { KeycloakProvider } from "./contexts/KeycloakContext";

ReactDOM.createRoot(document.getElementById("root")).render(
  <>
    {/* v7_startTransition is intentionally NOT enabled: with the app's
        React.lazy()-loaded routes, it wraps navigation in a transition that
        never flushes here, silently stranding the UI on the previous page
        with no request for the new route's chunk ever firing. */}
    <BrowserRouter future={{ v7_relativeSplatPath: true }}>
      <Provider store={store}>
        <KeycloakProvider>
          <App />
        </KeycloakProvider>
      </Provider>
    </BrowserRouter>
  </>
);
