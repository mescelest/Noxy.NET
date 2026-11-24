const resizeHandlerMap = new WeakMap();

const defaultGap = 4;
const defaultMargin = 8;
const propertyGap = "--popover-gap";
const propertyMargin = "--popover-margin";
const attributePlaced = "data-placed";
const attributeInvokerOpen = "data-popover-open";

export function Show(panel) {
    if (!panel.matches(":popover-open")) panel.showPopover();
}

export function Hide(panel) {
    if (panel.matches(":popover-open")) panel.hidePopover();
}

// Without an anchor the trigger that points at the panel with popovertarget is the anchor. With neither, the anchor is a
// point and the component uses the point it was given.
export function Measure(panel, anchor) {
    const box = panel.getBoundingClientRect();
    const target = anchor ?? FindInvoker(panel);
    const rect = target ? target.getBoundingClientRect() : {x: 0, y: 0, width: 0, height: 0};
    const style = getComputedStyle(panel);

    return {
        anchor: {x: rect.x, y: rect.y, width: rect.width, height: rect.height},
        panelWidth: box.width,
        panelHeight: box.height,
        viewportWidth: window.innerWidth,
        viewportHeight: window.innerHeight,
        gap: ToPixels(style.getPropertyValue(propertyGap), style, defaultGap),
        margin: ToPixels(style.getPropertyValue(propertyMargin), style, defaultMargin)
    };
}

export function Apply(panel, x, y, maxHeight) {
    panel.style.left = `${x}px`;
    panel.style.top = `${y}px`;
    panel.style.maxHeight = `${maxHeight}px`;
    panel.setAttribute(attributePlaced, "");
}

// Reports when the browser opens or closes the panel, however that came about: the trigger, a click away, Escape or a call.
export function Watch(panel, dotNetReference, openedMethod, closedMethod) {
    panel.addEventListener("toggle", (event) => {
        if (event.newState === "open") {
            Reset(panel);
            AddResizeClose(panel);
            MarkInvoker(panel, true);
            dotNetReference.invokeMethodAsync(openedMethod).catch(() => {
            });
            return;
        }

        RemoveResizeClose(panel);
        Reset(panel);
        MarkInvoker(panel, false);
        dotNetReference.invokeMethodAsync(closedMethod).catch(() => {
        });
    });
}

function FindInvoker(panel) {
    if (!panel.id) return null;

    return document.querySelector(`[popovertarget="${CSS.escape(panel.id)}"]`);
}

// The trigger that points at the panel says the panel is open for as long as it is, so it can be drawn pressed.
function MarkInvoker(panel, isOpen) {
    const invoker = FindInvoker(panel);
    if (!invoker) return;

    if (isOpen) invoker.setAttribute(attributeInvokerOpen, "");
    else invoker.removeAttribute(attributeInvokerOpen);
}

function Reset(panel) {
    panel.removeAttribute(attributePlaced);
    panel.style.left = "";
    panel.style.top = "";
    panel.style.maxHeight = "";
}

// The panel is placed once, so a resized window closes it instead of leaving it where it no longer fits.
function AddResizeClose(panel) {
    if (resizeHandlerMap.has(panel)) return;

    const handler = () => Hide(panel);
    window.addEventListener("resize", handler);
    resizeHandlerMap.set(panel, handler);
}

function RemoveResizeClose(panel) {
    const handler = resizeHandlerMap.get(panel);
    if (!handler) return;

    window.removeEventListener("resize", handler);
    resizeHandlerMap.delete(panel);
}

function ToPixels(value, style, fallback) {
    const text = value.trim();
    const number = parseFloat(text);
    if (Number.isNaN(number)) return fallback;

    if (text.endsWith("rem")) return number * parseFloat(getComputedStyle(document.documentElement).fontSize);
    if (text.endsWith("em")) return number * parseFloat(style.fontSize);
    return number;
}
