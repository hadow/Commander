using System;
using System.Drawing;
using System.Linq;
using EW.Graphics;
using EW.Primitives;
using EW.Widgets;
using EW.Framework;
using EW.Framework.Touch;
namespace EW.Mods.Common.Widgets
{

    public interface ILayout
    {
        void AdjustChild(Widget w);
        void AdjustChildren();
    }

    public enum ScrollPanelAlign
    {
        Bottom,
        Top
    }

    public class ScrollPanelWidget:Widget
    {


        readonly Ruleset modRules;
        public int ScrollbarWidth = 24;
        public int BorderWidth = 1;
        public int TopBottomSpacing = 2;
        public int ItemSpacing = 0;
        public int ButtonDepth = ChromeMetrics.Get<int>("ButtonDepth");
        public string Background = "scrollpanel-bg";
        public string Button = "scrollpanel-button";
        public int ContentHeight;

        public ILayout Layout;
        public int MinimumThumbSize = 10;
        public ScrollPanelAlign Align = ScrollPanelAlign.Top;
        public bool CollapseHiddenChildren;
        public float SmoothScrollSpeed = 0.333f;

        protected bool upPressed;
        protected bool downPressed;
        protected bool upDisabled;
        protected bool downDisabled;
        protected bool thumbPressed;
        protected Rectangle upButtonRect;
        protected Rectangle downButtonRect;
        protected Rectangle backgroundRect;
        protected Rectangle scrollbarRect;
        protected Rectangle thumbRect;


        // The target value is the list offset we're trying to reach
        float targetListOffset;

        // The current value is the actual list offset at the moment
        float currentListOffset;

        [ObjectCreator.UseCtor]
        public ScrollPanelWidget(ModData modData)
        {
            this.modRules = modData.DefaultRules;

            Layout = new ListLayout(this);
        }

        public override void DrawOuter()
        {
            if (!IsVisible())
                return;

            var rb = RenderBounds;

            var scrollbarHeight = rb.Height - 2 * ScrollbarWidth;

            var thumbHeight = ContentHeight == 0 ? 0 : Math.Max(MinimumThumbSize, (int)(scrollbarHeight * Math.Min(rb.Height * 1f / ContentHeight, 1f)));
            var thumbOrigin = rb.Y + ScrollbarWidth + (int)((scrollbarHeight - thumbHeight) * (-1f * currentListOffset / (ContentHeight - rb.Height)));
            if (thumbHeight == scrollbarHeight)
                thumbHeight = 0;

            backgroundRect = new Rectangle(rb.X, rb.Y, rb.Width - ScrollbarWidth + 1, rb.Height);
            upButtonRect = new Rectangle(rb.Right - ScrollbarWidth, rb.Y, ScrollbarWidth, ScrollbarWidth);
            downButtonRect = new Rectangle(rb.Right - ScrollbarWidth, rb.Bottom - ScrollbarWidth, ScrollbarWidth, ScrollbarWidth);
            scrollbarRect = new Rectangle(rb.Right - ScrollbarWidth, rb.Y + ScrollbarWidth - 1, ScrollbarWidth, scrollbarHeight + 2);
            thumbRect = new Rectangle(rb.Right - ScrollbarWidth, thumbOrigin, ScrollbarWidth, thumbHeight);
            
            WidgetUtils.DrawPanel(Background, backgroundRect);
            WidgetUtils.DrawPanel(Background, scrollbarRect);
            ButtonWidget.DrawBackground(Button, upButtonRect, upDisabled, upPressed, false, false);
            ButtonWidget.DrawBackground(Button, downButtonRect, downDisabled, downPressed, false, false);

            if (thumbHeight > 0)
                ButtonWidget.DrawBackground(Button, thumbRect, false, false, false);

            var upOffset = !upPressed || upDisabled ? 4 : 4 + ButtonDepth;
            var downOffset = !downPressed || downDisabled ? 4 : 4 + ButtonDepth;

            WidgetUtils.DrawRGBA(ChromeProvider.GetImage("scrollbar", upPressed || upDisabled ? "up_pressed" : "up_arrow"),
                new Vector2(upButtonRect.Left + upOffset, upButtonRect.Top + upOffset));
            WidgetUtils.DrawRGBA(ChromeProvider.GetImage("scrollbar", downPressed || downDisabled ? "down_pressed" : "down_arrow"),
                new Vector2(downButtonRect.Left + downOffset, downButtonRect.Top + downOffset));

            var drawBounds = backgroundRect.InflateBy(-BorderWidth, -BorderWidth, -BorderWidth, -BorderWidth);
            WarGame.Renderer.EnableScissor(drawBounds);

            drawBounds.Offset((-ChildOrigin).ToPoint());
            foreach (var child in Children)
                if (child.Bounds.IntersectsWith(drawBounds))
                    child.DrawOuter();

            WarGame.Renderer.DisableScissor();
        }

