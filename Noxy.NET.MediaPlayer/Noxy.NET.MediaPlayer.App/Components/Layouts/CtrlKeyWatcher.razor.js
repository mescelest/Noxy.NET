const attributeCtrl = "data-ctrl";

let handlerMap = null;

// While ctrl is held the page carries the attribute, so a stylesheet can show what ctrl does (a pointer, a tick box that lights up) without a round trip to the app for every key.
function Update(event) {
    const root = document.documentElement;
    const isHeld = event.ctrlKey;

    if (isHeld === root.hasAttribute(attributeCtrl)) return;

    if (isHeld) root.setAttribute(attributeCtrl, "");
    else root.removeAttribute(attributeCtrl);
}

function Clear() {
    document.documentElement.removeAttribute(attributeCtrl);
}

export function Watch() {
    if (handlerMap) return;

    // The pointer is listened to as well: ctrl can be pressed while another window has the focus, and the first move over this one then tells the truth.
    handlerMap = {keydown: Update, keyup: Update, mousemove: Update, blur: Clear};
    for (const [name, handler] of Object.entries(handlerMap)) window.addEventListener(name, handler);
}

export function Unwatch() {
    if (!handlerMap) return;

    for (const [name, handler] of Object.entries(handlerMap)) window.removeEventListener(name, handler);
    handlerMap = null;
    Clear();
}
