const selectorOpen = "dialog[open], :popover-open";
const functionPress = "Press";

let handler = null;

// A key is handled only when nothing but the page has the focus: a field, a button or an open dialog or menu keeps it.
function IsFree(event) {
    if (event.ctrlKey || event.altKey || event.metaKey || event.shiftKey || event.repeat) return false;
    if (document.activeElement !== document.body) return false;

    return !document.querySelector(selectorOpen);
}

export function Watch(reference, keyList) {
    if (handler) return;

    const keySet = new Set(keyList);
    handler = event => {
        if (!keySet.has(event.code) || !IsFree(event)) return;

        // Decided here, not after the round trip to the app: by then the page has scrolled.
        event.preventDefault();
        reference.invokeMethodAsync(functionPress, event.code);
    };
    window.addEventListener("keydown", handler);
}

export function Unwatch() {
    if (!handler) return;

    window.removeEventListener("keydown", handler);
    handler = null;
}