        public override void Tick()
        {
            if (upPressed)
                Scroll(1);

            if (downPressed)
                Scroll(-1);

            var offsetDiff = targetListOffset - currentListOffset;
            var absOffsetDiff = Math.Abs(offsetDiff);
            if (absOffsetDiff > 1f)
            {
                currentListOffset += offsetDiff * SmoothScrollSpeed.Clamp(0.1f, 1.0f);

                //Ui.ResetTooltips();
            }
            else
                SetListOffset(targetListOffset, false);
        }

        public override Int2 ChildOrigin { get { return RenderOrigin + new Int2(0, (int)currentListOffset); } }

        public override Rectangle GetEventBounds()
        {
            return EventBounds;
        }

        public void ScrollToItem(string itemKey, bool smooth = false)
        {
            var item = Children.FirstOrDefault(c =>
            {
                var si = c as ScrollItemWidget;
                return si != null && si.ItemKey == itemKey;
            });

            if (item != null)
                ScrollToItem(item, smooth);
        }

        void ScrollToItem(Widget item, bool smooth = false)
        {
            // Scroll the item to be visible
            float? newOffset = null;
            if (item.Bounds.Top + currentListOffset < 0)
                newOffset = ItemSpacing - item.Bounds.Top;

            if (item.Bounds.Bottom + currentListOffset > RenderBounds.Height)
                newOffset = RenderBounds.Height - item.Bounds.Bottom - ItemSpacing;

            if (newOffset.HasValue)
                SetListOffset(newOffset.Value, smooth);
        }

        public override bool YieldFocus(GestureSample gs)
        {
            upPressed = downPressed = thumbPressed = false;
            return base.YieldFocus(gs);
        }

        public override void RemoveChildren()
        {
            ContentHeight = 0;
            base.RemoveChildren();
        }
        

        public override void AddChild(Widget child)
        {
            // Initial setup of margins/height
            Layout.AdjustChild(child);
            base.AddChild(child);
        }

        public override void RemoveChild(Widget child)
        {
            base.RemoveChild(child);
            Layout.AdjustChildren();
            Scroll(0);
        }


        void Scroll(int amount, bool smooth = false)
        {
            var newTarget = targetListOffset + amount * WarGame.Settings.Game.UIScrollSpeed;
            newTarget = Math.Min(0, Math.Max(Bounds.Height - ContentHeight, newTarget));

            SetListOffset(newTarget, smooth);
        }

        // Setting "smooth" to true will only update the target list offset.
        // Setting "smooth" to false will also set the current list offset,
        // i.e. it will scroll immediately.
        //
        // For example, scrolling with the mouse wheel will use smooth
        // scrolling to give a nice visual effect that makes it easier
        // for the user to follow. Dragging the scrollbar's thumb, however,
        // will scroll to the desired position immediately.
        protected void SetListOffset(float value, bool smooth)
        {
            targetListOffset = value;
            if (!smooth)
            {
                var oldListOffset = currentListOffset;
                currentListOffset = value;

                //// Update mouseover
                //if (oldListOffset != currentListOffset)
                //    Ui.ResetTooltips();
            }
        }
    }
}