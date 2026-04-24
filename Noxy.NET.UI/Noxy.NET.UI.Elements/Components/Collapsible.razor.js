export function RegisterCollapsible(isCollapsed, element, refDotNet, method) {
    if (!element) return;
    
    element._state = isCollapsed;

    element._callback = event => {
        if (!refDotNet || event.propertyName !== "grid-template-rows") return;
        refDotNet.invokeMethodAsync(method, element._state);
    };

    element.addEventListener('transitionend', element._callback);
}

export function AnimateExpand(element) {
    element._state = false;
    element.style.gridTemplateRows = 'minmax(0, 1fr)';
}

export function AnimateCollapse(element) {
    element._state = true;
    element.style.gridTemplateRows = 'minmax(0, 0fr)';
}

export function ResetInlineStyle(element) {
    element.style.gridTemplateRows = null;
}
