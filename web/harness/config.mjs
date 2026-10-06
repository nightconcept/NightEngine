// A Playwright config for a nightengine game: the engine's own browser checks plus the game's.
//
//   // playwright.config.mjs in the game
//   import { harnessConfig } from './nightengine/web/harness/config.mjs';
//   export default harnessConfig({build: 'dist/web-offline', gameTests: 'tests/browser'});
import { defineConfig } from '@playwright/test';
import { existsSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const macChromium = '/Applications/Chromium.app/Contents/MacOS/Chromium';

export function harnessConfig({build, gameTests, port = 8765, viewport = {width: 1000, height: 800}}) {
  const executablePath = process.env.NIGHTENGINE_BROWSER ??
    (process.platform === 'darwin' && existsSync(macChromium) ? macChromium : undefined);
  const engineTests = join(dirname(fileURLToPath(import.meta.url)), 'tests');
  return defineConfig({
    timeout: 180_000,
    expect: {timeout: 15_000},
    workers: 1,
    outputDir: 'test-results',
    projects: [
      {name: 'engine', testDir: engineTests},
      ...(gameTests ? [{name: 'game', testDir: gameTests}] : []),
    ],
    use: {
      baseURL: `http://127.0.0.1:${port}`,
      viewport,
      hasTouch: true,
      trace: 'retain-on-failure',
      screenshot: 'only-on-failure',
      launchOptions: {
        executablePath,
        args: ['--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required'],
      },
    },
    webServer: {
      command: `python3 -m http.server ${port} --bind 127.0.0.1 --directory ${build}`,
      url: `http://127.0.0.1:${port}`,
      timeout: 30_000,
      reuseExistingServer: !process.env.CI,
      stderr: 'ignore',  // http.server logs every request.
    },
  });
}
