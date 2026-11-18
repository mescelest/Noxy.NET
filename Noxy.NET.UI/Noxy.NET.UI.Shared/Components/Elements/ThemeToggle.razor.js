const attribute = "theme";
const key = "theme";

// The theme the page already wears, else the one the user chose before, else the one the system asks for.
export function GetTheme() {
    const current = document.documentElement.getAttribute(attribute) || read();
    if (current) return current;

    if (window.matchMedia("(prefers-color-scheme: dark)").matches) return "Dark";
    if (window.matchMedia("(prefers-color-scheme: light)").matches) return "Light";

    return null;
}

// Wears the theme without remembering it, so a theme taken from the system is not stored as a choice.
export function ApplyTheme(theme) {
    if (theme) document.documentElement.setAttribute(attribute, theme);
    else document.documentElement.removeAttribute(attribute);
}

export function SetTheme(theme) {
    ApplyTheme(theme);
    write(theme);
}

function read() {
    try {
        return localStorage.getItem(key);
    } catch {
        return null;
    }
}

function write(theme) {
    try {
        if (theme) localStorage.setItem(key, theme);
        else localStorage.removeItem(key);
    } catch {
        // Storage can be blocked; the theme is still worn for this visit.
    }
}
