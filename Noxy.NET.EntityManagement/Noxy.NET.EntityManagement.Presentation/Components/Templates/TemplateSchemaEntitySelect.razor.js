export function Register(refElement, refDotNet, method) {

    new Sortable(refElement, {
        handle: ".handle",
        animation: 150,
        onUpdate: event => {
            event.item.remove();
            event.to.insertBefore(event.item, event.to.childNodes[event.oldIndex]);
            refDotNet.invokeMethodAsync(method, event.oldIndex, event.newIndex)
        }
    });
}
