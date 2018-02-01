using System;
using EW.Support;
using EW.Widgets;

namespace EW.Mods.Common.Widgets.Logic
{
    public class PerfDebugLogic:ChromeLogic
    {

        [ObjectCreator.UseCtor]
        public PerfDebugLogic(Widget widget)
        {
            var perfGraph = widget.Get("GRAPH_BG");
            perfGraph.IsVisible = () => WarGame.Settings.Debug.PerfGraph;

            var perfText = widget.Get<LabelWidget>("PERF_TEXT");
            perfText.IsVisible = () => WarGame.Settings.Debug.PerfText;
            perfText.GetText = () =>
              "Tick {0} @ {1:F1} ms\nRender {2} @ {3:F1} ms\nBatches:{4}".F(
                  WarGame.LocalTick, PerfHistory.Items["tick_time"].Average(WarGame.Settings.Debug.Samples),
                  WarGame.RenderFrame, PerfHistory.Items["render"].Average(WarGame.Settings.Debug.Samples),
                  PerfHistory.Items["batches"].LastValue
                  );

        }

    }
}