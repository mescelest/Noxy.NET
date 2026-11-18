export function initSortable(el, dotNetRef, options) {
    const Sortable = window.Sortable || globalThis.Sortable;

    let trueOldIndex = 0;
    let trueNewIndex = 0;

    options.sort = true;
    options.animation = 0;

    options.onStart = function (evt) {
        trueOldIndex = evt.oldIndex;
        trueNewIndex = evt.oldIndex;
    };

    options.onUpdate = function (evt) {
        trueNewIndex = evt.newIndex;
    };

    options.onEnd = function (evt) {
        if (trueOldIndex === trueNewIndex) {
            return;
        }

        const parent = evt.from;

        if (trueNewIndex < trueOldIndex) {
            parent.insertBefore(evt.item, parent.children[trueOldIndex + 1]);
        } else {
            parent.insertBefore(evt.item, parent.children[trueOldIndex]);
        }

        requestAnimationFrame(() => {
            dotNetRef.invokeMethodAsync('OnJSUpdate', trueOldIndex, trueNewIndex);
        });
    };

    return Sortable.create(el, options);
} 