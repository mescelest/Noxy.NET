export function Show(element) {
    if (!element.open) element.showModal();
}

export function Close(element) {
    if (element.open) element.close();
}

export function Watch(element, dotNetReference, methodName) {
    element.addEventListener("close", () => {
        dotNetReference.invokeMethodAsync(methodName).catch(() => {
        });
    });
}
