# Noxy.NET.UI.Common

## Styling

### How it works

The components carry no colours or sizes of their own. They read CSS variables, almost always with a built-in fallback (`var(--size-gap-xs, 0.75rem)`), so a page works without defining anything, but defining the variables once gives every component the same
look.

- Define the variables on `:root` in a stylesheet that loads before the component styles. Copy the block under *Quick start* into your project's `variables.css` and edit it.
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

Then the variables:

```css
/* Sizes: text, gaps, borders, controls, icons, page widths. */
:root {
    --size-text-label: 0.8125rem;
    --size-text-message: 0.875rem;
    --size-text-body: 1rem;
    --size-text-h1: 2rem;
    --size-text-h2: 1.75rem;
    --size-text-h3: 1.5rem;
    --size-text-h4: 1.375rem;
    --size-text-h5: 1.25rem;
    --size-text-h6: 1.125rem;

    --size-gap-tiny: 2px;
    --size-gap-xxxs: 0.25rem;
    --size-gap-xxs: 0.5rem;
    --size-gap-xs: 0.75rem;
    --size-gap-sm: 1rem;
    --size-gap-md: 1.25rem;
    --size-gap-lg: 1.5rem;
    --size-gap-xl: 1.75rem;
    --size-gap-xxl: 2rem;
    --size-gap-xxxl: 2.25rem;
    --size-gap-huge: 2.5rem;

    --size-border-width-sm: 1px;
    --size-border-width-md: 2px;
    --size-border-width-lg: 3px;
    --size-border-radius-sm: 0.125rem;
    --size-border-radius-md: 0.25rem;
    --size-border-radius-lg: 0.5rem;
    --size-box-shadow: 2px;

    --size-control-xxs: 1.5rem;
    --size-control-xs: 1.875rem;
    --size-control-sm: 2.25rem;
    --size-control-md: 2.625rem;
    --size-control-lg: 3rem;
    --size-control-xl: 4rem;
    --size-control-xxl: 5.75rem;

    --size-icon-xxs: 1rem;
    --size-icon-xs: 1.25rem;
    --size-icon-sm: 1.5rem;
    --size-icon-md: 1.75rem;
    --size-icon-lg: 2rem;
    --size-icon-xl: 2.25rem;
    --size-icon-xxl: 2.5rem;

    --size-page-xs: 20rem;
    --size-page-sm: 22.5rem;
    --size-page-md: 45rem;
    --size-page-lg: 80rem;
    --size-page-xl: 120rem;
    --size-column: 20rem;
}

/* What the form controls and a few components read, built from the sizes above. */
:root {
    --size-border-width: var(--size-border-width-sm);
    --size-border-radius: var(--size-border-radius-md);
    --size-control-input: var(--size-control-md);
    --size-control-choice: var(--size-control-xxs);
    --size-control-icon: var(--size-icon-sm);
}

:root {
    --animation-length-short: 150ms;
    --animation-length-medium: 250ms;
    --animation-length-long: 500ms;
}

:root {
    --font-family-primary: "Segoe UI", "Roboto", sans-serif;
    --font-label: normal normal 400 normal var(--size-text-label) var(--font-family-primary);
    --font-body: normal normal 400 normal var(--size-text-body) var(--font-family-primary);
    --font-button: normal normal 500 normal var(--size-text-body) var(--font-family-primary);
}

/* The palette and the status colours. */
:root {
    --color-light: #FFFFFF;
    --color-dark: #000000;
    --color-default: #4D4AD3;
    --color-muted: #767676;
    --color-highlight: #0073EC;
    --color-info: #289ED9;
    --color-success: #23883D;
    --color-warning: #D97F00;
    --color-notice: #A948DD;
    --color-error: #E42B21;
    --color-disabled: #7F7F7F;
}

/* The two sides of a theme: the page and what is written on it. Light is the default, Dark swaps them. */
:root,
:root[theme="Light"] {
    --color-primary: var(--color-light);
    --color-complementary: var(--color-dark);
}

:root[theme="Dark"] {
    --color-primary: var(--color-dark);
    --color-complementary: var(--color-light);
}

@media (prefers-color-scheme: dark) {
    :root:not([theme]) {
        --color-primary: var(--color-dark);
        --color-complementary: var(--color-light);
    }
}

/* Faint tints of the text colour over the page (and of the page over the text), for borders and stripes. */
:root {
    --color-offset-primary-sm: color-mix(in srgb, transparent 96%, var(--color-complementary));
    --color-offset-primary-md: color-mix(in srgb, transparent 90%, var(--color-complementary));
    --color-offset-primary-lg: color-mix(in srgb, transparent 80%, var(--color-complementary));
    --color-offset-complementary-sm: color-mix(in srgb, transparent 90%, var(--color-primary));
    --color-offset-complementary-md: color-mix(in srgb, transparent 80%, var(--color-primary));
    --color-offset-complementary-lg: color-mix(in srgb, transparent 50%, var(--color-primary));
}

:root {
    --color-border: var(--color-complementary);
    --color-border-focus: var(--color-highlight);
    --color-control: var(--color-complementary);
}
```

