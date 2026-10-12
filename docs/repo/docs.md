---
sidebar:
  order: 3
---

# Documentation

`docs/` is the single source of truth for agents and people. The site in `site/`
renders it. `AGENTS.md` and `README.md` link into it. Skills in `skills/` link into
it and do not copy it.

## Where a page goes

| Folder | Reader | Holds |
| --- | --- | --- |
| `docs/` root | Everyone | `index.md` (portal), `vision.md` (charter), `roadmap.md` (gaps) |
| `concepts/` | Everyone | How the engine works: architecture, contract, runtimes, platforms, parity |
| `games/` | Game developers and their agents | How a game uses the engine: consume, layout, configuration, commands, skills |
| `runtimes/` | Game developers | One page for each runtime: API, host, builds |
| `reference/` | Game developers | Detailed API and tool reference |
| `repo/` | Engine contributors | Rules, workflow and these documentation rules |
| `exec-plans/` | Engine contributors | Multi-step plans, active and completed |

Add a page to the folder for its reader. The site sidebar reads each folder, so a new
page needs no site change. A new folder needs one sidebar entry in `site/astro.config.mjs`.

## Page format

- Start with one `# Title`. The site uses it as the page title.
- Optional front matter sets the sidebar position: `sidebar: { order: <n> }`.
- Use relative links to `.md` files, so they work on GitHub and on the site.
- Write in short, plain sentences. Use the `simple-english` skill rules.
- Show a command as a code block that a reader can copy.
- Do not write history ("was", "changed from"). Git keeps the history. The
  `CHANGELOG.md` records releases.

## Keep the docs true

Change the docs in the same commit as the code. These pages change most often:

| When you change | Update |
| --- | --- |
| A feature in one runtime | `concepts/parity.md` |
| A builder or a release workflow | `concepts/platforms.md` |
| A `[tool.nightengine]` key | `games/configuration.md` |
| A standard recipe name | `games/commands.md` and the skills |
| A contract rule or fixture | `concepts/contract.md` and `CHANGELOG.md` |
| A roadmap item starts or ends | `roadmap.md` and `exec-plans/active/README.md` |
| A game workflow | The matching skill in `skills/` |

`AGENTS.md` has 100 lines or fewer. Add detail to a spoke page, then link it from the hub.

## Build the site

```sh
just docs        # Build into site/dist.
just docs-dev    # Serve with live reload.
```

A push to `main` that changes `docs/` or `site/` deploys the site to GitHub Pages.
