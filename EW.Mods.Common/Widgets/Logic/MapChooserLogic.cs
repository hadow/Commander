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

        Dictionary<MapClassification, ScrollPanelWidget> scrollpanels = new Dictionary<MapClassification, ScrollPanelWidget>();
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

            var itemTemplate = widget.Get<ScrollItemWidget>("MAP_TEMPLATE");
            widget.RemoveChild(itemTemplate);

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
            

            SetupMapTab(MapClassification.User, filter, "USER_MAPS_TAB_BUTTON", "USER_MAPS_TAB", itemTemplate);
            SetupMapTab(MapClassification.System, filter, "SYSTEM_MAPS_TAB_BUTTON", "SYSTEM_MAPS_TAB", itemTemplate);

            if (initialMap == null && tabMaps.Keys.Contains(initialTab) && tabMaps[initialTab].Any())
            {
                selectedUid = WarGame.ModData.MapCache.ChooseInitialMap(tabMaps[initialTab].Select(mp => mp.Uid).First(),
                    WarGame.CosmeticRandom);
                currentTab = initialTab;
            }
            else
            {
                selectedUid = WarGame.ModData.MapCache.ChooseInitialMap(initialMap, WarGame.CosmeticRandom);
                currentTab = tabMaps.Keys.FirstOrDefault(k => tabMaps[k].Select(mp => mp.Uid).Contains(selectedUid));
            }

            SwitchTab(currentTab, itemTemplate);
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

            var maps = tabMaps[tab]
                .Where(m => category == null || m.Categories.Contains(category))
                .Where(m => mapFilter == null ||
                    (m.Title != null && m.Title.IndexOf(mapFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (m.Author != null && m.Author.IndexOf(mapFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    m.PlayerCount == playerCountFilter)
                .OrderBy(m => m.PlayerCount)
                .ThenBy(m => m.Title);

            scrollpanels[tab].RemoveChildren();
            foreach(var loop in maps)
            {
                var preview = loop;

                // Access the minimap to trigger async generation of the minimap.
                preview.GetMinimap();

                Action dblClick = () =>
                {
                    if (onSelect != null)
                    {
                        UI.CloseWindow();
                        onSelect(preview.Uid);
                    }
                };

                var item = ScrollItemWidget.Setup(preview.Uid, template, () => selectedUid == preview.Uid,
                    () => selectedUid = preview.Uid, dblClick);
                //item.IsVisible = () => item.RenderBounds.IntersectsWith(scrollpanels[tab].RenderBounds);
                item.IsVisible = () => true;
                var titleLabel = item.Get<LabelWidget>("TITLE");
                if (titleLabel != null)
                {
                    var font = WarGame.Renderer.Fonts[titleLabel.Font];
                    var title = WidgetUtils.TruncateText(preview.Title, titleLabel.Bounds.Width, font);
                    titleLabel.GetText = () => title;
                }

                var previewWidget = item.Get<MapPreviewWidget>("PREVIEW");
                previewWidget.Preview = () => preview;

                var detailsWidget = item.GetOrNull<LabelWidget>("DETAILS");
                if (detailsWidget != null)
                {
                    var type = preview.Categories.FirstOrDefault();
                    var details = "";
                    if (type != null)
                        details = type + " ";

                    details += "({0} players)".F(preview.PlayerCount);
                    detailsWidget.GetText = () => details;
                }

                var authorWidget = item.GetOrNull<LabelWidget>("AUTHOR");
                if (authorWidget != null)
                {
                    var font = WarGame.Renderer.Fonts[authorWidget.Font];
                    var author = WidgetUtils.TruncateText("Created by {0}".F(preview.Author), authorWidget.Bounds.Width, font);
                    authorWidget.GetText = () => author;
                }

                var sizeWidget = item.GetOrNull<LabelWidget>("SIZE");
                if (sizeWidget != null)
                {
                    var size = preview.Bounds.Width + "x" + preview.Bounds.Height;
                    var numberPlayableCells = preview.Bounds.Width * preview.Bounds.Height;
                    if (numberPlayableCells >= 120 * 120) size += " (Huge)";
                    else if (numberPlayableCells >= 90 * 90) size += " (Large)";
                    else if (numberPlayableCells >= 60 * 60) size += " (Medium)";
                    else size += " (Small)";
                    sizeWidget.GetText = () => size;
                }

                scrollpanels[tab].AddChild(item);
            }

            if (tab == currentTab)
            {
                visibleMaps = maps.Select(m => m.Uid).ToArray();
                //SetupGameModeDropdown(currentTab, gameModeDropdown, template);
            }

            if (visibleMaps.Contains(selectedUid))
                scrollpanels[tab].ScrollToItem(selectedUid);
        }

        void SetupMapTab(MapClassification tab, MapVisibility filter, string tabButtonName, string tabContainerName, ScrollItemWidget itemTemplate)
        {

            var tabContainer = widget.Get<ContainerWidget>(tabContainerName);
            tabContainer.IsVisible = () => currentTab == tab;
            var tabScrollpanel = tabContainer.Get<ScrollPanelWidget>("MAP_LIST");
            tabScrollpanel.Layout = new GridLayout(tabScrollpanel);
            scrollpanels.Add(tab, tabScrollpanel);

            var tabButton = widget.Get<ButtonWidget>(tabButtonName);
            tabButton.IsHighlighted = () => currentTab == tab;
            tabButton.IsVisible = () => tabMaps[tab].Any();
            tabButton.OnClick = () => SwitchTab(tab, itemTemplate);

            RefreshMaps(tab, filter);
        }

        void RefreshMaps(MapClassification tab, MapVisibility filter)
        {
            tabMaps[tab] = modData.MapCache.Where(m => m.Status == MapStatus.Available &&
                m.Class == tab && (m.Visibility & filter) != 0).ToArray();
        }
    }
}