import { Component, forwardRef, Input, OnDestroy, OnInit } from "@angular/core";
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from "@angular/forms";
import { OptionType } from "../../app.enums";
import { IConnString, IOption, ISettings, ISlackChannel } from "../../app.interfaces";
import { AppService } from "../../services";

@Component({
    templateUrl: "./option.template.html",
    styleUrls: ["./option.style.scss"],
    selector: "check-option",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => OptionComponent),
            multi: true,
        },
    ],
})
export class OptionComponent implements ControlValueAccessor, OnInit, OnDestroy {
    @Input() public option: IOption;
    @Input() public environmentID?: number;

    public settings: ISettings;
    public slackChannels?: ISlackChannel[];

    get connStrings(): IConnString[] {
        if (this.settings) {
            if (this.environmentID) {
                return this.settings.ConnStrings.filter(x => x.EnvironmentID === this.environmentID);
            } else {
                return this.settings.ConnStrings;
            }
        } else {
            return [];
        }
    }

    public OptionType = OptionType;

    // tslint:disable-next-line:variable-name
    private _value: number = 0;

    get value() {
        return this._value;
    }

    set value(val) {
        this._value = val;
        this.propagateChange(this._value);
    }

    constructor(private appService: AppService) { }

    public async ngOnInit() {
        if (this.option.OptionType === OptionType.Login
            || this.option.OptionType === OptionType.ConnString
            || this.option.OptionType === OptionType.Environment) {
            this.settings = await this.appService.getSettings();
        }
        if (this.option.OptionType === OptionType.Slack) {
            this.slackChannels = await this.appService.getSlackChannels();
        }
    }

    public async ngOnDestroy() {
        delete this.settings;
    }

    public writeValue(value: any): void {
        this.value = value;
    }

    public propagateChange = (_: any) => {
        // empty
    }

    public registerOnChange(fn: any) {
        this.propagateChange = fn;
    }

    public registerOnTouched(fn: any) {
        // empty
    }
}
