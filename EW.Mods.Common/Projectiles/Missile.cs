using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Graphics;
using EW.Mods.Common.Graphics;
using EW.Mods.Common.Effects;
using EW.Mods.Common.Traits;
using EW.Traits;
namespace EW.Mods.Common.Projectiles
{
    public class MissileInfo : IProjectileInfo,IRulesetLoaded<WeaponInfo>
    {

        public readonly string Image = null;

        /// <summary>
        /// Loop a randomly chosen sequence of Image from this list while this projectile is moving.
        /// </summary>
        [SequenceReference("Image")]
        public readonly string[] Sequences = { "idle" };

        /// <summary>
        /// Image that contains the trail animation.
        /// </summary>
        public readonly string TrailImage = null;
        /// <summary>
        /// Palette used to render the projectile sequence.
        /// </summary>
        [PaletteReference]
        public readonly string Palette = "effect";

        public readonly bool Shadow = false;

        /// <summary>
        /// Width of projectile (used for finding blocking actors)
        /// </summary>
        public readonly WDist Width = new WDist(1);
        /// <summary>
        /// Run out of fuel after covering this distance.Zero for defaulting to weapon range.Negative for unlimited fuel.
        /// 覆盖这个距离后用尽燃料。
        /// </summary>
        public readonly WDist RangeLimit = WDist.Zero;

        /// <summary>
        /// Minimum vertical launch angle (pitch)
        /// </summary>
        public readonly WAngle MinimumLaunchAngle = new WAngle(-64);

        /// <summary>
        /// Maximum vertical launch angle (pitch)
        /// </summary>
        public readonly WAngle MaximumLaunchAngle = new WAngle(128);


        public readonly WDist MinimumLaunchSpeed = new WDist(-1);

        /// <summary>
        /// Maimum launch speed in WDist / tick. Defaults to Speed if -1.
        /// </summary>
        public readonly WDist MaximumLaunchSpeed = new WDist(-1);

        /// <summary>
        /// Maximum offset at the maximum range.
        /// </summary>
        public readonly WDist Inaccuracy = WDist.Zero;

        /// <summary>
        /// Maximum projectile speed in WDist / tick.
        /// </summary>
        public readonly WDist Speed = new WDist(384);

        /// <summary>
        /// Projectile acceleration when propulsion activated.
        /// 推进启动时的弹道加速
        /// </summary>
        public readonly WDist Acceleration = new WDist(5);

        /// <summary>
        /// Explodes when inside this proximity radius to target.
        /// Not: If this value is lower than the missile speed,this check might not trigger fast enough,causing the missile to fly past the target.
        /// </summary>
        public readonly WDist CloseEnough = new WDist(298);

        /// <summary>
        /// Horizontal rate of turn.
        /// 水平翻转率
        /// </summary>
        public readonly int HorizontalRateOfTurn = 5;

        /// <summary>
        /// Vertical rate of turn
        /// 垂直翻转率
        /// </summary>
        public readonly int VerticalRateOfTurn = 6;

        /// <summary>
        /// How many ticks before this missile is armed and can be explode.
        /// </summary>
        public readonly int Arm = 0;

        /// <summary>
        /// Altitude above terrain below which to explode.Zero effectively deactivates airburst.
        /// </summary>
        public readonly WDist AirburstAltitude = WDist.Zero;

        /// <summary>
        /// Is the missile aware of terrain height level.
        /// 导弹是否意识到地形高度水平
        /// </summary>
        public readonly bool TerrainHeightAware = false;
        /// <summary>
        /// Gravity applied while in free fall.
        /// </summary>
        public readonly int Gravity = 10;

        /// <summary>
        /// Probability of locking onto and following target.
        /// 锁定目标的可能性
        /// </summary>
        public readonly int LockOnProbability = 100;


        /// <summary>
        /// Use the Player Palette to render the trail sequence.
        /// </summary>
        public readonly bool TrailUsePlayerPalette = false;

        /// <summary>
        /// Palette used to render the trail sequence.
        /// </summary>
        [PaletteReference("TrailUsePlayerPalette")]
        public readonly string TrailPalette = "effect";

        /// <summary>
        /// Loop a randomly chosen sequence of TrailImage from this list while this projectile is moving.
        /// </summary>
        [SequenceReference("TrailImage")]
        public readonly string[] TrailSequences = { "idle" };

        /// <summary>
        /// Explodes when leaving the following terrain type, e.g.,Water for torpedoes.
        /// 当离开以下地形时爆破，例如鱼雷离开了水。
        /// </summary>
        public readonly string BoundToTerrainType = "";

