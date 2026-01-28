const collection = {};

export function RegisterCollapsible(id, refDotNet, method) {
    const element = document.getElementById(id);

    collection[id] = event => {
        if (event.propertyName !== "grid-template-rows") return;
        refDotNet.invokeMethodAsync(method, element.getAttribute("state") === "1");
    };

    element.addEventListener('transitionend', collection[id]);
}

export function DisposeCollapsible(id) {
    const element = document.getElementById(id);
    element.removeEventListener('transitionend', collection[id]);
    delete collection[id];
}

export function AnimateExpand(id) {
    const element = document.getElementById(id);
    element.style.marginTop = '';
    element.style.gridTemplateRows = 'minmax(0, 1fr)';
    element.setAttribute("state", "0");
}

export function AnimateCollapse(id) {
    const element = document.getElementById(id);
    element.style.marginTop = "0px";
    element.style.gridTemplateRows = 'minmax(0, 0fr)';
    element.setAttribute("state", "1");
}