using System.Drawing;

namespace Noxy.NET.UI.Models;

public class SortableOptions
{
    public SortableOptionsGroup? Group { get; set; }

    public bool IsDisabled { get; set; } = false;
    public bool IsPreventDefaultUsedWithFilter { get; set; } = true;
    public bool IsSortable { get; set; } = true;
    public bool IsSwapInverted { get; set; } = false;
    public bool IsScrollable { get; set; } = true;
    public bool IsSupportingPointer { get; set; } = true;
    public bool ShouldAvoidImplicitDeselect { get; set; } = false;
    public bool ShouldDelayOnTouchOnly { get; set; } = false;
    public bool ShouldDragoverEventBubble { get; set; } = false;
    public bool ShouldDropEventBubble { get; set; } = false;
    public bool ShouldFallbackToBodyElement { get; set; } = false;
    public bool ShouldForceBrowserFallback { get; set; } = false;
    public bool ShouldForceAutoScrollFallback { get; set; } = false;
    public bool ShouldRemoveCloneOnHide { get; set; } = true;
    public bool ShouldScrollEventBubble { get; set; } = true;

    public int AnimationDuration { get; set; } = 0;
    public int DelayBeforeSorting { get; set; } = 0;
    public int EmptyInsertThreshold { get; set; } = 5;
    public int FallbackTolerance { get; set; } = 0;
    public int TouchStartThreshold { get; set; } = 1;
    public int ScrollSensitivity { get; set; } = 30;
    public int ScrollSpeed { get; set; } = 10;

    public double? InvertSwapThreshold { get; set; } = null;
    public double SwapThreshold { get; set; } = 1;

    public string ChosenClass { get; set; } = "sortable-chosen";
    public string DataIDAttribute { get; set; } = "data-id";
    public string DragClass { get; set; } = "sortable-drag";
    public string DraggableSelector { get; set; } = ">*";
    public string? EasingFunction { get; set; } = null;
    public string FallbackClass { get; set; } = "sortable-fallback";
    public string? FilterSelector { get; set; } = null;
    public string GhostClass { get; set; } = "sortable-ghost";
    public string? HandleSelector { get; set; } = null;
    public string? MultiDragKey { get; set; } = null;
    public string Ignore { get; set; } = "a, img";
    public string SelectedClass { get; set; } = "sortable-selected";
    public string SortingDirection { get; set; } = "horizontal";
    public string SwapClass { get; set; } = "sortable-swap-highlight";

    public Point FallbackOffset { get; set; } = new(0, 0);

    public Dictionary<string, object?> ToJSON()
    {
        return new()
        {
            { "animation", AnimationDuration },
            { "avoidImplicitDeselect", ShouldAvoidImplicitDeselect },
            { "bubbleScroll", ShouldScrollEventBubble },
            { "chosenClass", ChosenClass },
            { "dataIdAttr", DataIDAttribute },
            { "delay", DelayBeforeSorting },
            { "delayOnTouchOnly", ShouldDelayOnTouchOnly },
            { "direction", SortingDirection },
            { "disabled", IsDisabled },
            { "dragClass", DragClass },
            { "draggable", DraggableSelector },
            { "dragoverBubble", ShouldDragoverEventBubble },
            { "dropBubble", ShouldDropEventBubble },
            { "easing", EasingFunction },
            { "emptyInsertThreshold", EmptyInsertThreshold },
            { "fallbackClass", FallbackClass },
            { "fallbackOffest", FallbackOffset },
            { "fallbackOnBody", ShouldFallbackToBodyElement },
            { "fallbackTolerance", FallbackTolerance },
            { "filter", FilterSelector },
            { "forceAutoScrollFallback", ShouldForceAutoScrollFallback },
            { "forceFallback", ShouldForceBrowserFallback },
            { "ghostClass", GhostClass },
            { "group", Group?.ToJSON() },
            { "handle", HandleSelector },
            { "ignore", Ignore },
            { "invertSwap", IsSwapInverted },
            { "invertSwapThreshold", InvertSwapThreshold },
            // { "multiDragKey", MultiDragKey },
            { "preventOnFilter", IsPreventDefaultUsedWithFilter },
            { "removeCloneOnHide", ShouldRemoveCloneOnHide },
            { "scroll", IsScrollable },
            { "scrollSensitivity", ScrollSensitivity },
            { "scrollSpeed", ScrollSpeed },
            { "selectedClass", SelectedClass },
            { "sort", IsSortable },
            { "supportPointer", IsSupportingPointer },
            { "swapClass", SwapClass },
            { "swapThreshold", SwapThreshold },
            { "touchStartThreshold", TouchStartThreshold },
        };
    }
}

public class SortableOptionsGroup
{
    public string? Name { get; set; }
    public SortablePullOption? Pull { get; set; }
    public SortablePutOption? Put { get; set; }
    public bool ShouldRevertClone { get; set; }

    public Dictionary<string, object?> ToJSON()
    {
        return new()
        {
            { "name", Name },
            { "pull", Pull?.ToJSON() },
            { "put", Put?.ToJSON() },
            { "revertClone", ShouldRevertClone },
        };
    }
}

public abstract class SortablePullOption(object? value)
{
    private object? Value { get; } = value;

    public object? ToJSON()
    {
        return Value;
    }

    public class Primitive(bool value) : SortablePullOption(value);

    public class Clone() : SortablePullOption("clone");

    public class Array(string[] value) : SortablePullOption(value);
}

public abstract class SortablePutOption(object? value)
{
    private object? Value { get; } = value;

    public object? ToJSON()
    {
        return Value;
    }

    public class Primitive(bool value) : SortablePutOption(value);

    public class Array(string[] value) : SortablePutOption(value);
}