        /// <summary>
        /// Interval in ticks between spawning trail animation.
        /// </summary>
        public readonly int TrailInterval = 2;

        public readonly int ContrailLength = 0;

        public readonly int ContrailZOffset = 2047;

        public readonly WDist ContrailWidth = new WDist(64);

        public readonly Color ContrailColor = Color.White;

        public readonly int ContrailDelay = 1;

        public readonly bool ContrailUsePlayerColor = false;

        /// <summary>
        /// Explode when running out of fuel.
        /// 当燃料耗尽时自动爆炸
        /// </summary>
        public readonly bool ExplodeWhenEmpty = true;

        /// <summary>
        /// Allow the missile to snap to the target,meaning jumping to the target immediately when the missile enters the radius of the current speed around the target.
        /// 允许导弹击中目标，
        /// </summary>
        public readonly bool AllowSnapping = false;

        /// <summary>
        /// Should trail animation be spawned when the propulsion is not activated.
        /// </summary>
        public readonly bool TrailWhenDeactivated = false;

        /// <summary>
        /// Activate homing mechanism after this many ticks;
        /// </summary>
        public readonly int HomingActivationDelay = 0;

        public WDist BlockerScanRadius = new WDist(-1);

        /// <summary>
        /// Is the missile blocked by actors with BlocksProjectiles:trait.
        /// </summary>
        public readonly bool Blockable = true;
        public IProjectile Create(ProjectileArgs args) { return new Missile(this,args); }

        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules, WeaponInfo info)
        {
            if (BlockerScanRadius < WDist.Zero)
                BlockerScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);
        }
    }

    /// <summary>
    /// 导弹
    /// </summary>
    public class Missile:IProjectile,ISync
    {
        enum States
        {
            Freefall,
            Homing,
            Hitting
        }

        readonly MissileInfo info;
        readonly ProjectileArgs args;
        readonly Animation anim;

        readonly WVec gravity;
        readonly int minLaunchSpeed;
        readonly int maxLaunchSpeed;
        readonly int maxSpeed;

        readonly WAngle minLaunchAngle;
        readonly WAngle maxLaunchAngle;

        int ticks;

        int ticksToNextSmoke;
        ContrailRenderable contrail;
        string trailPalette;

        States state;

        WPos targetPosition;
        WVec offset;

        bool targetPassedBy;
        bool lockOn = false;
        bool allowPassBy;

        int renderFacing;

        WDist rangeLimit;
        WDist distanceCovered;
        int loopRadius;
        int speed;
        WVec velocity;

        WVec tarVel;
        WVec predVel;

        [Sync]
        WPos pos;

        [Sync]
        int hFacing;
        [Sync]
        int vFacing;

        public Missile(MissileInfo info,ProjectileArgs args)
        {
            this.info = info;
            this.args = args;

            pos = args.Source;
            hFacing = args.Facing;
            gravity = new WVec(0, 0, -info.Gravity);
            targetPosition = args.PassiveTarget;

            rangeLimit = info.RangeLimit != WDist.Zero ? info.RangeLimit : args.Weapon.Range;

            minLaunchSpeed = info.MinimumLaunchSpeed.Length > -1 ? info.MinimumLaunchSpeed.Length : info.Speed.Length;
            maxLaunchSpeed = info.MaximumLaunchSpeed.Length > -1 ? info.MaximumLaunchSpeed.Length : info.Speed.Length;

            maxSpeed = info.Speed.Length;

            minLaunchAngle = info.MinimumLaunchAngle;
            maxLaunchAngle = info.MaximumLaunchAngle;
            
            var world = args.SourceActor.World;

            if (info.Inaccuracy.Length > 0)
            {
                var inaccuracy = Util.ApplyPercentageModifiers(info.Inaccuracy.Length, args.InaccuracyModifiers);
                offset = WVec.FromPDF(world.SharedRandom, 2) * inaccuracy / 1024;
            }


            DetermineLaunchSpeedAndAngle(world, out speed, out vFacing);

            velocity = new WVec(0, -speed, 0)
                .Rotate(new WRot(WAngle.FromFacing(vFacing), WAngle.Zero, WAngle.Zero))
                .Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(hFacing)));

            if (world.SharedRandom.Next(100) <= info.LockOnProbability)
                lockOn = true;

            

            if (!string.IsNullOrEmpty(info.Image))
            {
                anim = new Animation(world, info.Image, () =>renderFacing);
                anim.PlayRepeating(info.Sequences.Random(world.SharedRandom));
            }

            if (info.ContrailLength > 0)
            {
                var color = info.ContrailUsePlayerColor ? ContrailRenderable.ChooseColor(args.SourceActor) : info.ContrailColor;
                contrail = new ContrailRenderable(world, color, info.ContrailWidth, info.ContrailLength, info.ContrailDelay, info.ContrailZOffset);
            }

            trailPalette = info.TrailPalette;
            if (info.TrailUsePlayerPalette)
                trailPalette += args.SourceActor.Owner.InternalName;

        }


        /// <summary>
        /// 判定导弹启动速度和角度
        /// </summary>
        /// <param name="world"></param>
        /// <param name="speed"></param>
        /// <param name="vFacing"></param>
        void DetermineLaunchSpeedAndAngle(World world,out int speed,out int vFacing)
        {
            speed = maxLaunchSpeed;
            loopRadius = LoopRadius(speed, info.VerticalRateOfTurn);

            //Compute current distance from target position
            var tarDistVec = targetPosition + offset - pos;
            var relTarHorDist = tarDistVec.HorizontalLength;

            int predClfHgt = 0;
            int predClfDist = 0;
            int lastHtChg = 0;
            int lastHt = 0;

            if(info.TerrainHeightAware)
            {
                InclineLookahead(world, relTarHorDist, out predClfHgt, out predClfDist, out lastHtChg, out lastHt);
            }
            //Height difference between the incline height and missile height
            //倾斜高度和导弹高度之间的高度差。
            var diffClfMslHgt = predClfHgt - pos.Z;


            //Incline coming up
            if(info.TerrainHeightAware && diffClfMslHgt >=0 && predClfDist>0)
            {
                DetermineLaunchSpeedAndAngleForIncline(predClfDist, diffClfMslHgt, relTarHorDist, out speed, out vFacing);
            }
            else if (lastHt != 0)
            {
                vFacing = Math.Max((sbyte)(minLaunchAngle.Angle >> 2), (sbyte)0);
                speed = maxLaunchSpeed;
            }
            else
            {

                //Set vertical facing so that the missile faces its target
                var vDist = new WVec(-tarDistVec.Z, -relTarHorDist, 0);
                vFacing = (sbyte)vDist.Yaw.Facing;

                //Do not accept -1 as a valid vertical facing since it is usually a numerical error
                //and will lead to premature descent and crashing into the ground.
                //不要接受-1作为有效的垂直面向，因为它通常是一个数字错误 并且会导致过早下降并坠入地面。
                if (vFacing == -1)
                    vFacing = 0;

                //Make sure the chosen vertical facing adheres to prescribed bounds
                //确保所选的垂直面向符合规定的边界
                vFacing = vFacing.Clamp((sbyte)(minLaunchAngle.Angle >> 2), (sbyte)(maxLaunchAngle.Angle >> 2));
            }


        }


        void DetermineLaunchSpeedAndAngleForIncline(int predClfDist,int diffClfMslHgt,int relTarHorDist,out int speed,out int vFacing)
        {
            speed = maxLaunchSpeed;

            vFacing = maxLaunchAngle.Angle >> 2;

            var minSpeed = ((Math.Min(predClfDist * 1024 / (1024 - WAngle.FromFacing(vFacing).Sin()),
                (relTarHorDist + predClfDist) * 1024 / (2 * (2048 - WAngle.FromFacing(vFacing).Sin())))
                * info.VerticalRateOfTurn * 157) / 6400).Clamp(minLaunchSpeed, maxLaunchSpeed);

            if ((sbyte)vFacing < 0)
                speed = minSpeed;
            else if(!WillClimbWithinDistance(vFacing,loopRadius,predClfDist,diffClfMslHgt)
                && !WillClimbAroundInclineTop(vFacing,loopRadius,predClfDist,diffClfMslHgt,speed))
            {
                var vFac = vFacing;
                speed = BisectionSearch(minSpeed, maxLaunchSpeed, spd =>
                {
                    var lpRds = LoopRadius(spd, info.VerticalRateOfTurn);
                    return WillClimbWithinDistance(vFac, lpRds, predClfDist, diffClfMslHgt) || WillClimbAroundInclineTop(vFac, lpRds, predClfDist, diffClfMslHgt, spd);
                });
            }
            else
            {
                vFacing = BisectionSearch(Math.Max((sbyte)(minLaunchAngle.Angle >> 2), (sbyte)0),
                    (sbyte)(maxLaunchAngle.Angle >> 2),
                    vFac => !WillClimbWithinDistance(vFac, loopRadius, predClfDist, diffClfMslHgt)) + 1;
            }
        }

        /// <summary>
        /// It might be desirable to make lookahead more intelligent by outputting more information
        /// than just the highest point in the lookahead distance.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="distCheck"></param>
        /// <param name="predClfHgt"></param>
        /// <param name="predClfDist"></param>
        /// <param name="lastHtChg"></param>
        /// <param name="lastHt"></param>
        void InclineLookahead(World world,int distCheck,out int predClfHgt,out int predClfDist,out int lastHtChg,out int lastHt)
        {
            predClfHgt = 0;// Highest probed terrain height.最高的探测地形高度
            predClfDist = 0;//Distance from highest point
            lastHtChg = 0;//Distance from last time the height changes//与上次高度变化的距离
            lastHt = 0;//Height just before the last height change

            //
            var stepSize = 32;
            var step = new WVec(0, -stepSize, 0).Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(hFacing)));

            //Probe terrain ahead of the missile
            //探测导弹前方的地形
            var maxLookaheadDistance = loopRadius * 4;
            var posProbe = pos;
            var curDist = 0;
            var tickLimit = Math.Min(maxLookaheadDistance, distCheck) / stepSize;
            var prevHt = 0;

            for(var tick = 0;tick<= tickLimit; tick++)
            {
                posProbe += step;
                if (!world.Map.Contains(world.Map.CellContaining(posProbe)))
                    break;

                var ht = world.Map.Height[world.Map.CellContaining(posProbe)] * 512;

                curDist += stepSize;

                if(ht> predClfHgt)
                {
                    predClfHgt = ht;
                    predClfDist = curDist;
                }

                if(prevHt != ht)
                {
                    lastHtChg = curDist;
                    lastHt = prevHt;
                    prevHt = ht;
                }
            }
        }

        static bool WillClimbWithinDistance(int vFacing,int loopRadius,int predClfDist,int diffClfMslHgt)
        {
            //Missile's horizontal distance from loop's center.
            var missDist = loopRadius * WAngle.FromFacing(vFacing).Sin() / 1024;

            //Missile's height below loop's top
            var missHgt = loopRadius * (1024 - WAngle.FromFacing(vFacing).Cos()) / 1024;

            var hgtChg = (predClfDist - missDist) * WAngle.FromFacing(vFacing).Tan() / 1024;

            //Check if total manoeuvre height enough to overcome the incline's height.
            return hgtChg + missHgt >= diffClfMslHgt;

        }

        static bool WillClimbAroundInclineTop(int vFacing,int loopRadius,int predClfDist,int diffClfMslHgt,int speed)
        {
            var radius = new WVec(loopRadius, 0, 0).Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(Math.Max(0, 64 - vFacing))));

            var topVector = new WVec(predClfDist, diffClfMslHgt + 64, 0) - radius;

            //Check if incline top inside of the vertical loop.
            return topVector.Length <= loopRadius;
        }

        static int LoopRadius(int speed,int rot)
        {
            return (speed * 6400) / (157 * rot);
        }


        static int BisectionSearch(int lowerBound,int upperBound,Func<int,bool> testCriterion)
        {
            while(upperBound - lowerBound > 1)
            {
                var middle = (upperBound + lowerBound) / 2;

                if (testCriterion(middle))
                    lowerBound = middle;
                else
                    upperBound = middle;
            }
            return lowerBound;
        }

        public void Tick(World world)
        {
            ticks++;
            if (anim != null)
                anim.Tick();

            //Switch from freefall mode to homing mode
            if(ticks == info.HomingActivationDelay + 1)
            {
                state = States.Homing;
                speed = velocity.Length;

                //Compute the vertical loop radius.
                loopRadius = LoopRadius(speed, info.VerticalRateOfTurn);
            }

            //Switch from homing mode to freefall mode
            if(rangeLimit >= WDist.Zero && distanceCovered > rangeLimit)
            {
                state = States.Freefall;
                velocity = new WVec(0, -speed, 0)
                    .Rotate(new WRot(WAngle.FromFacing(vFacing), WAngle.Zero, WAngle.Zero))
                    .Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(hFacing)));
            }

            //Check if target position should be updated (actor visible & locked on)
            var newTarPos = targetPosition;
            if (args.GuidedTarget.IsValidFor(args.SourceActor) && lockOn)
                newTarPos = (args.Weapon.TargetActorCenter ? args.GuidedTarget.CenterPosition : args.GuidedTarget.Positions.PositionClosestTo(args.Source)) 
                    + new WVec(WDist.Zero, WDist.Zero, info.AirburstAltitude);


            //Compute target's predicted velocity vector (assuming uniform circular motion)
            var yaw1 = tarVel.HorizontalLengthSquared != 0 ? tarVel.Yaw : WAngle.FromFacing(hFacing);
            tarVel = newTarPos - targetPosition;
            var yaw2 = tarVel.HorizontalLengthSquared != 0 ? tarVel.Yaw : WAngle.FromFacing(hFacing);
            predVel = tarVel.Rotate(WRot.FromYaw(yaw2 - yaw1));
            targetPosition = newTarPos;

            //Comput current distance from target position
            var tarDistVec = targetPosition + offset - pos;
            var relTarDist = tarDistVec.Length;
            var relTarHorDist = tarDistVec.HorizontalLength;

            WVec move;
            if (state == States.Freefall)
                move = FreefallTick();
            else
                move = HomingTick(world, tarDistVec, relTarHorDist);

            renderFacing = new WVec(move.X, move.Y - move.Z, 0).Yaw.Facing;


            //Move the missile
            var lastPos = pos;
            if (info.AllowSnapping && state != States.Freefall && relTarDist < move.Length)
                pos = targetPosition + offset;
            else
                pos += move;

            //Check for walls or other blocking obstacles
            var shouldExplode = false;
            WPos blockedPos;
            if(info.Blockable && BlocksProjectiles.AnyBlockingActorsBetween(world,lastPos,pos,info.Width,info.BlockerScanRadius,out blockedPos))
            {
                pos = blockedPos;
                shouldExplode = true;
            }


            //Create the sprite trail effect
            if(!string.IsNullOrEmpty(info.TrailImage) && --ticksToNextSmoke <0 && (state != States.Freefall || info.TrailWhenDeactivated))
            {
                world.AddFrameEndTask(w => w.Add(new SpriteEffect(pos - 3 * move / 2, w, info.TrailImage, info.TrailSequences.Random(world.SharedRandom), trailPalette, false, false, renderFacing)));

                ticksToNextSmoke = info.TrailInterval;
            }

            if (info.ContrailLength > 0)
                contrail.Update(pos);

            distanceCovered += new WDist(speed);
            var cell = world.Map.CellContaining(pos);
            var height = world.Map.DistanceAboveTerrain(pos);
            shouldExplode |= height.Length < 0 //hit the ground
                || relTarDist < info.CloseEnough.Length //Within range
                || (info.ExplodeWhenEmpty && rangeLimit >= WDist.Zero && distanceCovered > rangeLimit)//Ran out of fuel
                || !world.Map.Contains(cell) //
                || (!string.IsNullOrEmpty(info.BoundToTerrainType) && world.Map.GetTerrainInfo(cell).Type != info.BoundToTerrainType)//Hit incompatible terrain
                || (height.Length < info.AirburstAltitude.Length && relTarHorDist < info.CloseEnough.Length);//Airburst

            if (shouldExplode)
                Explode(world);
            
        }

        void Explode(World world)
        {
            if (info.ContrailLength > 0)
                world.AddFrameEndTask(w => w.Add(new ContrailFader(pos, contrail)));
            world.AddFrameEndTask(w => w.Remove(this));

            if (ticks <= info.Arm)
                return;

            args.Weapon.Impact(Target.FromPos(pos), args.SourceActor, args.DamagedModifiers);
        }

        WVec HomingTick(World world,WVec tarDistVec,int relTarHorDist)
        {
            return new WVec(0, -1024 * speed, 0)
                .Rotate(new WRot(WAngle.FromFacing(vFacing), WAngle.Zero, WAngle.Zero))
                .Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(hFacing))) / 1024;
        }

        WVec FreefallTick()
        {
            //Compute the projectile's freefall displacement
            var move = velocity + gravity / 2;
            velocity += gravity;
            var velRatio = maxSpeed * 1024 / velocity.Length;
            if (velRatio < 1024)
                velocity = velocity * velRatio / 1024;

            return move;
        }


        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            if (info.ContrailLength > 0)
                yield return contrail;

            if (anim == null)
                yield break;

            var world = args.SourceActor.World;

            if (!world.FogObscures(pos))
            {
                if (info.Shadow)
                {
                    var dat = world.Map.DistanceAboveTerrain(pos);
                    var shadowPos = pos - new WVec(0, 0, dat.Length);
                    foreach (var r in anim.Render(shadowPos, wr.Palette("shadow")))
                        yield return r;
                }

                var palette = wr.Palette(info.Palette);
                foreach (var r in anim.Render(pos, palette))
                    yield return r;
            }
        }



    }
}