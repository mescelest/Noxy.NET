// noinspection JSUnusedGlobalSymbols

export class InputControlColor {
    constructor(windowElement, dotNetHelper) {
        this.windowElement = windowElement;
        this.dotNetHelper = dotNetHelper;
        this.isDragging = false;
        this.ticking = false;
        this.latestEvent = null;
        this.cachedRect = null;

        this.onMouseDown = this.onMouseDown.bind(this);
        this.onMouseMove = this.onMouseMove.bind(this);
        this.onMouseUp = this.onMouseUp.bind(this);

        this.windowElement.addEventListener('mousedown', this.onMouseDown);
    }

    onMouseDown(e) {
        if (e.button !== 0) return;
        this.isDragging = true;
        this.latestEvent = e;
        this.cachedRect = this.windowElement.getBoundingClientRect();

        this.updateColorThrottled();

        window.addEventListener('mousemove', this.onMouseMove);
        window.addEventListener('mouseup', this.onMouseUp);
    }

    onMouseMove(e) {
        if (!this.isDragging) return;
        this.latestEvent = e;

        if (!this.ticking) {
            window.requestAnimationFrame(() => {
                this.updateColorThrottled();
                this.ticking = false;
            });
            this.ticking = true;
        }
    }

    onMouseUp() {
        if (!this.isDragging) return;
        this.isDragging = false;

        window.removeEventListener('mousemove', this.onMouseMove);
        window.removeEventListener('mouseup', this.onMouseUp);

        this.dotNetHelper.invokeMethodAsync('OnDragEnd');
    }

    updateColorThrottled() {
        if (!this.latestEvent || !this.cachedRect) return;

        const rect = this.cachedRect;
        const x = Math.max(0, Math.min(this.latestEvent.clientX - rect.left, rect.width));
        const y = Math.max(0, Math.min(this.latestEvent.clientY - rect.top, rect.height));

        const saturationPercentage = (x / rect.width) * 100;
        const valuePercentage = (1 - (y / rect.height)) * 100;

        this.dotNetHelper.invokeMethodAsync('UpdateColorPreviewFromJs', saturationPercentage, valuePercentage);
    }

    dispose() {
        this.windowElement.removeEventListener('mousedown', this.onMouseDown);
    }
}

export function init(windowElement, dotNetHelper) {
    return new InputControlColor(windowElement, dotNetHelper);
}