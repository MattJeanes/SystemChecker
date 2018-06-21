import { Injectable } from "@angular/core";
import { CanDeactivate } from "@angular/router";
import { Observable } from "rxjs";

export interface ICanComponentDeactivate {
    canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}

@Injectable()
export class CanDeactivateGuard implements CanDeactivate<ICanComponentDeactivate> {
    public canDeactivate(component: ICanComponentDeactivate) {
        return component ? component.canDeactivate ? component.canDeactivate() : true : true;
    }
}
