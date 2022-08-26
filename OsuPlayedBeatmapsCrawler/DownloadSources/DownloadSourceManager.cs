using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsuPlayedBeatmapsCrawler.DownloadSources
{
    public static class DownloadSourceManager
    {
        private static Dictionary<string, Type> registeredSources = new();

        public static void RegisterDownloadSource<T>(string optName) where T : DownloadSourceBase
        {
            registeredSources[optName] = typeof(T);
        }

        public static DownloadSourceBase GetSource(string optName, HttpClient client)
        {
            if (!registeredSources.TryGetValue(optName, out var type))
                return default;

            return Activator.CreateInstance(type, new[] { client }) as DownloadSourceBase;
        }
    }
}
