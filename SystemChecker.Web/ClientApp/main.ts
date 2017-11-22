// Main

declare global {
    // tslint:disable-next-line:interface-name
    interface Window { paceOptions: any; }
}

window.paceOptions = {
    ajax: {
        trackWebSockets: false, // Fix SignalR
    },
};

import * as Pace from "pace-progress";

Pace.start({
    ajax: false,
    restartOnRequestAfter: false,
});

import "./styles/main.scss";

import "./polyfills";

import "./imports";

import "hammerjs";

import { enableProdMode } from "@angular/core";
import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";
import { AppModule } from "./app/app.module";

declare var module: any;

if (module.hot) {
    module.hot.accept();
    module.hot.dispose(() => {
        // Before restarting the app, we create a new root element and dispose the old one
        const oldRootElem = document.querySelector("systemchecker");
        const newRootElem = document.createElement("systemchecker");
        if (oldRootElem && oldRootElem.parentNode) {
            oldRootElem.parentNode.insertBefore(newRootElem, oldRootElem);
            oldRootElem.parentNode.removeChild(oldRootElem);
        }
        modulePromise.then(appModule => appModule.destroy());
    });
} else {
    enableProdMode();
}

const modulePromise = platformBrowserDynamic().bootstrapModule(AppModule);
