using System;
using EW.Mods.Common.Traits;
using System.Collections.Generic;
using System.Linq;
using EW.Widgets;
namespace EW.Mods.Common.Widgets
{

    public class ProductionTab
    {
        public string Name;
        public ProductionQueue Queue;
    }


    public class ProductionTabGroup{

        public List<ProductionTab> Tabs = new List<ProductionTab>();
        public string Group;
        public int NextQueueName = 1;
    }
    public class ProductionTabsWidget:Widget
    {

        readonly World world;


        public readonly int TabWidth = 30;
        public readonly int ArrowWidth = 20;

        public readonly Dictionary<string, ProductionTabGroup> Groups;
        Lazy<ProductionPaletteWidget> paletteWidget;

        string queueGroup;

        [ObjectCreator.UseCtor]
        public ProductionTabsWidget(World world)
        {
            this.world = world;

            Groups = world.Map.Rules.Actors.Values.SelectMany(a => a.TraitInfos<ProductionQueueInfo>())
                .Select(q => q.Group).Distinct().ToDictionary(g => g, g => new ProductionTabGroup() { Group = g });

            // Only visible if the production palette has icons to display
            IsVisible = () => queueGroup != null && Groups[queueGroup].Tabs.Count > 0;
        }

        public ProductionQueue CurrentQueue
        {
            get
            {
                return paletteWidget.Value.CurrentQueue;
            }

            set
            {
                paletteWidget.Value.CurrentQueue = value;
                queueGroup = value != null ? value.Info.Group : null;

                // TODO: Scroll tabs so selected queue is visible
            }
        }
    }
}
