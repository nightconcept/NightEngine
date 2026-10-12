import {createRequire} from 'node:module';
import {resolve} from 'node:path';
import net from 'node:net';
const require=createRequire(resolve(process.argv[2]??'.tools/monogame-mcp','package.json'));
const {Server}=require('@modelcontextprotocol/sdk/server/index.js');
const {StdioServerTransport}=require('@modelcontextprotocol/sdk/server/stdio.js');
const {ListToolsRequestSchema,CallToolRequestSchema}=require('@modelcontextprotocol/sdk/types.js');
let id=0;
function request(command,args={}) {
 return new Promise((resolve,reject)=>{
  const socket=net.createConnection({host:'127.0.0.1',port:12346}); let buffer='';
  const timer=setTimeout(()=>{socket.destroy();reject(new Error('Start a Debug MonoGame build with --dev. Bridge timed out.'));},10000);
  socket.on('connect',()=>socket.write(JSON.stringify({command,...args,_reqId:++id})+'\n'));
  socket.on('error',error=>{clearTimeout(timer);reject(error);});
  socket.on('data',chunk=>{buffer+=chunk;if(buffer.includes('\n')) {clearTimeout(timer);socket.end();try {const result=JSON.parse(buffer.split('\n')[0]);if(result.error)reject(new Error(result.error));else resolve(result);}catch(error){reject(error);}}});
 });
}
const schema=(properties={},required=[])=>({type:'object',properties,required,additionalProperties:false});
const integer=(minimum,maximum)=>({type:'integer',minimum,maximum});
const tools=[
 {name:'get_objects',description:'Read the game scene, frame, input, cues and deterministic snapshot.',inputSchema:schema()},
 {name:'get_screenshot',description:'Capture the actual MonoGame logical framebuffer as PNG.',inputSchema:schema()},
 {name:'send_input',description:'Advance paused simulation through Game.step with held button bits and optional pointers.',inputSchema:schema({code:integer(0,268435455),frames:integer(1,600),pointers:{type:'array',items:schema({id:integer(-1,2147483647),x:integer(-1000,4096),y:integer(-1000,4096),startX:integer(-1000,4096),startY:integer(-1000,4096),phase:{enum:['P','H','R','C']},kind:{enum:['touch','mouse','pen']}},['id','x','y','startX','startY','phase'])}},['code','frames'])},
 {name:'run_action',description:'Typed development actions: pause, resume, step, save replay. Arbitrary C# execution is unavailable.',inputSchema:schema({action:{enum:['pause','resume','step','save_replay']},frames:integer(1,600),path:{type:'string',minLength:1,maxLength:512}},['action'])},
 {name:'watch_game_state',description:'Read current state and enable state change notifications.',inputSchema:schema()},
 {name:'unwatch_game_state',description:'Disable state change notifications.',inputSchema:schema()},
 {name:'list_assets',description:'List content files available for inspection.',inputSchema:schema()},
 {name:'reload_assets',description:'Reload textures, fonts and audio in the running Debug build.',inputSchema:schema()}
];
const server=new Server({name:'nightengine-monogame',version:'0.2.0'},{capabilities:{tools:{}}});let watch;
server.setRequestHandler(ListToolsRequestSchema,async()=>({tools}));
server.setRequestHandler(CallToolRequestSchema,async({params})=>{
 try {
  if(!tools.some(t=>t.name===params.name))throw new Error('Unknown tool');
  if(params.name==='unwatch_game_state'){clearInterval(watch);watch=undefined;return {content:[{type:'text',text:'Stopped watching'}]};}
  const command=params.name==='watch_game_state'?'get_objects':params.name;
  const result=await request(command,params.arguments);
  if(params.name==='watch_game_state'&&!watch){let last='';watch=setInterval(async()=>{try{const state=await request('get_objects');const text=JSON.stringify(state.result);if(text!==last){last=text;await server.notification({method:'notifications/message',params:{level:'info',data:{event:'game_state_changed',state:state.result}}});}}catch{clearInterval(watch);watch=undefined;}},500);}
  return {content:params.name==='get_screenshot'?[{type:'image',mimeType:'image/png',data:result.result}]:[{type:'text',text:JSON.stringify(result.result)}]};
 }catch(error){return {isError:true,content:[{type:'text',text:error.message}]};}
});
await server.connect(new StdioServerTransport());
process.stdin.on('end',()=>{clearInterval(watch);process.exit(0);});
