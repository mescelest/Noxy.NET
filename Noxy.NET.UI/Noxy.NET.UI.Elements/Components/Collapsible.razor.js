export function RegisterCollapsible(id, isCollapsed, element, refDotNet, method) {
    element._state = isCollapsed;
    console.log("Element is initialized to state:", element._state);

    element._callback = event => {
        if (event.propertyName !== "grid-template-rows") return;
        console.log(element._state);
        refDotNet.invokeMethodAsync(method, element._state);
    };

    element.addEventListener('transitionend', element._callback);
}

export function DisposeCollapsible(id, element) {
    element.removeEventListener('transitionend', element._callback);
    delete element._callback;
    delete element._state;
}

export function AnimateExpand(element) {
    console.log("expanding");
    element._state = false;
    element.style.gridTemplateRows = 'minmax(0, 1fr)';
}

export function AnimateCollapse(element) {
    console.log("collapsing");
    element._state = true;
    element.style.gridTemplateRows = 'minmax(0, 0fr)';
}

export function ResetInlineStyle(element) {
    element.style.gridTemplateRows = null;
}