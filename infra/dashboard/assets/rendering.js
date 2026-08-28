// Canvas charts for the static dashboard.

import { truncate } from './charts.js';

/** Shared 5-color palette for donut slices and the matching legend swatches. */
export const DONUT_COLORS = ['#FFFFFF', '#A6A7AC', '#32D583', '#F5B942', '#F04438'];

export function drawBarChart(canvas, series, options = {}) {
  const ctx = canvas.getContext('2d');
  const { width, height } = canvas;
  ctx.clearRect(0, 0, width, height);
  if (!series?.length) return drawEmptyState(ctx, width, height);
  if (options.horizontal) return drawHorizontalBarChart(ctx, width, height, series, options);

  const max = Math.max(...series.map((point) => point.value), 1);
  const barWidth = width / series.length;
  series.forEach((point, index) => {
    const barHeight = Math.max(0, (point.value / max) * (height - 28));
    ctx.fillStyle = options.color || '#FFFFFF';
    ctx.fillRect(index * barWidth + 4, height - barHeight - 20, Math.max(1, barWidth - 8), barHeight);
    ctx.fillStyle = '#A6A7AC'; ctx.font = '10px Inter, "Segoe UI", sans-serif'; ctx.textAlign = 'center';
    ctx.fillText(truncate(point.label, 12), index * barWidth + barWidth / 2, height - 6);
  });
}

export function drawLineChart(canvas, series, options = {}) {
  const ctx = canvas.getContext('2d'); const { width, height } = canvas;
  ctx.clearRect(0, 0, width, height);
  if (!series?.length) return drawEmptyState(ctx, width, height);
  const max = Math.max(...series.map((point) => point.y), 1); const stepX = series.length > 1 ? width / (series.length - 1) : width;
  ctx.strokeStyle = options.color || '#FFFFFF'; ctx.lineWidth = 2; ctx.beginPath();
  series.forEach((point, index) => { const x = index * stepX; const y = height - (point.y / max) * (height - 20) - 10; index ? ctx.lineTo(x, y) : ctx.moveTo(x, y); });
  ctx.stroke();
}

export function drawDonutChart(canvas, series, options = {}) {
  const ctx = canvas.getContext('2d'); const { width, height } = canvas;
  ctx.clearRect(0, 0, width, height);
  if (!series?.length) return drawEmptyState(ctx, width, height);
  const colors = options.colors || DONUT_COLORS;
  const total = series.reduce((sum, point) => sum + point.value, 0) || 1; const x = width / 2; const y = height / 2; const radius = Math.min(width, height) * .34; let start = -Math.PI / 2;
  series.forEach((point, index) => { const end = start + (point.value / total) * Math.PI * 2; ctx.beginPath(); ctx.strokeStyle = colors[index % colors.length]; ctx.lineWidth = radius * .36; ctx.arc(x, y, radius, start, end); ctx.stroke(); start = end; });
  ctx.fillStyle = '#FFFFFF'; ctx.font = '700 20px Inter, "Segoe UI", sans-serif'; ctx.textAlign = 'center'; ctx.fillText(new Intl.NumberFormat('pt-BR').format(total), x, y + 4);
  ctx.fillStyle = '#A6A7AC'; ctx.font = '11px Inter, "Segoe UI", sans-serif'; ctx.fillText('eventos', x, y + 20);
}

function drawHorizontalBarChart(ctx, width, height, series, options) {
  const max = Math.max(...series.map((point) => point.value), 1); const rowHeight = height / series.length; const labelWidth = Math.min(width * .48, 190);
  series.forEach((point, index) => { const y = index * rowHeight + 3; const barX = labelWidth + 8; const barWidth = width - barX - 8; const barHeight = Math.max(8, rowHeight - 12); ctx.fillStyle = '#2D2E33'; ctx.fillRect(barX, y + 4, barWidth, barHeight); ctx.fillStyle = options.color || '#FFFFFF'; ctx.fillRect(barX, y + 4, (point.value / max) * barWidth, barHeight); ctx.fillStyle = '#FFFFFF'; ctx.font = '11px Inter, "Segoe UI", sans-serif'; ctx.textAlign = 'left'; ctx.fillText(truncate(point.label, 22), 0, y + Math.max(15, rowHeight / 2 + 4)); });
}

function drawEmptyState(ctx, width, height) { ctx.fillStyle = '#A6A7AC'; ctx.font = '12px Inter, "Segoe UI", sans-serif'; ctx.textAlign = 'center'; ctx.fillText('Sem dados ainda', width / 2, height / 2); }
