using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Widgets;
using EW.Graphics;
using EW.NetWork;
using EW.Framework;
using EW.Mods.Common.Traits;
using EW.Framework.Touch;
using EW.Mods.Common.Orders;
namespace EW.Mods.Common.Widgets
{

    public class ProductionIcon
    {
        public ActorInfo Actor;
        public string Name;
        public Sprite Sprite;
        public PaletteReference Palette;
        public PaletteReference IconClockPalette;
        public PaletteReference IconDarkenPalette;
        public Vector2 Pos;
        public List<ProductionItem> Queued;
        public ProductionQueue ProductionQueue;
    }

    public class ProductionPaletteWidget:Widget
    {
        [Translate] public readonly string ReadyText = "";
        [Translate] public readonly string HoldText = "";

        public readonly int Columns = 3;
        public readonly Int2 IconSize = new Int2(64, 48);
        public readonly Int2 IconMargin = Int2.Zero;
        public readonly Int2 IconSpriteOffset = Int2.Zero;

        public int MinimumRows = 4;
        public int MaximumRows = int.MaxValue;

        public int IconRowOffset = 0;
        public int MaxIconRowOffset = int.MaxValue;

        public readonly string TabClick = null;


        public readonly string ClockAnimation = "clock";
        public readonly string ClockSequence = "idle";
        public readonly string ClockPalette = "chrome";

        public readonly string NotBuildableAnimation = "clock";
        public readonly string NotBuildableSequence = "idle";
        public readonly string NotBuildablePalette = "chrome";

        public int DisplayedIconCount { get; private set; }
        public int TotalIconCount { get; private set; }
        public event Action<int, int> OnIconCountChanged = (a, b) => { };

        public readonly World World;
        readonly ModData modData;
        readonly OrderManager orderManager;

        Dictionary<Rectangle, ProductionIcon> icons = new Dictionary<Rectangle, ProductionIcon>();

        Animation cantBuild, clock;
        Rectangle eventBounds = Rectangle.Empty;

        readonly WorldRenderer worldRenderer;

        SpriteFont overlayFont;
        Vector2 holdOffset, readyOffset, timeOffset, queuedOffset;

        ProductionQueue currentQueue;

        public ProductionQueue CurrentQueue
        {
            get { return currentQueue; }
            set { currentQueue = value; RefreshIcons(); }
        }

        [ObjectCreator.UseCtor]
        public ProductionPaletteWidget(ModData modData, OrderManager orderManager, World world, WorldRenderer worldRenderer)
        {
            this.modData = modData;
            this.orderManager = orderManager;
            World = world;
            this.worldRenderer = worldRenderer;


            cantBuild = new Animation(world, NotBuildableAnimation);
            cantBuild.PlayFetchIndex(NotBuildableSequence, () => 0);
            clock = new Animation(world, ClockAnimation);
        }


        public IEnumerable<ActorInfo> AllBuildables
        {
            get
            {
                if (CurrentQueue == null)
                    return Enumerable.Empty<ActorInfo>();

                return CurrentQueue.AllItems().OrderBy(a => a.TraitInfo<BuildableInfo>().BuildPaletteOrder);
            }
        }

