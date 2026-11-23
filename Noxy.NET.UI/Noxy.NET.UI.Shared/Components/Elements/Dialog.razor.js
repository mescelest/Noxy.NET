const backdropHandlerMap = new WeakMap();

// The close button is marked by the component; everything else here is found by what a browser can focus.
const closeSelector = "[data-dialog-close]";
const focusableSelector = "a[href], button, input:not([type='hidden']), select, textarea, [tabindex]";

export function Show(element) {
    if (element.open) return;

    element.showModal();

    // The content can still be on its way to the page when the dialog opens, so it is looked at on the next frame.
    requestAnimationFrame(() => FocusContent(element));
}

function isFocusable(candidate) {
    return !candidate.matches(closeSelector)
        && !candidate.matches(":disabled, [tabindex='-1'], [aria-hidden='true']")
        && !candidate.closest("[inert]")
        && candidate.getClientRects().length > 0;
}

// A modal dialog focuses its first focusable element, which is the close button; the first thing after the header is the better start. Something the content focused itself (autofocus) is left alone, and with nothing else to focus the close button keeps it.
export function FocusContent(element) {
    if (!element.open) return;

    const active = document.activeElement;
    if (active !== element && element.contains(active) && !active.matches(closeSelector)) return;

    const target = Array.from(element.querySelectorAll(focusableSelector)).find(isFocusable);
    if (target) target.focus();
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
