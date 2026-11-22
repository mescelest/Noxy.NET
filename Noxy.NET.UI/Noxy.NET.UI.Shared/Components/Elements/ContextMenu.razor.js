const propertyNarrow = "--layout-narrow";
const queryCoarsePointer = "(pointer: coarse)";

// A narrow window or a touch screen has no room, and no hover, for a sub-menu that flies out to the side.
export function IsDrillIn() {
    const narrow = getComputedStyle(document.documentElement).getPropertyValue(propertyNarrow).trim() === "1";
    return narrow || window.matchMedia(queryCoarsePointer).matches;
}

// Without an index the menu itself takes the focus, so its keys work before anything is chosen.
export function FocusItem(menu, index) {
    if (!menu) return;

    const item = index === null ? null : menu.querySelector(`:scope > [data-index="${index}"]`);
    (item ?? menu).focus();
}

export function FocusTrigger(element) {
    if (!element) return;

    if (document.activeElement === document.body || document.activeElement === null) element.focus();
}
