using System;
using System.Linq;
using EW.Widgets;
using EW.NetWork;
using EW.Mods.Common.Traits;


namespace EW.Mods.Common.Widgets.Logic
{
    public class ClassicProductionLogic:ChromeLogic
    {

        readonly ProductionPaletteWidget palette;
        readonly World world;

        [ObjectCreator.UseCtor]
        public ClassicProductionLogic(Widget widget, OrderManager orderManager, World world)
        {

            this.world = world;
            palette = widget.Get<ProductionPaletteWidget>("PRODUCTION_PALETTE");


            var typesContainer = widget.Get("PRODUCTION_TYPES");
            foreach (var i in typesContainer.Children)
                SetupProductionGroupButton(orderManager, i as ProductionTypeButtonWidget);


            var ticker = widget.Get<LogicTickerWidget>("PRODUCTION_TICKER");
            ticker.OnTick = () =>
            {
                if (palette.CurrentQueue == null || palette.DisplayedIconCount == 0)
                {
                    // Select the first active tab
                    foreach (var b in typesContainer.Children)
                    {
                        var button = b as ProductionTypeButtonWidget;
                        if (button == null || button.IsDisabled())
                            continue;

                        button.OnClick();
                        break;
                    }
                }
            };
        }


        void SetupProductionGroupButton(OrderManager orderManager, ProductionTypeButtonWidget button)
        {
            if (button == null)
                return;

            // Classic production queues are initialized at game start, and then never change.
            var queues = world.LocalPlayer.PlayerActor.TraitsImplementing<ProductionQueue>()
                .Where(q => q.Info.Type == button.ProductionGroup)
                .ToArray();

            Action<bool> selectTab = reverse =>
            {
                palette.CurrentQueue = queues.FirstOrDefault(q => q.Enabled);

                // When a tab is selected, scroll to the top because the current row position may be invalid for the new tab
                //palette.ScrollToTop();

                // Attempt to pick up a completed building (if there is one) so it can be placed
                //palette.PickUpCompletedBuilding();
            };

            button.OnClick = () => selectTab(false);

            var chromeName = button.ProductionGroup.ToLowerInvariant();
            var icon = button.Get<ImageWidget>("ICON");
            icon.GetImageName = () => button.IsDisabled() ? chromeName + "-disabled" :
                queues.Any(q => q.CurrentDone) ? chromeName + "-alert" : chromeName;
        }
    }
}
