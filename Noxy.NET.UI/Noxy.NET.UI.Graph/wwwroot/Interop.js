export class NoxyNETUI_Graph {
    static GraphViewerDrag(ref, element, callback, speed = 1, x = 0, y = 0) {
        element.addEventListener("mousedown", () => {
            window.addEventListener("mouseup", onMouseUp);
            window.addEventListener("mousemove", onMouseMove);
        });

        function onMouseMove(event) {
            x += event.movementX * speed;
            y += event.movementY * speed;
            element.querySelector(".translate").setAttribute("transform", `translate(${x} ${y})`);
        }

        function onMouseUp() {
            window.removeEventListener("mousemove", onMouseMove);
            window.removeEventListener("mouseup", onMouseUp);
            ref.invokeMethodAsync(callback, x, y);
        }
    }

    static GraphViewerZoom(ref, element, callback, level = 1, delta = 0.2, min = 0.2, max = 3.0) {
        element.addEventListener("wheel", event => {
            event.preventDefault();
            level = Math.min(Math.max(min, level + delta * (event.deltaY > 0 ? -1 : 1)), max);
            element.querySelector(".zoom").setAttribute("transform", `scale(${level})`);
            ref.invokeMethodAsync(callback, level);
        });
    }
}