### Reference

**Colours**

| Variable                                                                                | Standard                                              | Used by                                                                                                 |
|-----------------------------------------------------------------------------------------|-------------------------------------------------------|---------------------------------------------------------------------------------------------------------|
| `--color-primary`                                                                       | `--color-light` (Dark: `--color-dark`)                | the page behind everything: buttons, inputs, table header, autosuggest dropdown                         |
| `--color-complementary`                                                                 | `--color-dark` (Dark: `--color-light`)                | text and outlines: buttons, collapsible, inputs, loader, message box, autosuggest                       |
| `--color-default`                                                                       | `#4D4AD3`                                             | `Button` and `MessageBox` with no status (`Default`)                                                    |
| `--color-highlight`                                                                     | `#0073EC`                                             | `Button` and `MessageBox` highlight status, focus rings and checked state in the inputs                 |
| `--color-info`, `--color-success`, `--color-warning`, `--color-notice`, `--color-error` | `#289ED9`, `#23883D`, `#D97F00`, `#A948DD`, `#E42B21` | the status of `Button` and `MessageBox`                                                                 |
| `--color-muted`                                                                         | `#767676`                                             | unchecked checkbox and radio                                                                            |
| `--color-disabled`                                                                      | `#7F7F7F`                                             | disabled `Button`, checkbox, radio                                                                      |
| `--color-offset-primary-sm`, `-lg`                                                      | 4% and 20% of the text colour                         | table rows and header (`sm`), input borders and backgrounds (`sm`, `lg`)                                |
| `--color-offset-primary-md`, `--color-offset-complementary-*`                           | see the block                                         | not read by the shared components                                                                       |
| `--color-border`                                                                        | `--color-complementary`                               | the border of text inputs, selects, the checkbox and radio boxes, the autosuggest and the colour swatch |
| `--color-border-focus`                                                                  | `--color-highlight`                                   | the focus ring of `Button`                                                                              |
| `--color-control`                                                                       | `--color-complementary`                               | not read by the shared components                                                                       |
| `--color-light`, `--color-dark`                                                         | `#FFFFFF`, `#000000`                                  | the palette the theme is built from                                                                     |

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
- **Dark mode:** only `--color-primary` and `--color-complementary` swap. Choose the status colours so they stay readable on both backgrounds, and keep the text colours at a 4.5:1 contrast on the page colour.
- **Text size:** `rem` follows the user's setting, so don't set the root font size in `px`. `--size-text-body: 1rem` is 16px at the browser's default.
- **Rounded or square:** `--size-border-radius-md` is used by buttons, inputs and the message box; set it to `0` for square corners.
- **Without the variables:** every variable a component reads has a built-in fallback equal to the standard value in the tables above, so a project that defines none of them gets the same sizes and colours as the block under *Quick start* (in the Light theme).
  Define a variable only for what you want to change.

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
