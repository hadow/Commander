using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
using EW.Mods.Common.Traits;
using EW.Widgets;

namespace EW.Mods.Common.Widgets.Logic
{
    public class CommandBarLogic:ChromeLogic
    {

        readonly World world;
        int selectionHash;

        Actor[] selectedActors = { };

        bool attackMoveDisabled = true;
        bool forceMoveDisabled = true;
        bool forceAttackDisabled = true;
        bool guardDisabled = true;
        bool scatterDisabled = true;
        bool waypointModeDisabled = true;


        [ObjectCreator.UseCtor]
        public CommandBarLogic(Widget widget,World world,Dictionary<string,MiniYaml> logicArgs)
        {
            this.world = world;

            var attackMoveButton = widget.GetOrNull<ButtonWidget>("ATTACK_MOVE");

            if(attackMoveButton != null)
            {
                BindButtonIcon(attackMoveButton);

            }

            var forceMoveButton = widget.GetOrNull<ButtonWidget>("FORCE_MOVE");
            if(forceMoveButton != null)
            {
                BindButtonIcon(forceMoveButton);
            }

            var forceAttackButton = widget.GetOrNull<ButtonWidget>("FORCE_ATTACK");
            if(forceAttackButton != null)
            {
                BindButtonIcon(forceAttackButton);
            }

            var guardButton = widget.GetOrNull<ButtonWidget>("GUARD");
            if(guardButton != null)
            {
                BindButtonIcon(guardButton);
            }


            var scatterButton = widget.GetOrNull<ButtonWidget>("SCATTER");
            if(scatterButton != null)
            {
                BindButtonIcon(scatterButton);
            }

            var deployButton = widget.GetOrNull<ButtonWidget>("DEPLOY");
            if (deployButton != null)
            {
                BindButtonIcon(deployButton);
            }


            var stopButton = widget.GetOrNull<ButtonWidget>("STOP");
            if(stopButton != null)
            {
                BindButtonIcon(stopButton);
            }
        }


        void BindButtonIcon(ButtonWidget button)
        {
            var icon = button.Get<ImageWidget>("ICON");

            var hasDisabled = ChromeProvider.GetImage(icon.ImageCollection, icon.ImageName + "-disabled") != null;
            var hasActive = ChromeProvider.GetImage(icon.ImageCollection, icon.ImageName + "-active") != null;

            icon.GetImageName = () => hasActive && button.IsHighlighted() ? icon.ImageName + "-active" : hasDisabled && button.IsDisabled() ? icon.ImageName + "-disabled" : icon.ImageName;

        }

    }
}