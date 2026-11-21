const backdropHandlerMap = new WeakMap();

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

// The backdrop is the dialog element itself, so a click counts as outside when it is not inside the dialog's box.
export function SetBackdropClose(element, enabled) {
    const registered = backdropHandlerMap.has(element);
    if (enabled === registered) return;

    if (!enabled) {
        element.removeEventListener("click", backdropHandlerMap.get(element));
        backdropHandlerMap.delete(element);
        return;
    }

    const handler = (event) => {
        if (event.target !== element) return;

        const box = element.getBoundingClientRect();
        const isInside = event.clientX >= box.left && event.clientX <= box.right && event.clientY >= box.top && event.clientY <= box.bottom;
        if (!isInside) element.close();
    };

    element.addEventListener("click", handler);
    backdropHandlerMap.set(element, handler);
}
