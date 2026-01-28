// noinspection JSUnusedGlobalSymbols
export class NoxyNETUIWebForm {
    static OnInputCollection = {};

    static RegisterOnInput(id, delay, ref, cb) {
        const element = document.getElementById(id);
        this.OnInputCollection[id] = {id, element, timer: undefined};

        document.getElementById(id).addEventListener("oninput", () => {
            this.OnInputCollection[id].timer = setTimeout(() => {
                ref.invokeMethodAsync(cb, this.OnInputCollection[id].element.value);
            }, delay);
        });
    }

    static DeregisterOnInput(id) {
        this.OnInputCollection[id].element.removeEventListener('oninput');
        clearTimeout(this.OnInputCollection[id].timer);
        delete this.OnInputCollection[id];
    }

    static SetInputValue(id, value) {
        document.getElementById(id).value = value;
    }
}
