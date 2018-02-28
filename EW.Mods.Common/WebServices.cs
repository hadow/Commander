using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
namespace EW.Mods.Common
{
    public enum ModVersionStatus { NotChecked, Latest, Outdated, Unknown, PlaytestAvailable }

    public class WebServices : IGlobalModData
    {
        public readonly string ServerList = "http://master.openra.net/games";
        public readonly string ServerAdvertise = "http://master.openra.net/ping";
        public readonly string MapRepository = "http://resource.openra.net/map/";
        public readonly string GameNews = "http://master.openra.net/gamenews";
        public readonly string VersionCheck = "http://master.openra.net/versioncheck";

        public ModVersionStatus ModVersionStatus { get; private set; }
        const int VersionCheckProtocol = 1;

        public void CheckModVersion()
        {
            Action<DownloadDataCompletedEventArgs> onComplete = i =>
            {
                if (i.Error != null)
                    return;
                try
                {
                    var data = Encoding.UTF8.GetString(i.Result);

                    var status = ModVersionStatus.Latest;
                    switch (data)
                    {
                        case "outdated": status = ModVersionStatus.Outdated; break;
                        case "unknown": status = ModVersionStatus.Unknown; break;
                        case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
                    }

                    WarGame.RunAfterTick(() => ModVersionStatus = status);
                }
                catch { }
            };

            var queryURL = VersionCheck + "?protocol={0}&engine={1}&mod={2}&version={3}".F(
                VersionCheckProtocol,
                Uri.EscapeUriString(WarGame.EngineVersion),
                Uri.EscapeUriString(WarGame.ModData.Manifest.Id),
                Uri.EscapeUriString(WarGame.ModData.Manifest.Metadata.Version));

            new Download(queryURL, _ => { }, onComplete);
        }
    }
}