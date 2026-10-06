import { installBridge } from './pointer.mjs';

// The build writes the game's logical size into <body data-width data-height>. The game may change it while it runs
// (bridge.resize), and the page then refits and updates the data attributes.
const query = new URLSearchParams(location.search);
const debug = query.get('app') === 'debug';
// ?shell=app: the page is the whole app (the APK). No header or notes, and the game fills the screen.
const shell = query.get('shell') === 'app';
if (shell) document.body.classList.add('app');
let [width, height] = debug ? [256, 144] : [Number(document.body.dataset.width), Number(document.body.dataset.height)];
const stage = document.getElementById('stage');
const status = document.getElementById('status');
if (debug) document.getElementById('help').textContent =
  'Touch, drag, or click to see pointer IDs, phases, and logical coordinates. Try several fingers. Resize to clear contacts.';
const box = document.getElementById('pyxel-screen');
// Pyxel never draws below 1x (its scale is at least 1), so a canvas smaller than the game would crop it. When the
// shown size is smaller, Pyxel's box gets the game's size and a CSS transform shrinks it. The pointer bridge maps
// from the canvas's on-screen rectangle, which includes the transform.
const fit = () => {
  const availableHeight = shell ? innerHeight : Math.max(120, innerHeight - 170);
  const gameWidth = Math.min(innerWidth, availableHeight * width / height);
  const gameHeight = gameWidth * height / width;
  stage.style.width = `${gameWidth}px`;
  stage.style.height = `${gameHeight}px`;
  if (shell) stage.style.marginTop = `${Math.max(0, (innerHeight - gameHeight) / 2)}px`;
  const full = document.fullscreenElement === stage;
  const shownWidth = full ? Math.min(innerWidth, innerHeight * width / height) : gameWidth;
  const shownHeight = shownWidth * height / width;
  const shrink = Math.min(1, shownWidth / width);
  Object.assign(box.style, {
    left: `${full ? (innerWidth - shownWidth) / 2 : 0}px`,
    top: `${full ? (innerHeight - shownHeight) / 2 : 0}px`,
    width: `${shownWidth / shrink}px`,
    height: `${shownHeight / shrink}px`,
    transform: `scale(${shrink})`,
    transformOrigin: '0 0',
  });
};
fit();
window.addEventListener('resize', fit);
document.addEventListener('fullscreenchange', fit);
document.getElementById('fullscreen').addEventListener('click', async () => {
  try {
    if (document.fullscreenElement) await document.exitFullscreen();
    else await stage.requestFullscreen();
  } catch (error) { status.textContent = `Fullscreen unavailable: ${error.message}`; }
});

try {
  const response = await fetch(debug ? 'debug.pyxapp' : 'game.pyxapp');
  if (!response.ok) throw new Error(`Game download failed: HTTP ${response.status}`);
  const bytes = new Uint8Array(await response.arrayBuffer());
  let binary = '';
  for (const byte of bytes) binary += String.fromCharCode(byte);
  // Canvas creation happens during the upstream launch. Attach before Python starts.
  const observer = new MutationObserver(() => {
    const canvas = document.getElementById('canvas');
    if (canvas) {
      // SDL sizes its window (Pyxel's drawing surface) from canvas.getBoundingClientRect(), which includes the
      // shrink transform from fit(). Report the layout size instead, so Pyxel keeps the game's full pixels.
      // The pointer bridge measures the real on-screen rectangle (pointer.mjs, screenRect).
      canvas.getBoundingClientRect = function () {
        const shown = Element.prototype.getBoundingClientRect.call(this);
        return new DOMRect(shown.x, shown.y, this.offsetWidth, this.offsetHeight);
      };
      installBridge(canvas, width, height, window, () => window.pyxelContext.initialized, (w, h) => {
        [width, height] = [w, h];
        Object.assign(document.body.dataset, {width: String(w), height: String(h)});
        fit();
        window.dispatchEvent(new Event('resize'));  // SDL reads the new canvas size on a resize event.
      });
      observer.disconnect();
    }
  });
  observer.observe(stage, {childList: true, subtree: true});
  // Pyxel 2.9.9 registers page-wide pinch handlers synchronously at launch.
  // Move those exact handlers to the stage and preserve the accessible page viewport.
  const viewport = document.querySelector('meta[name="viewport"]');
  const viewportContent = viewport.content;
  const addEventListener = document.addEventListener;
  const pinchHandlers = [];
  let launching;
  try {
    document.addEventListener = function(type, handler, options) {
      if (type === 'touchstart' || type === 'touchmove') pinchHandlers.push([type, handler, options]);
      return addEventListener.call(this, type, handler, options);
    };
    launching = window.launchPyxel({command: 'play', name: debug ? 'debug.pyxapp' : 'game.pyxapp',
      base64: btoa(binary), gamepad: 'disabled'});
  } finally {
    document.addEventListener = addEventListener;
    viewport.content = viewportContent;
    for (const [type, handler, options] of pinchHandlers) {
      document.removeEventListener(type, handler, options);
      stage.addEventListener(type, handler, options);
    }
  }
  await launching;
  status.textContent = 'Running.';
} catch (error) {
  status.textContent = `Could not start: ${error.message}. Check network access and reload.`;
}
