// Rewrites relative links to .md files into site URLs, so docs/ links work on GitHub and on the site.
import path from 'node:path';

const slugSegment = (segment: string) =>
  segment
    .toLowerCase()
    .replace(/[^\p{L}\p{N}\s_-]/gu, '')
    .replace(/\s/g, '-');

function siteUrl(docsDir: string, source: string, link: string, base: string): string | undefined {
  const match = link.match(/^([^:#?\s]+\.md)(#\S*)?$/);
  if (!match) return undefined;
  const relative = path.relative(docsDir, path.resolve(path.dirname(source), match[1]));
  if (relative.startsWith('..') || path.isAbsolute(relative)) return undefined;
  const parts = relative.replace(/\.md$/, '').split(path.sep).map(slugSegment);
  if (parts.at(-1) === 'index') parts.pop();
  const slug = parts.join('/');
  return `${base}/${slug}${slug ? '/' : ''}${match[2] ?? ''}`;
}

/** Rewrites `](x.md)` links in Markdown text. Fenced code blocks stay unchanged. */
export function rewriteMdLinks(markdown: string, source: string, docsDir: string, base = ''): string {
  return markdown
    .split(/(^```[\s\S]*?^```)/m)
    .map((part) =>
      part.startsWith('```')
        ? part
        : part.replace(/\]\(([^)\s]+)\)/g, (whole, link) => {
            const url = siteUrl(docsDir, source, link, base);
            return url ? `](${url})` : whole;
          }),
    )
    .join('');
}
