const collection = {};

export function RegisterCollapsible(id, element, refDotNet, method) {
    collection[id] = event => {
        if (event.propertyName !== "grid-template-rows") return;
        refDotNet.invokeMethodAsync(method, element.getAttribute("state") === "1");
    };

    element.addEventListener('transitionend', collection[id]);
}

export function DisposeCollapsible(id, element) {
    element.removeEventListener('transitionend', collection[id]);
    delete collection[id];
}

export function AnimateExpand(element) {
    element.style.gridTemplateRows = 'minmax(0, 1fr)';
    element.setAttribute("state", "0");
}

export function AnimateCollapse(element) {
    element.style.gridTemplateRows = 'minmax(0, 0fr)';
    element.setAttribute("state", "1");
}

export function ResetInlineStyle(element) {
    element.style.gridTemplateRows = null;
}