using System;
using EW.Widgets;
namespace EW.Mods.Common.Widgets
{

    public enum TextAlign{Left,Center,Right}

    public enum TextVAlign{Top,Middle,Bottom}

    public class LabelWidget:Widget
    {
        public TextAlign Align = TextAlign.Left;

        public string Text = null;

        public string Font = ChromeMetrics.Get<string>("TextFont");

        public bool Contrast = ChromeMetrics.Get<bool>("TextContrast");


        public LabelWidget()
        {
        }


        public override void Draw()
        {
            base.Draw();
        }
    }
}
