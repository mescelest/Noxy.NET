const watchMap = new WeakMap();

const tolerance = 1;

// Reports whether the tab list is wider than its box, and whether it can still move each way. Only a change is reported, since a scroll fires many events.
export function Watch(list, dotNetReference, methodName) {
    let last = "";

    const report = () => {
        const maxScroll = list.scrollWidth - list.clientWidth;
        const isOverflowing = maxScroll > tolerance;
        const canBack = list.scrollLeft > tolerance;
        const canForward = list.scrollLeft < maxScroll - tolerance;

        const state = `${isOverflowing}|${canBack}|${canForward}`;
        if (state === last) return;

        last = state;
        dotNetReference.invokeMethodAsync(methodName, isOverflowing, canBack, canForward).catch(() => {
        });
    };

    // The list changes size with the window, and its content with each tab that comes, goes or changes width.
    const resizeObserver = new ResizeObserver(report);
    const observeTabs = () => {
        for (const tab of list.children) resizeObserver.observe(tab);
    };
    const mutationObserver = new MutationObserver(() => {
        observeTabs();
        report();
    });

    resizeObserver.observe(list);
    mutationObserver.observe(list, {childList: true});
    list.addEventListener("scroll", report, {passive: true});
    observeTabs();
    report();

    watchMap.set(list, {resizeObserver, mutationObserver, report});
}

export function Unwatch(list) {
    const watch = watchMap.get(list);
    if (!watch) return;

    watch.resizeObserver.disconnect();
    watch.mutationObserver.disconnect();
    list.removeEventListener("scroll", watch.report);
    watchMap.delete(list);
}

// One tab width to the side: the width of the next tab that is not fully in view in that direction.
export function Scroll(list, direction) {
    const view = list.getBoundingClientRect();
    const tabList = [...list.children];
    const target = direction > 0
        ? tabList.find(tab => tab.getBoundingClientRect().right > view.right + tolerance)
        : tabList.findLast(tab => tab.getBoundingClientRect().left < view.left - tolerance);
    if (!target) return;

    list.scrollBy({left: direction * target.getBoundingClientRect().width, behavior: "smooth"});
}

export function ScrollToTab(tab) {
    tab.scrollIntoView({inline: "nearest", block: "nearest", behavior: "smooth"});
}
