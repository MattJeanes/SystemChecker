import { Component, forwardRef, Input, NgZone, OnDestroy, OnInit } from "@angular/core";
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from "@angular/forms";
import { ContactType, OptionType } from "../../app.enums";
import { IConnString, IContact, IOption, ISettings, ISlackChannel } from "../../app.interfaces";
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

    public OptionType = OptionType;

    public settings: ISettings;
    public slackChannels?: ISlackChannel[];

    get connStrings(): IConnString[] | undefined {
        if (this.settings) {
            if (this.environmentID) {
                return this.settings.ConnStrings.filter(x => x.EnvironmentID === this.environmentID);
            } else {
                return this.settings.ConnStrings;
            }
        } else {
            return undefined;
        }
    }

    get emails(): IContact[] | undefined {
        if (this.settings) {
            return this.settings.Contacts.filter(x => x.TypeID === ContactType.Email);
        } else {
            return undefined;
        }
    }

    get phones(): IContact[] | undefined {
        if (this.settings) {
            return this.settings.Contacts.filter(x => x.TypeID === ContactType.Phone);
        } else {
            return undefined;
        }
    }

    get environmentID() {
        return this._environmentID;
    }

    @Input() set environmentID(val) {
        if (this._environmentID && val !== this._environmentID && this.option.OptionType === OptionType.ConnString) {
            // ExpressionChangedAfterItHasBeenCheckedError is thrown if we change it now, so wait
            setTimeout(() => {
                this.ngZone.run(() => {
                    this.writeValue(undefined);
                });
            }, 0);
        }
        this._environmentID = val;
    }

    get value() {
        return this._value;
    }

    set value(val) {
        this._value = val;
        this.propagateChange(this._value);
    }

    // tslint:disable-next-line:variable-name
    private _value?: number = 0;

    // tslint:disable-next-line:variable-name
    private _environmentID: number = 0;

    constructor(private appService: AppService, private ngZone: NgZone) { }

    public async ngOnInit() {
        if (this.option.OptionType === OptionType.Login
            || this.option.OptionType === OptionType.ConnString
            || this.option.OptionType === OptionType.Environment
            || this.option.OptionType === OptionType.Email
            || this.option.OptionType === OptionType.Phone
        ) {
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
