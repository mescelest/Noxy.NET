# Noxy.NET.UI.Common

## Styling

### How it works

The components carry no colours or sizes of their own. They read CSS variables, almost always with a built-in fallback (`var(--size-gap-xs, 0.75rem)`), so a page works without defining anything, but defining the variables once gives every component the same
look.

- Define the variables on `:root` in a stylesheet that loads before the component styles. Copy `wwwroot/css/variables.css` from this library into your project (see *Quick start*) and edit it. It holds every variable the components read, colours included.
- Sizes are in `rem`, so they follow the user's font size and the browser's text zoom. Border widths, shadows and blurs stay in `px`, because a hairline should stay a hairline.
- The theme is the `theme` attribute on `<html>` (`Light` or `Dark`). `ThemeToggle` and `ThemeService` read the stored choice, else the system's preference, set the attribute, and remember a choice in `localStorage`. To avoid a flash of the wrong theme before
  Blazor starts, set the attribute from storage in the page head and let the CSS follow the system when there is no attribute (both are in *Quick start*).
- Variables the shared components read are marked **used** below. The rest are the same family of names, kept so an app can use them for its own components.

### Quick start

In the page head, before any stylesheet, so the stored theme is worn on the first paint:

```html

<script>try {
    if (localStorage.theme) document.documentElement.setAttribute("theme", localStorage.theme);
} catch (e) {
}</script>
```

Then the variables. Copy `wwwroot/css/variables.css` from this library into your project's `wwwroot/css` and link it before the component styles:

```html
<link rel="stylesheet" href="css/variables.css"/>
```

The file holds every variable the shared components read, at the standard value: sizes, fonts, motion and colours. Edit it in place, and add your own app's variables below. To skip the copy, link it from the library
(`_content/Noxy.NET.UI.Shared/css/variables.css`) and set only the variables you want to change in a stylesheet that loads after it.

### Colours

The colours are the last part of `variables.css`, the file under *Quick start*.

- **Pairs.** Every colour has a fill and an "on" colour: what is written or drawn inside it (`--color-warning` and `--color-on-warning`). A component never picks its own text colour, so contrast is decided in this one file. Use them as a pair in your own
  components too.
- **Handpicked per theme.** Each value is `light-dark(light, dark)`, or a single value where it reads well on both pages. To tune one theme, write the pair out: `--color-warning: light-dark(#D97F00, #F2A33A);`.
- **Shades.** Every status colour has `-hover`, `-active`, `-subtle` and `-border`, all worked out from its fill with `color-mix`. A new colour only needs a fill and an "on".
- **Surfaces.** `--color-surface` with `--color-on-surface`, and `--color-input` with `--color-on-input`, for the page and for inputs.
- **Themes.** `color-scheme` on the root decides which side of `light-dark()` shows: the system's by default, or `theme="Light"` and `theme="Dark"` on `<html>`. A project without a dark theme adds `:root { color-scheme: light; }` after the file and stays
  light.
- **Contrast.** Between a fill and its "on" colour it is at least 4.5:1, and between a fill and the page at least 3:1, in both themes (`disabled` is exempt). Keep to that when you change a colour.
- **Needs** a browser with `light-dark()` (Chromium 123, Safari 17.5, Firefox 120 or newer).

### Reference

**Colours**