        public void RefreshIcons(){

            icons = new Dictionary<Rectangle, ProductionIcon>();
            var producer = CurrentQueue != null ? CurrentQueue.MostLikelyProducer() : default(TraitPair<Production>);
            if (CurrentQueue == null || producer.Trait == null)
            {
                if (DisplayedIconCount != 0)
                {
                    OnIconCountChanged(DisplayedIconCount, 0);
                    DisplayedIconCount = 0;
                }

                return;
            }

            var oldIconCount = DisplayedIconCount;
            DisplayedIconCount = 0;

            var rb = RenderBounds;
            var faction = producer.Trait.Faction;

            foreach (var item in AllBuildables.Skip(IconRowOffset * Columns).Take(MaxIconRowOffset * Columns)){

                var x = DisplayedIconCount % Columns;
                var y = DisplayedIconCount / Columns;
                var rect = new Rectangle(rb.X + x * (IconSize.X + IconMargin.X), rb.Y + y * (IconSize.Y + IconMargin.Y), IconSize.X, IconSize.Y);

                var rsi = item.TraitInfo<RenderSpritesInfo>();
                var icon = new Animation(World, rsi.GetImage(item, World.Map.Rules.Sequences, faction));
                var bi = item.TraitInfo<BuildableInfo>();
                icon.Play(bi.Icon);

                var pi = new ProductionIcon()
                {
                    Actor = item,
                    Name = item.Name,
                    Sprite = icon.Image,
                    Palette = worldRenderer.Palette(bi.IconPalette),
                    IconClockPalette = worldRenderer.Palette(ClockPalette),
                    IconDarkenPalette = worldRenderer.Palette(NotBuildablePalette),
                    Pos = new Vector2(rect.Location),
                    Queued = currentQueue.AllQueued().Where(a => a.Item == item.Name).ToList(),
                    ProductionQueue = currentQueue
                };

                icons.Add(rect, pi);
                DisplayedIconCount++;
            }

            eventBounds = icons.Any() ? icons.Keys.Aggregate(Rectangle.Union) : Rectangle.Empty;

            if (oldIconCount != DisplayedIconCount)
                OnIconCountChanged(oldIconCount, DisplayedIconCount);

        }


        public override void Tick()
        {
            TotalIconCount = AllBuildables.Count();

            if (CurrentQueue != null && !CurrentQueue.Actor.IsInWorld)
                CurrentQueue = null;

            if (CurrentQueue != null)
                RefreshIcons();
        }

        public override bool HandleInput(GestureSample gs){

            var icon = icons.Where(i => i.Key.Contains(gs.Position.ToInt2())).Select(i => i.Value).FirstOrDefault();

            if (icon == null)
                return false;

            return HandleEvent(icon, gs);
        }

        bool HandleEvent(ProductionIcon icon,GestureSample gs){

            var item =  icon.Queued.FirstOrDefault();

            if(gs.GestureType == GestureType.Tap){

                var startCount = 1;
                var handled = HandlClick(item, icon, startCount);
            }
            return true;
        }


        protected bool PickUpCompletedBuildingIcon(ProductionIcon icon,ProductionItem item){


            var actor = World.Map.Rules.Actors[icon.Name];

            if(item != null && item.Done && actor.HasTraitInfo<BuildingInfo>())
            {
                World.OrderGenerator = new PlaceBuildingOrderGenerator(currentQueue, icon.Name, worldRenderer);
                return true;
            }
            return false;
        }

        bool HandlClick(ProductionItem item,ProductionIcon icon,int handleCount){

            if(PickUpCompletedBuildingIcon(icon,item)){
                
                return true;
            }

            if(currentQueue.BuildableItems().Any(a=>a.Name == icon.Name)){

                //Queue a new item
                WarGame.Sound.Play(SoundType.UI, TabClick);
                WarGame.Sound.PlayNotification(World.Map.Rules, World.LocalPlayer, "Speech", CurrentQueue.Info.QueuedAudio, World.LocalPlayer.Faction.InternalName);
                World.IssueOrder(Order.StartProduction(currentQueue.Actor,icon.Name,handleCount));
            }

            return false;
        }

        public override void Draw()
        {
            var iconOffset = 0.5f * IconSize.ToVector2() + IconSpriteOffset;

            overlayFont = WarGame.Renderer.Fonts["TinyBold"];
            timeOffset = iconOffset - overlayFont.Measure(WidgetUtils.FormatTime(0, World.Timestep)) / 2;
            queuedOffset = new Vector2(4, 2);
            holdOffset = iconOffset - overlayFont.Measure(HoldText) / 2;
            readyOffset = iconOffset - overlayFont.Measure(ReadyText) / 2;

            if (CurrentQueue == null)
                return;

            var buildableItems = CurrentQueue.BuildableItems(); 

            var pios = currentQueue.Actor.Owner.PlayerActor.TraitsImplementing<IProductionIconOverlay>();
        

            //Icons
            foreach(var icon in icons.Values){

                WidgetUtils.DrawSHPCentered(icon.Sprite,icon.Pos + iconOffset,icon.Palette);


            }
        }



    }
}
