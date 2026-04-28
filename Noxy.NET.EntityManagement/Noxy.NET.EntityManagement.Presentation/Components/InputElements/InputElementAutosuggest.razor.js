export function RegisterAutosuggest(delay, component, input, refDotNet, listCallback) {
    const [ArrowDown, ArrowUp, Enter, Escape, Tab, OnBlur, OnValueInput] = listCallback;

    input._ref = refDotNet;
    input._delay = delay;
    input._callback = {ArrowDown, ArrowUp, Enter, Escape, Tab, OnBlur, OnValueInput};
    input._clickOutsideHandler = function (e) {
        if (!component.contains(e.target) && component.contains(document.activeElement)) {
            input._ref.invokeMethodAsync(input._callback.OnBlur);
        }
    };

    input.addEventListener("keydown", HandleKeyDown);
    input.addEventListener("input", HandleInput)
    document.addEventListener("mousedown", input._clickOutsideHandler);
}

export function DeregisterAutosuggest(element) {
    element.removeEventListener("keydown", HandleKeyDown);

    if (element._clickOutsideHandler) {
        document.removeEventListener("mousedown", element._clickOutsideHandler);
    }
}

function HandleInput(event) {
    const element = event.target;

    if (element._timeout) {
        clearTimeout(element._timeout);
    }
    element._timeout = setTimeout(() => {
        element._ref.invokeMethodAsync(element._callback.OnValueInput, element.value);
    }, element._delay);
}

async function HandleKeyDown(event) {
    const {_ref, _callback} = event.target;

    if (event.key === "ArrowDown") {
        event.preventDefault();
        _ref.invokeMethodAsync(_callback.ArrowDown);
    } else if (event.key === "ArrowUp") {
        event.preventDefault();
        _ref.invokeMethodAsync(_callback.ArrowUp);
    } else if (event.key === "Enter") {
        event.preventDefault();
        _ref.invokeMethodAsync(_callback.Enter);
    } else if (event.key === "Escape") {
        event.preventDefault();
        _ref.invokeMethodAsync(_callback.Escape);
    } else if (event.key === "Tab") {
        _ref.invokeMethodAsync(_callback.Tab);
    }
}
