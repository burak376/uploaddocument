import { spawn } from 'node:child_process';
import { setTimeout as wait } from 'node:timers/promises';
import { once } from 'node:events';

const abortController = new AbortController();

async function build() {
  console.log('Building production bundle...');
  const buildProcess = spawn('npm', ['run', 'build'], { stdio: 'inherit', signal: abortController.signal });
  const [code] = await once(buildProcess, 'exit');
  if (code !== 0) {
    throw new Error(`Build failed with exit code ${code}`);
  }
}

async function startPreviewServer() {
  console.log('Starting static server on http://localhost:4173 ...');
  const server = spawn('npx', ['serve', 'dist', '-l', '4173', '--single'], { stdio: 'pipe', signal: abortController.signal });

  let serverReady = false;
  server.stdout.on('data', (data) => {
    const text = data.toString();
    process.stdout.write(text);
    if (text.includes('Accepting connections')) {
      serverReady = true;
    }
  });
  server.stderr.on('data', (data) => process.stderr.write(data.toString()));

  for (let i = 0; i < 20; i += 1) {
    if (serverReady) break;
    await wait(250);
  }

  if (!serverReady) {
    server.kill();
    throw new Error('Static server did not start in time');
  }

  return server;
}

async function assertRoute(route) {
  const url = `http://localhost:4173${route}`;
  const response = await fetch(url, { redirect: 'manual' });
  if (response.status !== 200) {
    throw new Error(`Expected 200 for ${route}, received ${response.status}`);
  }
  const text = await response.text();
  if (!text.includes('<div id="root"></div>')) {
    throw new Error(`Response for ${route} does not contain the SPA root element.`);
  }
  console.log(`✔ Route ${route} serves index.html`);
}

async function main() {
  await build();
  const server = await startPreviewServer();

  try {
    await assertRoute('/');
    await assertRoute('/login');
    await assertRoute('/dashboard');
  } finally {
    server.kill('SIGTERM');
    await wait(250);
    if (!server.killed) {
      server.kill('SIGKILL');
    }
    server.stdout?.destroy();
    server.stderr?.destroy();
  }

  console.log('SPA routing test finished successfully.');
}

main().catch((error) => {
  abortController.abort();
  console.error(error);
  process.exitCode = 1;
});
