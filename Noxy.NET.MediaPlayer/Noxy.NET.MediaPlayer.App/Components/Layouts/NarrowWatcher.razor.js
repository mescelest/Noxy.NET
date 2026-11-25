const listenerMap = new Map();

// The window is narrow when breakpoints.css says so: the condition is written there only, this reads the flag it sets.
export function IsNarrow() {
    return getComputedStyle(document.documentElement).getPropertyValue("--layout-narrow").trim() === "1";
}

export function Watch(key, dotNetReference, methodName) {
    let narrow = IsNarrow();
    const onResize = () => {
        const now = IsNarrow();
        if (now === narrow) return;

        narrow = now;
        dotNetReference.invokeMethodAsync(methodName, now).catch(() => {
        });
    };

    window.addEventListener("resize", onResize);
    listenerMap.set(key, onResize);
}

export function Unwatch(key) {
    const onResize = listenerMap.get(key);
    if (!onResize) return;

    window.removeEventListener("resize", onResize);
    listenerMap.delete(key);
}
