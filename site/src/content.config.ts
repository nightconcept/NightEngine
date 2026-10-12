import { defineCollection } from 'astro:content';
import { glob } from 'astro/loaders';
import { docsSchema } from '@astrojs/starlight/schema';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

/**
 * Loads docs directly from the repository's root docs/ directory.
 * Automatically infers page titles from the first Markdown `# Heading`
 * if YAML frontmatter does not define a title.
 */
function externalDocsLoader(relativeDocsDir: string) {
  const globLoader = glob({
    base: relativeDocsDir,
    pattern: '**/[^_]*.{md,mdx}',
  });

  return {
    name: 'nightengine-docs-loader',
    async load(context: any) {
      const origParseData = context.parseData;
      const rootDir = fileURLToPath(context.config.root);

      context.parseData = async (entry: any) => {
        let title = entry.data?.title;

        // If title is missing from frontmatter, read from first # Heading
        if (!title && entry.filePath) {
          try {
            const fullPath = path.isAbsolute(entry.filePath)
              ? entry.filePath
              : path.join(rootDir, entry.filePath);
            const content = fs.readFileSync(fullPath, 'utf-8');
            const match = content.match(/^#\s+(.+)$/m);
            if (match) {
              title = match[1].trim();
            }
          } catch {
            // Ignore read errors and fall through
          }
        }

        // Fallback: humanize the slug ID if still no title
        if (!title) {
          const raw = entry.id.split('/').pop() || 'index';
          title = raw.replace(/[-_]/g, ' ');
          title = title.charAt(0).toUpperCase() + title.slice(1);
        }

        entry.data = { ...entry.data, title };
        return origParseData(entry);
      };

      return globLoader.load(context);
    },
  };
}

export const collections = {
  docs: defineCollection({
    loader: externalDocsLoader('../docs'),
    schema: docsSchema(),
  }),
};
