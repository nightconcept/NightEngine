// Verify the installed MCP server can initialize and expose its development tools.
import assert from 'node:assert/strict';
import {createRequire} from 'node:module';
import {resolve} from 'node:path';

const root = resolve(process.argv[2] ?? '.tools/lovepilot');
const require = createRequire(resolve(root, 'package.json'));
const {Client} = require('@modelcontextprotocol/sdk/client/index.js');
const {StdioClientTransport} = require('@modelcontextprotocol/sdk/client/stdio.js');
const client = new Client({name: 'nightengine-check', version: '0.1.1'});
try {
  await client.connect(new StdioClientTransport({command: 'node', args: [resolve(root, 'build/index.js')]}));
  const {tools} = await client.listTools();
  assert(tools.some(tool => tool.name === 'run_lua'), 'run_lua tool is missing');
  console.log(`LovePilot MCP: ${tools.length} tools available`);
} finally {
  await client.close();
}
