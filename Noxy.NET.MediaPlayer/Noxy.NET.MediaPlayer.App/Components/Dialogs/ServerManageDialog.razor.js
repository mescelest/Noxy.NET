// Tells the component when a click lands outside the element that matches the selector. The click itself is left alone, so whatever was clicked still does its own work.
export function WatchOutsideClick(dotNetReference, methodName, selector) {
    const onClick = (event) => {
        const element = document.querySelector(selector);
        if (element === null || element.contains(event.target)) return;

        dotNetReference.invokeMethodAsync(methodName).catch(() => {
        });
    };

    // Capture phase, so the component hears about it before the clicked element's own handler runs.
    window.addEventListener("click", onClick, true);
    return DotNet.createJSObjectReference({dispose: () => window.removeEventListener("click", onClick, true)});
}
