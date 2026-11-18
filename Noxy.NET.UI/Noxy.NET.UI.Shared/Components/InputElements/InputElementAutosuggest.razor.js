export function RegisterAutosuggest(delay, component, input, refDotNet, listCallback) {
    const [ArrowDown, ArrowUp, Enter, Escape, Tab, OnBlur, OnValueInput] = listCallback;

    input._ref = refDotNet;
    input._delay = delay;
    input._callback = {ArrowDown, ArrowUp, Enter, Escape, Tab, OnBlur, OnValueInput};
    input._clickOutsideHandler = function (e) {
        if (input._ref && input._callback && !component.contains(e.target) && component.contains(document.activeElement)) {
            input._ref.invokeMethodAsync(input._callback.OnBlur);
        }
    };

    input.addEventListener("keydown", HandleKeyDown);
    input.addEventListener("input", HandleInput);
    document.addEventListener("mousedown", input._clickOutsideHandler);
}

export function DeregisterAutosuggest(element) {
    element.removeEventListener("keydown", HandleKeyDown);
    element.removeEventListener("input", HandleInput);

    if (element._clickOutsideHandler) {
        document.removeEventListener("mousedown", element._clickOutsideHandler);
    }

    if (element._timeout) {
        clearTimeout(element._timeout);
    }

    element._ref = null;
    element._callback = null;
    element._clickOutsideHandler = null;
    element._timeout = null;
    element._delay = null;
}

function HandleInput(event) {
    const element = event.target;

    if (element._timeout) {
        clearTimeout(element._timeout);
    }

    element._timeout = setTimeout(() => {
        if (element._ref && element._callback) {
            element._ref.invokeMethodAsync(element._callback.OnValueInput, element.value);
        }
    }, element._delay);
}

function ClearPendingInput(element) {
    if (element._timeout) {
        clearTimeout(element._timeout);
        element._timeout = null;
    }
}

function HandleKeyDown(event) {
    const {_ref, _callback} = event.target;
    if (!_ref || !_callback) return;

    switch (event.key) {
        case "ArrowDown":
            event.preventDefault();
            _ref.invokeMethodAsync(_callback.ArrowDown);
            break;

        case "ArrowUp":
            event.preventDefault();
            _ref.invokeMethodAsync(_callback.ArrowUp);
            break;

        case "Enter":
            event.preventDefault();
            ClearPendingInput(event.target);
            _ref.invokeMethodAsync(_callback.Enter);
            break;

        case "Escape":
            event.preventDefault();
            ClearPendingInput(event.target);
            _ref.invokeMethodAsync(_callback.Escape);
            break;

        case "Tab":
            ClearPendingInput(event.target);
            _ref.invokeMethodAsync(_callback.Tab);
            break;
    }
}
