// noinspection JSUnusedGlobalSymbols

export function RegisterOnInput(element, delay, refDotNot, cb) {
    element._ref = refDotNot;
    element._callback = cb;
    element._delay = delay;

    console.log("Test register", element, delay, refDotNot, cb);

    element.addEventListener("input", HandleOnInput);
}

function HandleOnInput(e) {
    const element = e.target;

    if (element._timeout) {
        console.log("Clearing timeout", element._timeout);
        clearTimeout(element._timeout);
    }
    element._timeout = setTimeout(() => {
        console.log("Test call", element._callback, element.value);
        element._ref.invokeMethodAsync(element._callback, element.value);
    }, element._delay);
    console.log("Current timeout", element._timeout);
}

export function DeregisterOnInput(element) {
    element.removeEventListener('input', HandleOnInput);
    clearTimeout(element._timeout);
}

export function SetInputValue(element, value) {
    element.value = value;
}
