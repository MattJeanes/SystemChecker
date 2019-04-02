// https://github.com/stevepapa/ng-autosize
// Package currently isn't published to npm properly so can't use yet + made small adjustments to it

import { AfterContentChecked, Directive, ElementRef, HostListener, Input } from "@angular/core";

@Directive({
    selector: "textarea[autosize]",
})

export class Autosize implements AfterContentChecked {

    private el: HTMLElement;
    // tslint:disable:variable-name
    private _minHeight: string;
    private _maxHeight: string;
    private _clientWidth: number;
    // tslint:enable:variable-name

    @Input("minHeight")
    get minHeight(): string {
        return this._minHeight;
    }
    set minHeight(val: string) {
        this._minHeight = val;
        this.updateMinHeight();
    }

    @Input("maxHeight")
    get maxHeight(): string {
        return this._maxHeight;
    }
    set maxHeight(val: string) {
        this._maxHeight = val;
        this.updateMaxHeight();
    }

    constructor(public element: ElementRef) {
        this.el = element.nativeElement;
        this._clientWidth = this.el.clientWidth;
    }

    @HostListener("window:resize", ["$event.target"])
    public onResize(textArea: HTMLTextAreaElement): void {
        // Only apply adjustment if element width had changed.
        if (this.el.clientWidth === this._clientWidth) {
            return;
        }
        this._clientWidth = this.element.nativeElement.clientWidth;
        this.adjust();
    }

    @HostListener("input", ["$event.target"])
    public onInput(textArea: HTMLTextAreaElement) {
        this.adjust();
    }

    public ngAfterContentChecked() {
        // set element resize allowed manually by user
        const style = window.getComputedStyle(this.el, undefined);
        if (style.resize === "both") {
            this.el.style.resize = "horizontal";
        } else if (style.resize === "vertical") {
            this.el.style.resize = "none";
        }
        // run first adjust
        this.adjust();
    }

    public adjust() {
        const oldHeight = this.el.style.height;
        this.el.style.height = "1px";
        // perform height adjustments after input changes, if height is different
        if (oldHeight === this.element.nativeElement.scrollHeight + "px") {
            this.el.style.height = oldHeight;
            return;
        }
        this.el.style.overflow = "hidden";
        this.el.style.height = "auto";
        this.el.style.height = this.el.scrollHeight + "px";
    }

    public updateMinHeight() {
        // Set textarea min height if input defined
        this.el.style.minHeight = this._minHeight + "px";
    }

    public updateMaxHeight() {
        // Set textarea max height if input defined
        this.el.style.maxHeight = this._maxHeight + "px";
    }
}
