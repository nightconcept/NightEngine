import { installBridge } from './pointer.mjs';

// The build writes the game's logical size into <body data-width data-height>.
const debug = new URLSearchParams(location.search).get('app') === 'debug';
const [width, height] = debug ? [256, 144] : [Number(document.body.dataset.width), Number(document.body.dataset.height)];
const stage = document.getElementById('stage');
const status = document.getElementById('status');
if (debug) document.getElementById('help').textContent =
  'Touch, drag, or click to see pointer IDs, phases, and logical coordinates. Try several fingers. Resize to clear contacts.';
const fit = () => {
  const availableHeight = Math.max(120, innerHeight - 170);
  const gameWidth = Math.min(innerWidth, availableHeight * width / height);
  const gameHeight = gameWidth * height / width;
  stage.style.width = `${gameWidth}px`;
  stage.style.height = `${gameHeight}px`;
  const fullscreenWidth = Math.min(innerWidth, innerHeight * width / height);
  stage.style.setProperty('--game-width', `${fullscreenWidth}px`);
  stage.style.setProperty('--game-height', `${fullscreenWidth * height / width}px`);
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
      installBridge(canvas, width, height, window, () => window.pyxelContext.initialized);
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
  status.textContent = 'Running. Touch, mouse, and keyboard input are enabled.';
} catch (error) {
  status.textContent = `Could not start: ${error.message}. Check network access and reload.`;
}
