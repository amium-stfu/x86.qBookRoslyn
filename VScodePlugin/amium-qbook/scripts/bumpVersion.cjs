// Set package.json version based on current date in format yyyy.Md.Hm
// Example (25.02.2026 16:23): 2026.225.1623
const fs = require('fs');
const path = require('path');

const pkgPath = path.join(__dirname, '..', 'package.json');

function getDateBasedVersion() {
  const now = new Date();
  const year = now.getFullYear();

  // .NET "Md" = month (no leading 0) + day (no leading 0) direkt hintereinander
  const month = now.getMonth() + 1; // 1-12
  const day = now.getDate(); // 1-31
  const md = String(month) + String(day); // z.B. 2 und 25 => "225"

  // .NET "Hm" = hour (0-23) + minute (0-59), jeweils ohne führende 0
  const hour = now.getHours();
  const minute = now.getMinutes();
  const hm = String(hour) + String(minute); // z.B. 16 und 23 => "1623"

  return `${year}.${md}.${hm}`;
}

try {
  const raw = fs.readFileSync(pkgPath, 'utf8');
  const pkg = JSON.parse(raw);
  const prev = pkg.version || '';
  const next = getDateBasedVersion();
  pkg.version = next;
  fs.writeFileSync(pkgPath, JSON.stringify(pkg, null, 2) + '\n', 'utf8');
  console.log(`Version set: ${prev} -> ${next}`);
} catch (err) {
  console.error('Failed to set version:', err);
  process.exit(1);
}
