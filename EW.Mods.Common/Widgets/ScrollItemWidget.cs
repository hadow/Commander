using System;
using EW.Widgets;

namespace EW.Mods.Common.Widgets
{
    public class ScrollItemWidget:ButtonWidget
    {
        public readonly string BaseName = "scrollitem";
        public string ItemKey;


        [ObjectCreator.UseCtor]
        public ScrollItemWidget(ModData modData)
            : base(modData)
        {
            IsVisible = () => false;
            VisualHeight = 0;
        }

        protected ScrollItemWidget(ScrollItemWidget other)
            : base(other)
        {
            IsVisible = () => false;
            VisualHeight = 0;
            BaseName = other.BaseName;
        }

        public Func<bool> IsSelected = () => false;

        public override void Draw()
        {

            var state = IsSelected() ? BaseName + "-selected" :
                false ? BaseName + "-hover" :
                null;

            if (state != null)
                WidgetUtils.DrawPanel(state, RenderBounds);
        }

        public override Widget Clone() { return new ScrollItemWidget(this); }


        public static ScrollItemWidget Setup(ScrollItemWidget template, Func<bool> isSelected, Action onClick)
        {
            var w = template.Clone() as ScrollItemWidget;
            w.IsVisible = () => true;
            w.IsSelected = isSelected;
            w.OnClick = onClick;
            return w;
        }

        public static ScrollItemWidget Setup(ScrollItemWidget template, Func<bool> isSelected, Action onClick, Action onDoubleClick)
        {
            var w = Setup(template, isSelected, onClick);
            //w.OnDoubleClick = onDoubleClick;
            return w;
        }

        public static ScrollItemWidget Setup(string key, ScrollItemWidget template, Func<bool> isSelected, Action onClick, Action onDoubleClick)
        {
            var w = Setup(template, isSelected, onClick);
            //w.OnDoubleClick = onDoubleClick;
            w.ItemKey = key;
            return w;
        }
    }
}