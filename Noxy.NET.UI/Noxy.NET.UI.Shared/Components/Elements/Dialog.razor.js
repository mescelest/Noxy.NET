export function Show(element) {
    if (!element.open) element.showModal();
}

export function Close(element) {
    if (element.open) element.close();
}

export function Watch(element, dotNetReference, methodName) {
    const onClose = () => dotNetReference.invokeMethodAsync(methodName);
    element.addEventListener("close", onClose);
    return DotNet.createJSObjectReference({dispose: () => element.removeEventListener("close", onClose)});
}
