using System;
using System.Collections.Concurrent;

namespace MasterAntiqueRepair
{
    /// <summary>
    /// In-memory, per-IP rate limiter. Complements User's per-account lockout by also
    /// slowing an attacker who spreads attempts across many different usernames from one
    /// IP (which per-account lockout alone doesn't catch). State is process-local - lost on
    /// app restart, not shared across server instances - which is an accepted limitation
    /// here since this app has no reverse proxy/WAF layer to do this instead (see README).
    /// </summary>
    public static class IpThrottle
    {
        public const int MaxAttemptsPerWindow = 20;
        public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

        private class Entry
        {
            public int Count;
            public DateTime WindowStart;
            public DateTime? BlockedUntil;
        }

        private static readonly ConcurrentDictionary<string, Entry> _entries = new ConcurrentDictionary<string, Entry>();

        public static bool IsBlocked(string scope, string ipAddress)
        {
            Entry entry;
            if (!_entries.TryGetValue(Key(scope, ipAddress), out entry))
            {
                return false;
            }

            lock (entry)
            {
                return entry.BlockedUntil.HasValue && entry.BlockedUntil.Value > DateTime.Now;
            }
        }

        public static void RecordAttempt(string scope, string ipAddress)
        {
            var entry = _entries.GetOrAdd(Key(scope, ipAddress), _ => new Entry { WindowStart = DateTime.Now });

            lock (entry)
            {
                if (DateTime.Now - entry.WindowStart > Window)
                {
                    entry.WindowStart = DateTime.Now;
                    entry.Count = 0;
                    entry.BlockedUntil = null;
                }

                entry.Count++;
                if (entry.Count >= MaxAttemptsPerWindow)
                {
                    entry.BlockedUntil = DateTime.Now.Add(Window);
                }
            }
        }

        private static string Key(string scope, string ipAddress)
        {
            return scope + ":" + (ipAddress ?? "unknown");
        }
    }
}
