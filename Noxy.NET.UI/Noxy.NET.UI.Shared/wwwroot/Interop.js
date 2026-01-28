// noinspection JSUnusedGlobalSymbols
export class NoxyNETUI {
    static DropdownCollection = {};

    static KeyDownPreventDefaultForKeyList(element, list) {
        element.addEventListener("keydown", e => {
            if (list.includes(e.key)) {
                e.preventDefault();
            }
        });
    }

    static EventPreventDefaultForElementEvent(element, event) {
        element.addEventListener(event, e => e.preventDefault());
    }

    static EventStopPropagationForElementEvent(element, event) {
        element.addEventListener(event, e => e.stopPropagation());
    }

    static SetTheme(theme) {
        window.document.documentElement.setAttribute("theme", theme);
        localStorage.theme = theme;
    }

    static GetTheme() {
        let value = window.document.documentElement.getAttribute("theme") || localStorage.theme;
        if (!value) {
            if (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) {
                value = "Dark";
            } else if (window.matchMedia && window.matchMedia("(prefers-color-scheme: light)").matches) {
                value = "Light";
            }
        }
        return value;
    }

    static RegisterDropdown(id, ref, callback) {
        const element = document.getElementById(id);
        this.DropdownCollection[id] = {id, ref, element};
    }

    static DisposeDropdown(id) {
        delete this.DropdownCollection[id];
    }
}
