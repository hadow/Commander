using System;
using System.Linq;
using EW.Widgets;
using System.Collections.Generic;
using EW.Graphics;
using System.Drawing;
using EW.Framework;
using EW.Server;
using EW.NetWork;
namespace EW.Mods.Common.Widgets
{

    public class SpawnOccupant
    {
        public readonly HSLColor Color;
        public readonly string PlayerName;
        public readonly int Team;
        public readonly string Faction;
        public readonly int SpawnPoint;

        public SpawnOccupant(Session.Client client)
        {
            Color = client.Color;
            PlayerName = client.Name;
            Team = client.Team;
            Faction = client.Faction;
            SpawnPoint = client.SpawnPoint;
        }

        //public SpawnOccupant(GameInformation.Player player)
        //{
        //    Color = player.Color;
        //    PlayerName = player.Name;
        //    Team = player.Team;
        //    Faction = player.FactionId;
        //    SpawnPoint = player.SpawnPoint;
        //}

        public SpawnOccupant(GameClient player, bool suppressFaction)
        {
            Color = player.Color;
            PlayerName = player.Name;
            Team = player.Team;
            Faction = !suppressFaction ? player.Faction : null;
            SpawnPoint = player.SpawnPoint;
        }
    }

    public class MapPreviewWidget:Widget
    {
        public readonly bool IgnoreMouseInput = false;
        public readonly bool ShowSpawnPoints = true;

        public readonly string TooltipContainer;
        public readonly string TooltipTemplate = "SPAWN_TOOLTIP";

        readonly Sprite spawnClaimed, spawnUnclaimed;
        readonly SpriteFont spawnFont;
        readonly Color spawnColor, spawnContrastColor;
        readonly Int2 spawnLabelOffset;

        public Func<MapPreview> Preview = () => null;
        public Func<Dictionary<CPos, SpawnOccupant>> SpawnOccupants = () => new Dictionary<CPos, SpawnOccupant>();
        public Action<MouseInput> OnMouseDown = _ => { };
        public int TooltipSpawnIndex = -1;
        public bool ShowUnoccupiedSpawnpoints = true;

        Rectangle mapRect;
        float previewScale = 0;
        Sprite minimap;

        public MapPreviewWidget()
        {
            
            spawnClaimed = ChromeProvider.GetImage("lobby-bits", "spawn-claimed");
            spawnUnclaimed = ChromeProvider.GetImage("lobby-bits", "spawn-unclaimed");
            spawnFont = WarGame.Renderer.Fonts[ChromeMetrics.Get<string>("SpawnFont")];
            spawnColor = ChromeMetrics.Get<Color>("SpawnColor");
            spawnContrastColor = ChromeMetrics.Get<Color>("SpawnContrastColor");
            spawnLabelOffset = ChromeMetrics.Get<Int2>("SpawnLabelOffset");
        }

        protected MapPreviewWidget(MapPreviewWidget other)
            : base(other)
        {
            Preview = other.Preview;

            IgnoreMouseInput = other.IgnoreMouseInput;
            ShowSpawnPoints = other.ShowSpawnPoints;
            TooltipTemplate = other.TooltipTemplate;
            TooltipContainer = other.TooltipContainer;
            SpawnOccupants = other.SpawnOccupants;
            

            spawnClaimed = ChromeProvider.GetImage("lobby-bits", "spawn-claimed");
            spawnUnclaimed = ChromeProvider.GetImage("lobby-bits", "spawn-unclaimed");
            spawnFont = WarGame.Renderer.Fonts[ChromeMetrics.Get<string>("SpawnFont")];
            spawnColor = ChromeMetrics.Get<Color>("SpawnColor");
            spawnContrastColor = ChromeMetrics.Get<Color>("SpawnContrastColor");
            spawnLabelOffset = ChromeMetrics.Get<Int2>("SpawnLabelOffset");
        }

        public override Widget Clone() { return new MapPreviewWidget(this); }

        public Int2 ConvertToPreview(CPos cell, MapGridT gridType)
        {
            var preview = Preview();
            var point = cell.ToMPos(gridType);
            var cellWidth = gridType == MapGridT.RectangularIsometric ? 2 : 1;
            var dx = (int)(previewScale * cellWidth * (point.U - preview.Bounds.Left));
            var dy = (int)(previewScale * (point.V - preview.Bounds.Top));

            // Odd rows are shifted right by 1px
            if ((point.V & 1) == 1)
                dx += 1;

            return new Int2(mapRect.X + dx, mapRect.Y + dy);
        }

        public override void Draw()
        {
            var preview = Preview();
            if (preview == null)
                return;

            // Stash a copy of the minimap to ensure consistency
            // (it may be modified by another thread)
            minimap = preview.GetMinimap();
            if (minimap == null)
                return;

            // Update map rect
            previewScale = Math.Min(RenderBounds.Width / minimap.Size.X, RenderBounds.Height / minimap.Size.Y);
            var w = (int)(previewScale * minimap.Size.X);
            var h = (int)(previewScale * minimap.Size.Y);
            var x = RenderBounds.X + (RenderBounds.Width - w) / 2;
            var y = RenderBounds.Y + (RenderBounds.Height - h) / 2;
            mapRect = new Rectangle(x, y, w, h);

            WarGame.Renderer.RgbaSpriteRenderer.DrawSprite(minimap, new Vector2(mapRect.Location), new Vector2(mapRect.Size));

            TooltipSpawnIndex = -1;
            if (ShowSpawnPoints)
            {
                var colors = SpawnOccupants().ToDictionary(c => c.Key, c => c.Value.Color.RGB);

                var spawnPoints = preview.SpawnPoints;
                var gridType = preview.GridT;
                foreach (var p in spawnPoints)
                {
                    var owned = colors.ContainsKey(p);
                    var pos = ConvertToPreview(p, gridType);
                    var sprite = owned ? spawnClaimed : spawnUnclaimed;
                    var offset = new Int2(sprite.Bounds.Width, sprite.Bounds.Height) / 2;

                    if (owned)
                        WidgetUtils.FillEllipseWithColor(new Rectangle(pos.X - offset.X + 1, pos.Y - offset.Y + 1, sprite.Bounds.Width - 2, sprite.Bounds.Height - 2), colors[p]);

                    WarGame.Renderer.RgbaSpriteRenderer.DrawSprite(sprite, pos - offset);
                    var number = Convert.ToChar('A' + spawnPoints.IndexOf(p)).ToString();
                    var textOffset = spawnFont.Measure(number) / 2 + spawnLabelOffset;

                    spawnFont.DrawTextWithContrast(number, pos - textOffset, spawnColor, spawnContrastColor, 1);

                    if (((pos - GameViewPort.LastMousePos).ToVector2() / offset.ToVector2()).LengthSquared <= 1)
                        TooltipSpawnIndex = spawnPoints.IndexOf(p) + 1;
                }
            }
        }
    }
}