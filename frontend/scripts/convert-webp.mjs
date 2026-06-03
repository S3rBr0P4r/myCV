import sharp from 'sharp';
import { readdir, unlink, readFile, writeFile } from 'node:fs/promises';
import { join, parse } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';

const PUBLIC = join(fileURLToPath(new URL('..', import.meta.url)), 'src', 'public');

function psConvertIcoToPng(input, output) {
  const script = `
    Add-Type -AssemblyName System.Drawing
    $$ico = [System.Drawing.Icon]::FromHandle((New-Object System.Drawing.Icon '${input.replace(/\\/g, '\\\\')}').Handle)
    $$bmp = $$ico.ToBitmap()
    $$bmp.Save('${output.replace(/\\/g, '\\\\')}', [System.Drawing.Imaging.ImageFormat]::Png)
    $$bmp.Dispose()
    $$ico.Dispose()
  `;
  const r = spawnSync('powershell', ['-NoProfile', '-Command', script], { encoding: 'utf8' });
  return r.status === 0;
}

async function convertDir(dir) {
  const fullPath = join(PUBLIC, dir);
  const entries = await readdir(fullPath, { withFileTypes: true });
  for (const entry of entries) {
    if (!entry.isFile()) continue;
    const ext = parse(entry.name).ext.toLowerCase();
    if (ext === '.webp') continue;
    const input = join(fullPath, entry.name);
    const output = join(fullPath, parse(entry.name).name + '.webp');
    try {
      if (ext === '.ico') {
        const tmp = join(fullPath, parse(entry.name).name + '_tmp.png');
        if (psConvertIcoToPng(input, tmp)) {
          await sharp(tmp).webp({ quality: 85 }).toFile(output);
          await unlink(tmp);
        } else {
          throw new Error('PowerShell ICO conversion failed');
        }
      } else {
        await sharp(input).webp({ quality: 85 }).toFile(output);
      }
      await unlink(input);
      console.log(`  ✓ ${entry.name} → ${parse(entry.name).name}.webp`);
    } catch (err) {
      console.log(`  ✗ ${entry.name}: ${err.message}`);
    }
  }
}

async function main() {
  console.log('Converting backgrounds...');
  await convertDir('backgrounds');
  console.log('Converting logos...');
  await convertDir('logos');
}

main().catch(console.error);
