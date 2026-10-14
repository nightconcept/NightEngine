import test from 'node:test';
import assert from 'node:assert/strict';
import { handle, SENTINEL, TextQueue } from '../static/text.mjs';

test('the queue joins pushes in order and drains', () => {
  const q = new TextQueue();
  q.push('he');
  q.push('');
  q.push('y\n');
  assert.equal(q.take(), 'hey\n');
  assert.equal(q.take(), '');
});

test('typed characters, Backspace and Enter from beforeinput', () => {
  assert.deepEqual(handle('beforeinput', {inputType: 'insertText', data: 'a'}), {text: 'a', prevent: true, reset: false});
  assert.equal(handle('beforeinput', {inputType: 'deleteContentBackward'}).text, '\b');
  assert.equal(handle('beforeinput', {inputType: 'insertLineBreak'}).text, '\n');
  assert.equal(handle('beforeinput', {inputType: 'insertText', data: 'a\r\nb'}).text, 'ab');
  assert.equal(handle('beforeinput', {inputType: 'insertCompositionText', data: 'x', isComposing: true}).text, '');
});

test('a phone keyboard: a composed word arrives when it ends', () => {
  assert.equal(handle('input', {isComposing: true}, SENTINEL + 'hel').text, '');
  assert.deepEqual(handle('compositionend', {data: 'hello'}), {text: 'hello', prevent: false, reset: true});
});

test('an input event catches what beforeinput did not', () => {
  assert.deepEqual(handle('input', {}, SENTINEL + 'ok'), {text: 'ok', prevent: false, reset: true});
  assert.deepEqual(handle('input', {}, ''), {text: '\b', prevent: false, reset: true});
  assert.equal(handle('input', {}, SENTINEL).text, '');
});

test('Enter and Escape keys', () => {
  assert.deepEqual(handle('keydown', {key: 'Enter'}), {text: '\n', prevent: true, reset: false});
  assert.equal(handle('keydown', {key: 'Escape'}).text, '\x1b');
  assert.equal(handle('keydown', {key: 'a'}).text, '');
});
