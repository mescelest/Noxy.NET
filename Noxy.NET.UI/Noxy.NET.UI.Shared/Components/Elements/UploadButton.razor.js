// The value is cleared first, so picking the same files again still raises a change.
export function Open(element) {
    element.value = "";
    element.click();
}
