# Project instructions

## Where superpowers files go

These override the superpowers skills' defaults. Never create `docs/superpowers/`.

- Specs: `docs/specs/<project>/YYYY-MM-DD-<topic>-design.md`, committed, with `<project>` one of `mediaplayer`, `entitymanagement` or `shared` (the shared libraries, such as `Noxy.NET.UI.Shared`). `docs/UI-GUIDE.md` stays in the `docs/` root, not in `specs/`.
- Plans: `docs/temp/plans/YYYY-MM-DD-<feature-name>.md`, committed until the work is done.
- Handoffs: `docs/temp/handoffs/HANDOFF-YYYY-MM-DD-<topic>.md`.
- Subagent-driven-development ledgers: `docs/temp/sdd/<plan-basename>/` (git-ignored).
- Brainstorm mockups: `docs/temp/brainstorm/` (git-ignored).
- Other work ledgers: `docs/temp/ledger-*.md` (git-ignored).

The superpowers scripts hardcode `.superpowers/sdd/` and `.superpowers/brainstorm/`. If a skill creates a `.superpowers/` folder anyway, move its contents into `docs/temp/` and delete `.superpowers/`.
