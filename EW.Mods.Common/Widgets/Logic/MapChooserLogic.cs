using System;
using System.Collections.Generic;
using EW.Widgets;
using EW.Primitives;
using System.Linq;

namespace EW.Mods.Common.Widgets.Logic
{
    public class MapChooserLogic : ChromeLogic
    {
        Dictionary<MapClassification, MapPreview[]> tabMaps = new Dictionary<MapClassification, MapPreview[]>();


        readonly Widget widget;

        readonly ModData modData;

        MapClassification currentTab;

        string selectedUid;
        Action<string> onSelect;
        string[] visibleMaps;

        string category;
        string mapFilter;

        [ObjectCreator.UseCtor]
        internal MapChooserLogic(Widget widget, ModData modData, string initialMap,
            MapClassification initialTab, Action onExit, Action<string> onSelect, MapVisibility filter)
        {
            this.widget = widget;
            this.modData = modData;
            this.onSelect = onSelect;

            var approving = new Action(() => { UI.CloseWindow(); onSelect(selectedUid); });
            var canceling = new Action(() => { UI.CloseWindow(); onExit(); });

            var okButton = widget.Get<ButtonWidget>("BUTTON_OK");
            okButton.Disabled = this.onSelect == null;
            okButton.OnClick = approving;
            widget.Get<ButtonWidget>("BUTTON_CANCEL").OnClick = canceling;

            var randomMapButton = widget.GetOrNull<ButtonWidget>("RANDOMMAP_BUTTON");
            if (randomMapButton != null)
            {
                randomMapButton.OnClick = () =>
                {
                    var uid = visibleMaps.Random(WarGame.CosmeticRandom);
                    selectedUid = uid;

                };

                randomMapButton.IsDisabled = () => visibleMaps == null || visibleMaps.Length == 0;

            }
        }

        void SwitchTab(MapClassification tab, ScrollItemWidget itemTemplate)
        {
            currentTab = tab;
            EnumerateMaps(tab, itemTemplate);
        }

        void EnumerateMaps(MapClassification tab, ScrollItemWidget template)
        {
            int playerCountFilter;
            if (!int.TryParse(mapFilter, out playerCountFilter))
                playerCountFilter = -1;


        }

        void SetupMapTab(MapClassification tab, MapVisibility filter, string tabButtonName, string tabContainerName, ScrollItemWidget itemTemplate)
        {



            RefreshMaps(tab, filter);
        }

        void RefreshMaps(MapClassification tab, MapVisibility filter)
        {
            tabMaps[tab] = modData.MapCache.Where(m => m.Status == MapStatus.Available &&
                m.Class == tab && (m.Visibility & filter) != 0).ToArray();
        }
    }
}