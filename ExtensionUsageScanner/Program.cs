using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Qlik.Engine;
using Qlik.Sense.Client;
using Qlik.Sense.RestClient;

namespace ExtensionUsageScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "<url>";
            var extensionNames = GetExtensions(url);

            ScanLocation(url, extensionNames);
        }

        private static void ScanLocation(string url, string[] extensionNames)
        {
            var location = Location.FromUri(url);
            location.AsNtlmUserViaProxy();

            var appIds = location.GetAppIdentifiers();
            var extensionUsage = appIds.Select(appId => (appId, GetExtensionUsage(location, appId, new HashSet<string>(extensionNames))));

            foreach (var (appId, extensions) in extensionUsage.ToArray())
            {
                if (!extensions.Any())
                    continue;

                Console.Write($"Extensions used by app \"{appId.AppName}\" ({appId.AppId})\n  ");
                Console.WriteLine(string.Join("\n  ", extensions.Distinct()));
            }
        }

        private static string[] GetExtensionUsage(ILocation location, IAppIdentifier appId, ICollection<string> extensionNames)
        {
            Console.WriteLine($"Scanning app \"{appId.AppName}\" ({appId.AppId})");
            using (var app = GetApp(location, appId, true) ?? GetApp(location, appId, false))
            {
                if (app == null)
                    throw new Exception($"Unable to open app {appId.AppName}.");

                var sheets = app.GetSheets();
                var objectTypes = sheets.SelectMany(GetObjectTypes);
                return objectTypes.Where(extensionNames.Contains).ToArray();
            }
        }

        private static IEnumerable<string> GetObjectTypes(IGenericObject obj)
        {
            return GetObjectTypes(obj.GetFullPropertyTree());
        }

        private static IEnumerable<string> GetObjectTypes(GenericObjectEntry entry)
        {
            return entry.Children.SelectMany(GetObjectTypes).Append(entry.Property.Info.Type);
        }

        private static IApp GetApp(ILocation location, IAppIdentifier appId, bool noData)
        {
            try
            {
                return location.App(appId, Session.Random, noData: noData);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve the name of all extensions.
        /// </summary>
        /// <param name="url">The location</param>
        /// <returns>An array containing all extension names.</returns>
        private static string[] GetExtensions(string url)
        {
            var client = new RestClient(url);
            client.AsNtlmUserViaProxy();
            var rsp = JArray.Parse(client.Get("/qrs/extension"));
            return rsp.OfType<JObject>().Select(o => o["name"].ToString()).ToArray();
        }
    }
}
