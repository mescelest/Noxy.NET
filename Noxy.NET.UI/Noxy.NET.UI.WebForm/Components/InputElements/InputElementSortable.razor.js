export function Register(options, refElement, refDotNet, method) {

    console.log(options);

    var args = {
        ...options,
        onUpdate: event => {
            console.log(event);
            event.item.remove();
            event.to.insertBefore(event.item, event.to.childNodes[event.oldIndex]);
            refDotNet.invokeMethodAsync(method, event.oldIndex, event.newIndex)
        }
    };

    console.log(args);
    console.log(refElement);

    var a = new Sortable(refElement, {
        ...options,
        onUpdate: event => {
            console.log(event);
            event.item.remove();
            event.to.insertBefore(event.item, event.to.childNodes[event.oldIndex]);
            refDotNet.invokeMethodAsync(method, event.oldIndex, event.newIndex)
        }
    });

    console.log(a);

}