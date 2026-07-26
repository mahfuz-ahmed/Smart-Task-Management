/**
 * Centralized date/time utilities for consistent handling across the app
 */

/**
 * Parse backend DateTime string (ISO 8601 UTC) to Date object
 */
export function parseBackendDate(dateStr: string | null | undefined): Date | null {
  if (!dateStr) return null;
  try {
    return new Date(dateStr);
  } catch {
    return null;
  }
}

/**
 * Format date for display (e.g., "Jan 15, 2026")
 */
export function formatDate(date: Date | string | null | undefined): string {
  if (!date) return '';
  const d = typeof date === 'string' ? parseBackendDate(date) : date;
  if (!d) return '';
  
  return d.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

/**
 * Format date and time for display (e.g., "Jan 15, 2026 3:30 PM")
 */
export function formatDateTime(date: Date | string | null | undefined): string {
  if (!date) return '';
  const d = typeof date === 'string' ? parseBackendDate(date) : date;
  if (!d) return '';
  
  return d.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
}

/**
 * Format date for backend (ISO 8601 format)
 */
export function formatForBackend(date: Date | string | null | undefined): string | null {
  if (!date) return null;
  const d = typeof date === 'string' ? parseBackendDate(date) : date;
  if (!d) return null;
  
  return d.toISOString();
}

/**
 * Get relative time string (e.g., "2 hours ago", "in 3 days")
 */
export function getRelativeTime(date: Date | string | null | undefined): string {
  if (!date) return '';
  const d = typeof date === 'string' ? parseBackendDate(date) : date;
  if (!d) return '';
  
  const now = new Date();
  const diffMs = d.getTime() - now.getTime();
  const diffSec = Math.abs(Math.floor(diffMs / 1000));
  const diffMin = Math.floor(diffSec / 60);
  const diffHour = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHour / 24);
  
  const isPast = diffMs < 0;
  const prefix = isPast ? '' : 'in ';
  const suffix = isPast ? ' ago' : '';
  
  if (diffSec < 60) return 'just now';
  if (diffMin < 60) return `${prefix}${diffMin} minute${diffMin > 1 ? 's' : ''}${suffix}`;
  if (diffHour < 24) return `${prefix}${diffHour} hour${diffHour > 1 ? 's' : ''}${suffix}`;
  if (diffDay < 7) return `${prefix}${diffDay} day${diffDay > 1 ? 's' : ''}${suffix}`;
  if (diffDay < 30) return `${prefix}${Math.floor(diffDay / 7)} week${Math.floor(diffDay / 7) > 1 ? 's' : ''}${suffix}`;
  
  return formatDate(d);
}

/**
 * Check if date is overdue (past and not today)
 */
export function isOverdue(date: Date | string | null | undefined): boolean {
  if (!date) return false;
  const d = typeof date === 'string' ? parseBackendDate(date) : date;
  if (!d) return false;
  
  const now = new Date();
  now.setHours(0, 0, 0, 0);
  d.setHours(0, 0, 0, 0);
  
  return d < now;
}

/**
 * Check if date is today
 */
export function isToday(date: Date | string | null | undefined): boolean {
  if (!date) return false;
  const d = typeof date === 'string' ? parseBackendDate(date) : date;
  if (!d) return false;
  
  const now = new Date();
  return (
    d.getDate() === now.getDate() &&
    d.getMonth() === now.getMonth() &&
    d.getFullYear() === now.getFullYear()
  );
}

/**
 * Check if date is within next N days
 */
export function isUpcoming(date: Date | string | null | undefined, days: number = 7): boolean {
  if (!date) return false;
  const d = typeof date === 'string' ? parseBackendDate(date) : date;
  if (!d) return false;
  
  const now = new Date();
  const future = new Date();
  future.setDate(future.getDate() + days);
  
  return d >= now && d <= future;
}