| Variable                                                                                                    | Standard                                              | Used by                                                                                                                          |
|-------------------------------------------------------------------------------------------------------------|-------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| `--color-primary`                                                                                           | `light-dark(--color-light, --color-dark)`             | the page behind everything: buttons, inputs, table header, autosuggest dropdown                                                  |
| `--color-complementary`                                                                                     | `light-dark(--color-dark, --color-light)`             | text and outlines: buttons, collapsible, inputs, loader, message box, autosuggest                                                |
| `--color-default`                                                                                           | `#4D4AD3` (Dark: `#5F5CE6`)                           | `Button` and `MessageBox` with no status (`Default`)                                                                             |
| `--color-highlight`                                                                                         | `#0073EC`                                             | `Button` and `MessageBox` highlight status, focus rings and checked state in the inputs                                          |
| `--color-info`, `--color-success`, `--color-warning`, `--color-notice`, `--color-error`                     | `#289ED9`, `#23883D`, `#D97F00`, `#A948DD`, `#E42B21` | the status of `Button` and `MessageBox`                                                                                          |
| `--color-muted`                                                                                             | 62% of the text colour over the page                  | unchecked checkbox and radio                                                                                                     |
| `--color-disabled`                                                                                          | `#7F7F7F`                                             | disabled `Button`, checkbox, radio                                                                                               |
| `--color-offset-primary-sm`, `-lg`                                                                          | 4% and 20% of the text colour                         | table rows and header (`sm`), input borders and backgrounds (`sm`, `lg`)                                                         |
| `--color-offset-primary-md`, `--color-offset-complementary-*`                                               | see the block                                         | not read by the shared components                                                                                                |
| `--color-border`                                                                                            | `--color-complementary`                               | the border of text inputs, selects, the checkbox and radio boxes, the autosuggest and the colour swatch                          |
| `--color-border-focus`                                                                                      | `--color-highlight`                                   | the focus ring of `Button`                                                                                                       |
| `--color-control`                                                                                           | `--color-complementary`                               | not read by the shared components                                                                                                |
| `--color-light`, `--color-dark`                                                                             | `#FFFFFF`, `#121212`                                  | the palette the theme is built from                                                                                              |
| `--color-on-<status>` (`default`, `highlight`, `info`, `success`, `warning`, `notice`, `error`, `disabled`) | handpicked per theme                                  | what is written or drawn on that status colour: the text and loader of `Button`. Always use it as a pair with `--color-<status>` |
| `--color-<status>-hover`, `-active`, `-border`                                                              | worked out from `--color-<status>` with `color-mix`   | the hover, pressed and frame colours of `Button`. `-subtle` (a faint wash) is for backgrounds                                    |
| `--color-input`, `--color-on-input`                                                                         | handpicked per theme                                  | the background and text of text inputs                                                                                           |
| `--color-surface`, `--color-on-surface`                                                                     | handpicked per theme                                  | the page a control sits on and what is written on it                                                                             |

**Sizes**

| Variable                                                           | Standard                                                   | Used by                                                                                      |
|--------------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| `--size-text-body`                                                 | `1rem`                                                     | default size of `MaterialIcon`, input groups                                                 |
| `--size-text-label`, `-message`, `-h1` to `-h6`                    | `0.8125rem`, `0.875rem`, `2rem` down to `1.125rem`         | not read by the shared components                                                            |
| `--size-gap-xxxs`                                                  | `0.25rem`                                                  | select arrow spacing                                                                         |
| `--size-gap-xxs`                                                   | `0.5rem`                                                   | padding inside buttons, inputs, selects, autosuggest                                         |
| `--size-gap-xs`                                                    | `0.75rem`                                                  | collapsible, message box, table padding, pagination, option sets                             |
| `--size-gap-sm`                                                    | `1rem`                                                     | table column gap, input groups                                                               |
| `--size-gap-md`                                                    | `1.25rem`                                                  | `ButtonList`, frames, collapsible                                                            |
| `--size-gap-xl`, `--size-gap-xxl`                                  | `1.75rem`, `2rem`                                          | `WebForm` spacing, select padding                                                            |
| `--size-gap-tiny`                                                  | `2px`                                                      | checkbox and radio                                                                           |
| `--size-gap-lg`, `-xxxl`, `-huge`                                  | `1.5rem`, `2.25rem`, `2.5rem`                              | not read by the shared components                                                            |
| `--size-border-width-sm`, `-md`, `-lg`                             | `1px`, `2px`, `3px`                                        | inputs and groups (`sm`), message box, table header, collapsible (`md`), button frame (`lg`) |
| `--size-border-radius-sm`, `-md`                                   | `0.125rem`, `0.25rem`                                      | checkbox and radio (`sm`), buttons, inputs, message box (`md`)                               |
| `--size-border-radius-lg`                                          | `0.5rem`                                                   | not read by the shared components                                                            |
| `--size-box-shadow`, `--size-border-width`, `--size-border-radius` | `2px`, `--size-border-width-sm`, `--size-border-radius-md` | not read by the shared components                                                            |
| `--size-control-input`                                             | `--size-control-md`                                        | height of text inputs, selects, the collapsible header, buttons' minimum size                |
| `--size-control-choice`                                            | `--size-control-xxs`                                       | the box of checkboxes and radios, the colour swatch                                          |
| `--size-control-icon`                                              | `--size-icon-sm`                                           | the arrow of a select                                                                        |
| `--size-control-md`                                                | `2.625rem`                                                 | input groups, autosuggest                                                                    |
| `--size-control-xxs`                                               | `1.5rem`                                                   | radio                                                                                        |
| `--size-control-xs`, `-sm`, `-lg`, `-xl`, `-xxl`                   | `1.875rem` to `5.75rem`                                    | not read by the shared components                                                            |
| `--size-icon-xxs` to `--size-icon-xxl`                             | `1rem` to `2.5rem`                                         | not read by the shared components; for sizing `MaterialIcon` in an app                       |
| `--size-column`                                                    | `20rem`                                                    | `MagazineFrame` columns, `WebForm`                                                           |
| `--size-page-sm`                                                   | `22.5rem`                                                  | the sidebar column of `SidebarLeftFrame` and `SidebarRightFrame`                             |
| `--size-page-xs`, `-md`, `-lg`, `-xl`                              | `20rem`, `45rem`, `80rem`, `120rem`                        | not read by the shared components                                                            |

