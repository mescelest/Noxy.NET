const watchMap = new WeakMap();

const attributeRow = "[data-row]";

// The rows of the list are all one height (the CSS sets it), so the place of a row is its number times that height.
function RowHeight(container) {
    const row = container.querySelector(attributeRow);
    return row ? row.getBoundingClientRect().height : 0;
}

// The header sticks to the top of the scroller, so the rows are seen in the part below it.
function HeaderHeight(container) {
    const header = container.querySelector(".header-row");
    return header ? header.getBoundingClientRect().height : 0;
}

function IsVisible(container, index) {
    const height = RowHeight(container);
    if (height === 0) return true;

    const top = index * height - container.scrollTop;
    return top + height > 0 && top < container.clientHeight - HeaderHeight(container);
}

// The virtual list draws its first rows a moment after the list is there, so with no row yet to measure this tries again on the next frame, for about a second.
const retryCount = 60;

// Puts the row near the top, with a few rows above it. The scroll it causes is not the user's.
export function ScrollToIndex(container, index, rowsAbove, triesLeft = retryCount) {
    const height = RowHeight(container);
    if (height === 0) {
        if (triesLeft > 0) requestAnimationFrame(() => ScrollToIndex(container, index, rowsAbove, triesLeft - 1));
        return;
    }

    const state = watchMap.get(container);
    if (state) {
        state.isProgrammatic = true;
        state.isAway = false;
    }

    container.scrollTop = Math.max(0, (index - rowsAbove) * height);

    requestAnimationFrame(() => requestAnimationFrame(() => {
        if (state) state.isProgrammatic = false;
    }));
}

// Which row is the one that is playing, so the list can tell when it is out of view.
export function SetCurrent(container, index) {
    const state = watchMap.get(container);
    if (state) state.index = index;
}

// Tells the component only when the song that is playing goes out of view or comes back into it by the user's scrolling, not when the list scrolled itself.
export function Watch(container, dotNetReference, methodName) {
    const state = {index: -1, isAway: false, isProgrammatic: false};
    state.handler = () => {
        if (state.isProgrammatic || state.index < 0) return;

        const isAway = !IsVisible(container, state.index);
        if (isAway === state.isAway) return;

        state.isAway = isAway;
        dotNetReference.invokeMethodAsync(methodName, isAway).catch(() => {
        });
    };

    container.addEventListener("scroll", state.handler, {passive: true});
    watchMap.set(container, state);
}

export function Unwatch(container) {
    const state = watchMap.get(container);
    if (!state) return;

    container.removeEventListener("scroll", state.handler);
    watchMap.delete(container);
}
