using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using EW.Framework;
using EW.Graphics;
using EW.Widgets;
namespace EW.Mods.Common.LoadScreens
{
    public sealed class LogoStripeLoadScreen:BlankLoadScreen
    {
        Stopwatch lastUpdate = Stopwatch.StartNew();
        Renderer r;

        Rectangle stripeRect;
        Vector2 logoPos;
        Sheet sheet;
        Sprite stripe, logo;
        string[] messages = { "Loading..." };

        public override void Init(ModData modData, Dictionary<string, string> info)
        {
            base.Init(modData, info);

            //Avoid standard loading mechanisms so we can display the loadscreen as early as possible.
            //避免使用标准的加载机制，以便尽可能早地显示加载画面。
            r = WarGame.Renderer;
            if (r == null)
                return;

            if (info.ContainsKey("Text"))
                messages = info["Text"].Split(',');

            if (info.ContainsKey("Image"))
            {
                using (var stream = modData.DefaultFileSystem.Open(info["Image"]))
                    sheet = new Sheet(SheetT.BGRA, stream);

                logo = new Sprite(sheet, new Rectangle(0, 0, 256, 256), TextureChannel.Alpha);
                stripe = new Sprite(sheet, new Rectangle(256, 0, 256, 256), TextureChannel.Alpha);
                stripeRect = new Rectangle(0, r.Resolution.Height / 2 - 128, r.Resolution.Width, 256);
                logoPos = new Vector2(r.Resolution.Width / 2 - 128, r.Resolution.Height / 2 - 128);
            }
        }


        public override void Display()
        {
            if (r == null)
                return;

            //Update text at most every 0.5 seconds
            if (lastUpdate.Elapsed.TotalSeconds < 0.5)
                return;
            if (r.Fonts == null)
                return;

            lastUpdate.Restart();
            var text = messages.Random(WarGame.CosmeticRandom);
            var textSize = r.Fonts["Bold"].Measure(text);

            r.BeginFrame(Int2.Zero, 1f);
            if (stripe != null)
            {
                WidgetUtils.FillRectWithSprite(stripeRect, stripe);
            }

            if (logo != null)
                r.RgbaSpriteRenderer.DrawSprite(logo, logoPos);


            r.Fonts["Bold"].DrawText(text, new Vector2(r.Resolution.Width - textSize.X - 20, r.Resolution.Height - textSize.Y - 20), Color.White);

            r.EndFrame();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && sheet != null)
                sheet.Dispose();

            base.Dispose(disposing);

        }
    }
}