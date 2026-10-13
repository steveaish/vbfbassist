// Light or dark text so a coloured event label stays readable
export function textOn(hex) {
  const n = parseInt((hex ?? '#ffffff').slice(1), 16);
  const lum = 0.299 * ((n >> 16) & 255) + 0.587 * ((n >> 8) & 255) + 0.114 * (n & 255);
  return lum > 150 ? '#1e2a26' : '#ffffff';
}

// Swatches offered when choosing an event colour (the most used ones from the Laravel data)
export const eventColours = ['#ffffff', '#ff4015', '#ff0000', '#80ff00', '#00ff00', '#18fb82', '#ffff80', '#ffff00', '#01c7fc', '#0000ff', '#000080', '#ff7f00'];
