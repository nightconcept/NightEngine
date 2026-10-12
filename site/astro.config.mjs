import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import astroMermaid from 'astro-mermaid';

// Automatically detect GitHub Pages repository for base path
const isGithubActions = process.env.GITHUB_ACTIONS === 'true';
const repoName = process.env.GITHUB_REPOSITORY ? process.env.GITHUB_REPOSITORY.split('/')[1] : '';
const base = isGithubActions && repoName ? `/${repoName}` : undefined;

export default defineConfig({
  site: 'https://nightconcept.github.io',
  base,
  integrations: [
    starlight({
      title: 'NightEngine Docs',
      description: 'Nightcircle game engine for Python/Pyxel, Lua/LÖVE and C#/MonoGame',
      social: [
        {
          icon: 'github',
          label: 'GitHub',
          href: 'https://github.com/nightconcept/NightEngine',
        },
      ],
      customCss: ['./src/styles/custom.css'],
      // Each group reads one docs/ folder. A new page needs no change here.
      // Order pages with `sidebar: { order: n }` front matter. See docs/repo/docs.md.
      sidebar: [
        { label: 'Overview', slug: 'index' },
        { label: 'Vision', slug: 'vision' },
        { label: 'Roadmap', slug: 'roadmap' },
        { label: 'Concepts', items: [{ autogenerate: { directory: 'concepts' } }] },
        { label: 'Making games', items: [{ autogenerate: { directory: 'games' } }] },
        { label: 'Runtimes', items: [{ autogenerate: { directory: 'runtimes' } }] },
        { label: 'Reference', items: [{ autogenerate: { directory: 'reference' } }] },
        { label: 'Engine development', items: [{ autogenerate: { directory: 'repo' } }] },
        { label: 'Execution plans', collapsed: true, items: [{ autogenerate: { directory: 'exec-plans' } }] },
      ],
    }),
    astroMermaid(),
  ],
});
