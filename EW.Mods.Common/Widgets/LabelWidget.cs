using System;
using System.Drawing;
using EW.Widgets;
using EW.Graphics;
using EW.Framework;
namespace EW.Mods.Common.Widgets
{

    public enum TextAlign{Left,Center,Right}

    public enum TextVAlign{Top,Middle,Bottom}

    public class LabelWidget:Widget
    {
        public TextAlign Align = TextAlign.Left;
        public TextVAlign VAlign = TextVAlign.Middle;
        public string Text = null;

        public string Font = ChromeMetrics.Get<string>("TextFont");
        public Color TextColor = ChromeMetrics.Get<Color>("TextColor");
        public bool Contrast = ChromeMetrics.Get<bool>("TextContrast");
        public bool Shadow = ChromeMetrics.Get<bool>("TextShadow");

        public Func<string> GetText;
        public Func<Color> GetColor;

        public LabelWidget()
        {
            GetText = () => Text;
            GetColor = () => TextColor;

        }


        public override void Draw()
        {
            SpriteFont font;
            if (!WarGame.Renderer.Fonts.TryGetValue(Font, out font))
                throw new ArgumentException("Requested font '{0}' was not found.".F(Font));

            var text = GetText();
            if (text == null)
                return;

            var textSize = font.Measure(text);
            var position = RenderOrigin;


            if (VAlign == TextVAlign.Middle)
                position += new Int2(0, (Bounds.Height - textSize.Y) / 2);

            if (VAlign == TextVAlign.Bottom)
                position += new Int2(0, Bounds.Height - textSize.Y);

            if (Align == TextAlign.Center)
                position += new Int2((Bounds.Width - textSize.X) / 2, 0);

            if (Align == TextAlign.Right)
                position += new Int2(Bounds.Width - textSize.X, 0);

            

            var color = GetColor();

            if (Contrast)
            {

            }
            else if (Shadow)
            {

            }
            else
            {
                font.DrawText(text, position, color);
            }
        }

        //public override Widget Clone()
        //{
        //    return new LabelWidget(this);
        //}
    }
}
