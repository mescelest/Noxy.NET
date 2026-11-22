// The window is narrow when breakpoints.css says so: the condition is written there only, this reads the flag it sets.
export function IsNarrow() {
    return getComputedStyle(document.documentElement).getPropertyValue("--layout-narrow").trim() === "1";
}

export function WatchNarrow(dotNetReference, methodName) {
    let narrow = IsNarrow();
    const onResize = () => {
        const now = IsNarrow();
        if (now === narrow) return;

        narrow = now;

        // The reference may already be released when a last resize arrives: that is not an error.
        dotNetReference.invokeMethodAsync(methodName, now).catch(() => {
        });
    };

    window.addEventListener("resize", onResize);
    return DotNet.createJSObjectReference({dispose: () => window.removeEventListener("resize", onResize)});
}
