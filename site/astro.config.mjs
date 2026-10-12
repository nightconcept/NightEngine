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
      description: 'Deterministic game engine for Python/Pyxel and Lua/LÖVE',
      social: [
        {
          icon: 'github',
          label: 'GitHub',
          href: 'https://github.com/nightconcept/NightEngine',
        },
      ],
      customCss: ['./src/styles/custom.css'],
      sidebar: [
        {
          label: 'Overview',
          slug: 'index',
        },
        {
          label: 'Core Concepts',
          items: [
            { label: 'Architecture', slug: 'architecture' },
            { label: 'Compatibility Contract', slug: 'compatibility' },
          ],
        },
        {
          label: 'Implementations',
          items: [
            { label: 'Python / Pyxel', slug: 'python-pyxel' },
            { label: 'Lua / LÖVE', slug: 'lua-love' },
          ],
        },
        {
          label: 'APIs & Observability',
          items: [
            { label: 'Nightrunner APIs', slug: 'nightrunner-api' },
            { label: 'Observability & Dev Bridge', slug: 'observability' },
          ],
        },
        {
          label: 'Development & Workflow',
          items: [
            { label: 'Engine Rules', slug: 'repo/engine-rules' },
            { label: 'Workflow & Releases', slug: 'repo/workflow' },
            { label: 'Active Execution Plans', slug: 'exec-plans/active/readme' },
          ],
        },
      ],
    }),
    astroMermaid(),
  ],
});