**Fonts and motion**

| Variable                                | Standard                                      | Used by                           |
|-----------------------------------------|-----------------------------------------------|-----------------------------------|
| `--font-body`                           | `--size-text-body` in `--font-family-primary` | `MessageBox`                      |
| `--font-button`                         | weight 500, `--size-text-body`                | `Button`                          |
| `--font-label`, `--font-family-primary` | see the block                                 | not read by the shared components |
| `--animation-length-short`              | `150ms`                                       | `Button`                          |
| `--animation-length-medium`             | `250ms`                                       | `Collapsible`                     |
| `--animation-length-long`               | `500ms`                                       | not read by the shared components |

**Set by the components, or per use**

| Variable                                                                                       | Where                  | Notes                                                                                                                                                           |
|------------------------------------------------------------------------------------------------|------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `--table-columns`                                                                              | `Table`                | The column template of the table, for example `repeat(auto-fit, minmax(6.25rem, 1fr))` or `2rem 2rem minmax(10rem, 3fr) 8rem`. Set it on the table or a parent. |
| `--center-width`                                                                               | frames                 | Set from the `ContentWidth` parameter of `MagazineFrame`, `SidebarLeftFrame` and `SidebarRightFrame`.                                                           |
| `--color-button`, `--button-background-color`, `--button-backdrop-color`, `--color-messagebox` | `Button`, `MessageBox` | Set by the component from its status. Don't define these yourself.                                                                                              |

### Tuning

- **Density:** change the `--size-control-*` and `--size-gap-*` values. `--size-control-input` decides how tall every input and button is, and `--size-control-md` is what it normally points at, so one change there resizes them all.
- **Brand:** `--color-highlight` is the accent (focus, checked, highlight status) and `--color-default` is the default button. The other status colours only matter where a status is shown.
- **Dark mode:** hand-pick each colour and its `--color-on-*` for both themes, for example `--color-warning: light-dark(#D97F00, #F2A33A)`, and set `color-scheme` on the root (`light dark` by default, `light` or `dark` when the theme is forced) so
  `light-dark()` picks the right side. Keep 4.5:1 between a colour and its "on" colour, and 3:1 between the colour and the page.
- **Text size:** `rem` follows the user's setting, so don't set the root font size in `px`. `--size-text-body: 1rem` is 16px at the browser's default.
- **Rounded or square:** `--size-border-radius-md` is used by buttons, inputs and the message box; set it to `0` for square corners.
- **Without the variables:** every variable a component reads has a built-in fallback equal to the standard value in the tables above, so a project that defines none of them gets the same sizes and colours as the block under *Quick start* (in the Light theme).
  Define a variable only for what you want to change.

