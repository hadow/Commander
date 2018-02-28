using System;
using System.Drawing;
using EW.Framework.Touch;
using EW.Widgets;
using EW.Framework;
namespace EW.Mods.Common.Widgets
{
    public class ButtonWidget:Widget
    {
        [Translate]
        public string Text = "";

        public bool Depressed = false;

        public string Background = "button";

        public string Font = ChromeMetrics.Get<string>("ButtonFont");
        public int VisualHeight = ChromeMetrics.Get<int>("ButtonDepth");
        public int BaseLine = ChromeMetrics.Get<int>("ButtonBaseLine");
        public Color TextColor = ChromeMetrics.Get<Color>("ButtonTextColor");

        public Color TextColorDisabled = ChromeMetrics.Get<Color>("ButtonTextColorDisabled");

        public bool Contrast = ChromeMetrics.Get<bool>("ButtonTextContrast");

        public bool Shadow = ChromeMetrics.Get<bool>("ButtonTextShadow");

        public Color ContrastColorDark = ChromeMetrics.Get<Color>("ButtonTextContrastColorDark");

        public Color ContrastColorLight = ChromeMetrics.Get<Color>("ButtonTextContrastColorLight");

        public bool Disabled = false;

        public bool Highlighted = true;

        public Func<string> GetText;
        public Func<Color> GetColor;
        public Func<Color> GetColorDisabled;
        public Func<bool> IsDisabled;
        public Func<bool> IsHighlighted;
        public Action OnClick = () => { };
        public Action<GestureSample> OnMouseUp = _ => { };
        protected readonly Ruleset ModRules;


        [ObjectCreator.UseCtor]
        public ButtonWidget(ModData modData)
        {
            ModRules = modData.DefaultRules;

            GetText = () => Text;
            GetColor = () => TextColor;
            OnMouseUp = _ => OnClick();
            GetColorDisabled = () => TextColorDisabled;
            IsHighlighted = () => Highlighted;
            IsDisabled = () => Disabled;
        }


        public override bool YieldFocus(GestureSample gs)
        {
            Depressed = false;
            return base.YieldFocus(gs);
        }


        public override bool HandleInput(GestureSample gs)
        {
            if (gs.GestureType == GestureType.Tap && !TakeFocus(gs))
                return false;

            var disabled = IsDisabled();
            if(HasFocus && gs.GestureType == GestureType.DoubleTap)
            {
                if (!disabled)
                {

                    return YieldFocus(gs);
                }
            }
            else if(HasFocus && gs.GestureType == GestureType.Tap)
            {
                if (!disabled)
                    OnMouseUp(gs);
                return YieldFocus(gs);
            }

            //if (gs.GestureType == GestureType.Tap)
            //{
            //    if (!disabled)
            //    {
            //        Depressed = true;
            //        WarGame.Sound.PlayNotification(ModRules, null, "Sounds", "ClickSound", null);
            //    }
            //    else
            //    {
            //        YieldFocus(gs);
            //    }
            //}
            //else if (gs.GestureType == GestureType.FreeDrag)
            //    Depressed = RenderBounds.Contains(gs.Position.ToInt2());

            return Depressed;
        }

        public virtual int UsableWidth { get { return Bounds.Width; } }

        public override void Draw()
        {
            var rb = RenderBounds;
            var disabled = IsDisabled();
            var highlighted = IsHighlighted();
            var font = WarGame.Renderer.Fonts[Font];
            var text = GetText();
            var color = GetColor();
            var colorDisabled = GetColorDisabled();
            var s = font.Measure(text);
            var stateOffset = Depressed ? new Int2(VisualHeight, VisualHeight) : Int2.Zero;
            var position = new Int2(rb.X + (UsableWidth - s.X) / 2, rb.Y - BaseLine + (Bounds.Height - s.Y) / 2);

            DrawBackground(rb, disabled, Depressed, false, highlighted);

            font.DrawText(text, position + stateOffset, disabled ? colorDisabled : color);
        }

        public virtual void DrawBackground(Rectangle rect,bool disabled,bool pressed,bool hover=false,bool highlighted= false)
        {
            DrawBackground(Background, rect, disabled, pressed, hover, highlighted);
        }



        public static void DrawBackground(string baseName,Rectangle rect,bool disabled,bool pressed,bool hover = false,bool highlighted = false)
        {
            var variant = highlighted ? "-highlighted" : "";
            var state = disabled ? "-disabled" :
                pressed ? "-pressed" :
                hover ? "-hover" : "";

            WidgetUtils.DrawPanel(baseName + variant + state, rect);
        }
    }
}