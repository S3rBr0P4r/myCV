import sharp from 'sharp';
import { readFile, writeFile, unlink } from 'node:fs/promises';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createWriteStream } from 'node:fs';
import { get } from 'node:https';

const LOGOS = join(fileURLToPath(new URL('..', import.meta.url)), 'src', 'public', 'logos');
const webpPath = join(LOGOS, 'hemini.webp');

function download(url, dest) {
  return new Promise((resolve, reject) => {
    const file = createWriteStream(dest);
    get(url, res => {
      if (res.statusCode >= 300 && res.headers.location) {
        file.close();
        return download(res.headers.location, dest).then(resolve).catch(reject);
      }
      res.pipe(file);
      file.on('finish', () => { file.close(); resolve(); });
    }).on('error', reject);
  });
}

// Download ICO from icon.horse
const icoPath = join(LOGOS, 'hemini_source.ico');
await download('https://icon.horse/icon/hemini.com', icoPath);

const buf = await readFile(icoPath);
const count = buf.readUInt16LE(4);
console.log(`ICO entries: ${count}`);

for (let i = 0; i < count; i++) {
  const off = 6 + i * 16;
  const w = buf.readUInt8(off);
  const h = buf.readUInt8(off + 1);
  const size = buf.readUInt32LE(off + 8);
  const dataOff = buf.readUInt32LE(off + 12);
  console.log(`Entry ${i}: ${w}x${h}, offset ${dataOff}, size ${size}`);

  // Check if it's a PNG or BMP
  const isPng = buf[dataOff] === 0x89 && buf[dataOff + 1] === 0x50;
  if (isPng) {
    console.log(`  PNG embedded, extracting...`);
    const pngBuf = buf.subarray(dataOff, dataOff + size);
    await writeFile(join(LOGOS, 'hemini_png.png'), pngBuf);
    await sharp(join(LOGOS, 'hemini_png.png'))
      .webp({ quality: 85, alphaQuality: 100 })
      .toFile(webpPath);
    await unlink(join(LOGOS, 'hemini_png.png'));
    console.log(`  ✓ hemini.webp created from embedded PNG`);
    break;
  } else {
    // BMP inside ICO: BITMAPINFOHEADER + AND mask + XOR mask
    // For 32-bit icons: biSize(4) + width(4) + height(4 = 2x actual) + planes(2) + bpp(2) + ...
    const biSize = buf.readUInt32LE(dataOff);
    const bmpWidth = buf.readInt32LE(dataOff + 4);
    const bmpHeight = buf.readInt32LE(dataOff + 8) / 2; // height doubled for ICO
    const bpp = buf.readUInt16LE(dataOff + 14);
    console.log(`  BMP: ${bmpWidth}x${bmpHeight}, ${bpp}bpp, header size ${biSize}`);

    if (bpp === 32) {
      // 32-bit BGRA pixels starting after header + color table
      const pixelOff = dataOff + biSize;
      const rowLen = bmpWidth * 4;
      // BMP rows are bottom-up, so reverse
      const pixels = Buffer.alloc(bmpWidth * bmpHeight * 4);
      for (let y = 0; y < bmpHeight; y++) {
        const srcRow = pixelOff + y * rowLen;
        const dstRow = (bmpHeight - 1 - y) * bmpWidth * 4;
        for (let x = 0; x < bmpWidth; x++) {
          const si = srcRow + x * 4;
          const di = dstRow + x * 4;
          // BMP is BGRA, need RGBA
          pixels[di] = buf[si + 2];     // R
          pixels[di + 1] = buf[si + 1]; // G
          pixels[di + 2] = buf[si];     // B
          pixels[di + 3] = buf[si + 3]; // A
        }
      }
      await sharp(pixels, { raw: { width: bmpWidth, height: bmpHeight, channels: 4 } })
        .webp({ quality: 85, alphaQuality: 100 })
        .toFile(webpPath);
      console.log(`  ✓ hemini.webp created from BMP (with alpha)`);
      break;
    } else {
      // Lower bpp — convert via PNG temp
      console.log(`  Non-32bpp, extracting via raw...`);
      // For now fallback to the old method
    }
  }
}

await unlink(icoPath);
const { stat } = await import('node:fs/promises');
const s = await stat(webpPath);
console.log(`Final hemini.webp: ${s.size} bytes`);