## Wizard

A list of numbered steps beside the content of the current step. The progress lives in a `WizardContext`, which you create and keep in your own component, so the content of a step can complete or fail it.

```razor
<Wizard Context="_context" Label="Set up" OnWizardComplete="FinishAsync">
    <WizardStep Title="Account" Subtitle="Who you are">
        <FormAccount OnValid="_context.CompleteStep" OnInvalid="_context.FailStep"/>
    </WizardStep>
    <WizardStep Title="Settings">
        ...
    </WizardStep>
</Wizard>

@code {
    private readonly WizardContext _context = new();
}
```

- **Steps.** A step is completed (before the progress), active (the one still to do) or pending (not reachable yet). The current step has a ring and `aria-current="step"`. Each step is a button in an ordered list, so the keyboard and screen readers can use it.
  Pending steps are disabled, unless the step has `IgnoreCompletion`.
- **`CompleteStep()`** moves the progress to the next step and shows it. On the last step it raises `OnWizardComplete`. Completing an earlier step again moves the progress back to the step after it, so the later steps are checked again.
- **`FailStep()`** moves the progress back to the current step.
- **`GoToStep(index, ignoreProgress)`** shows a step. Without `ignoreProgress` only the steps the user has reached can be shown.
- **Events** on the context: `StepChanged`, `WizardCompleted` and `StateChanged` (anything the steps show changed).
- **Order.** Steps register in the order they are rendered, so a step that appears later in the middle takes that place in the list.
- **Text for assistive technology.** `Label` names the list on `Wizard`, and `CompletedLabel` (default `Completed`) is said after a completed step. Set both for a language other than English.
- **Colours.** Pending steps use the surface and border colours, active steps `--color-highlight` with `--color-on-highlight`, and completed steps `--color-success` with `--color-on-success`. The connecting lines use `--color-offset-primary-lg`.

This replaces the separate `Noxy.NET.UI.Wizard` package: reference `Noxy.NET.UI.Shared` instead. The components are now in `Noxy.NET.UI.Components.Containers`, and the `WizardStep` parameter `IgnoreCompletion` is a `bool` instead of a `bool?`.

## Table

### Structure

```razor
<Table RowCount="List.Count">
    <TableHeader>
        <TableCell>Name</TableCell>
        <TableHeaderCellSort Direction="..." OnClick="...">Title</TableHeaderCellSort>
    </TableHeader>
    <TableBody Loading="IsLoading">
        @foreach (var (index, row) in List.Index())
        {
            <TableRow @key="row.ID" Index="index">...</TableRow>
        }
    </TableBody>
</Table>
```

`TableBodyVirtualize` replaces `TableBody` when a list is long enough that only the rows in view should be drawn: it takes `List`, `ItemSize` and a row fragment receiving `(Row, Index)`. The table scrolls, and the header stays at the top.

The columns come from `--table-columns` (or the grid the table is placed in); the header, the body and the rows are subgrids of it.

### Accessibility

The components render the roles of the table, its header row, the body (`rowgroup`) and the rows, with `aria-rowcount` (from `RowCount`) and `aria-rowindex` (from `Index`), so a virtualized table reports its full size. `TableCell` gets its role from where it
sits: a column header inside `TableHeader`, a cell inside `TableRow`. Set `Kind="TableCellKindEnum.RowHeader"` on the cell that names the row. `TableHeaderCellSort` is a column header.

`SortableTable` rows can be reordered with the keyboard: focus a row and press Alt with the up or down arrow.

## Containers

- `EmptyState`: a centred box that fills what is left of its container, for a list or page with nothing in it. The caller passes the `Graphic`, `Title`, `Description` and what can be done about it (`ChildContent`).
- `CardGrid`: a grid that makes as many columns as fit (`--size-column` wide at least); each card is as high as what it holds.
- `PageContent`: the body of a fullscreen page, whose own padding is none: its children are stacked with room around them and it scrolls when too tall.
