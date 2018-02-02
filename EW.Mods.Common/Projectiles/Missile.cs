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
        /// Cruise altitude.Zero means no cruise altitude used.
        /// 巡航高度，0表示 没有巡航高度
        /// </summary>
        public readonly WDist CruiseAltitude = new WDist(512);

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
        /// 自由落体时应用的重力
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

        public readonly bool Jammable = true;

        /// <summary>
        /// Range of facings by which jammed missiles can stray from current path
        /// 卡住的导弹可以从目前的路径偏离的范围
        /// </summary>
        public readonly int JammedDiversionRange = 20;

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
            //发射速率
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
            //计算目标位置的当前距离
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predClfDist"></param>
        /// <param name="diffClfMslHgt"></param>
        /// <param name="relTarHorDist"></param>
        /// <param name="speed"></param>
        /// <param name="vFacing"></param>
        void DetermineLaunchSpeedAndAngleForIncline(int predClfDist,int diffClfMslHgt,int relTarHorDist,out int speed,out int vFacing)
        {
            speed = maxLaunchSpeed;

            //Find smallest vertical facing, for which the missile will be able to climb terrAltDiff w-units
            //within hHeightChange w-units all the while ending the ascent with vertical facing 0

            vFacing = maxLaunchAngle.Angle >> 2;

            //Compute minimum sspeed necessary to both be able to face directly upwards and have enough space to hit the target without passing it by (and thus having to do horizontal loops)
            //计算必要的最低速度,既能够直接面向上方，并有足够的空间击中目标而不经过(因此必须做横向循环检查)
            var minSpeed = ((Math.Min(predClfDist * 1024 / (1024 - WAngle.FromFacing(vFacing).Sin()),
                (relTarHorDist + predClfDist) * 1024 / (2 * (2048 - WAngle.FromFacing(vFacing).Sin())))
                * info.VerticalRateOfTurn * 157) / 6400).Clamp(minLaunchSpeed, maxLaunchSpeed);

            if ((sbyte)vFacing < 0)
                speed = minSpeed;
            else if(!WillClimbWithinDistance(vFacing,loopRadius,predClfDist,diffClfMslHgt)
                && !WillClimbAroundInclineTop(vFacing,loopRadius,predClfDist,diffClfMslHgt,speed))
            {
                //Find highest speed greater than the above minimum that allows the missile to surmount the incline.
                //发现最大速度大于上述允许导弹超越倾斜的最小值
                var vFac = vFacing;
                speed = BisectionSearch(minSpeed, maxLaunchSpeed, spd =>
                {
                    var lpRds = LoopRadius(spd, info.VerticalRateOfTurn);
                    return WillClimbWithinDistance(vFac, lpRds, predClfDist, diffClfMslHgt) 
                    || WillClimbAroundInclineTop(vFac, lpRds, predClfDist, diffClfMslHgt, spd);
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
        /// 通过输出更多的信息而不仅仅是向前看距离中的最高点，使得向前看起来更加智能化可能是可取的。
        /// </summary>
        /// <param name="world"></param>
        /// <param name="distCheck"></param>
        /// <param name="predClfHgt"></param>
        /// <param name="predClfDist"></param>
        /// <param name="lastHtChg"></param>
        /// <param name="lastHt"></param>
        void InclineLookahead(World world,int distCheck,out int predClfHgt,out int predClfDist,out int lastHtChg,out int lastHt)
        {
            predClfHgt = 0;// Highest probed terrain height.    //最高的探测地形高度
            predClfDist = 0;//Distance from highest point       //距最高点的距离
            lastHtChg = 0;//Distance from last time the height changes//与上次高度变化的距离
            lastHt = 0;//Height just before the last height change      //最后一次高度变化 之前的高度

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
            //Make sure cell on map !!!
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

        /// <summary>
        /// Will missile be able to climb  terrAltDiff w-units within hHeightChange w-units all the while ending the ascent with veritcal facing 0
        /// Calling this function only makes sense when vFacing is nonnegative.
        /// 导弹能否在一定的范围内持续攀升
        /// </summary>
        /// <param name="vFacing"></param>
        /// <param name="loopRadius"></param>
        /// <param name="predClfDist"></param>
        /// <param name="diffClfMslHgt"></param>
        /// <returns></returns>
        static bool WillClimbWithinDistance(int vFacing,int loopRadius,int predClfDist,int diffClfMslHgt)
        {
            //Missile's horizontal distance from loop's center.
            var missDist = loopRadius * WAngle.FromFacing(vFacing).Sin() / 1024;

            //Missile's height below loop's top 导弹的高度低于循环的顶部
            var missHgt = loopRadius * (1024 - WAngle.FromFacing(vFacing).Cos()) / 1024;

            //Height that would be climbed without changing vertical facing for a horizontal distance hHeightChange - missDist
            //在不改变水平距离的情况下攀升的高度
            var hgtChg = (predClfDist - missDist) * WAngle.FromFacing(vFacing).Tan() / 1024;

            //Check if total manoeuvre height enough to overcome the incline's height.
            //检查总的操纵高度是否足以克服倾斜的高度
            return hgtChg + missHgt >= diffClfMslHgt;

        }

        /// <summary>
        /// Will missile climb around incline top if bringing vertical facing
        /// down to zero on an arc of radius loopRadius
        /// Calling this function only makes sense when IsNearInclineTop return true
        /// 
        /// </summary>
        /// <param name="vFacing"></param>
        /// <param name="loopRadius"></param>
        /// <param name="predClfDist"></param>
        /// <param name="diffClfMslHgt"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        static bool WillClimbAroundInclineTop(int vFacing,int loopRadius,int predClfDist,int diffClfMslHgt,int speed)
        {
            //Vector from missile's current position pointing to the loop's center.

            var radius = new WVec(loopRadius, 0, 0).Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(Math.Max(0, 64 - vFacing))));

            // Vector from loop's center to incline top +64 hardcoded in height buffer zone
            //在循环的中心向上倾斜顶部+64 硬编码的高度缓冲区
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
            // Assuming that there exists an integer N between lowerBound and upperBound
            // for which testCriterion returns true as well as all integers less than N,
            // and for which testCriterion returns false for all integers greater than N,
            // this function finds N.
            //
            while (upperBound - lowerBound > 1)
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
            //计算目标的预测速度向量(假设均匀的圆周运动)
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
            int predClfHgt = 0;
            int predClfDist = 0;
            int lastHtChg = 0;
            int lastHt = 0;

            if (info.TerrainHeightAware)
                InclineLookahead(world, relTarHorDist, out predClfHgt, out predClfDist, out lastHtChg, out lastHt);

            var diffClfMslHgt = predClfHgt - pos.Z;


            var nxtRelTarHorDist = (relTarHorDist - speed - info.Acceleration.Length).Clamp(0, relTarHorDist);

            //Target height relative to the missile
            var relTarHgt = tarDistVec.Z;

            //Compute which direction the projectile should be facing
            var velVec = tarDistVec + predVel;

            var desiredHFacing = velVec.HorizontalLengthSquared != 0 ? velVec.Yaw.Facing : hFacing;

            var delta = Util.NormalizeFacing(hFacing - desiredHFacing);

            if (allowPassBy && delta > 64 && delta < 192)
            {
                desiredHFacing = (desiredHFacing + 128) & 0xFF;
                targetPassedBy = true;
            }
            else
                targetPassedBy = false;

            var desiredVFacing = HomingInnerTick(predClfDist, diffClfMslHgt, relTarHorDist, lastHtChg, lastHt,
                nxtRelTarHorDist, relTarHgt, vFacing, targetPassedBy);

            // The target has been passed by
            if (tarDistVec.HorizontalLength < speed * WAngle.FromFacing(vFacing).Cos() / 1024)
                targetPassedBy = true;

            // Check whether the homing mechanism is jammed
            var jammed = info.Jammable && world.ActorsWithTrait<JamsMissiles>().Any(JammedBy);
            if (jammed)
            {
                desiredHFacing = hFacing + world.SharedRandom.Next(-info.JammedDiversionRange, info.JammedDiversionRange + 1);
                desiredVFacing = vFacing + world.SharedRandom.Next(-info.JammedDiversionRange, info.JammedDiversionRange + 1);
            }
            else if (!args.GuidedTarget.IsValidFor(args.SourceActor))
                desiredHFacing = hFacing;

            // Compute new direction the projectile will be facing
            hFacing = Util.TickFacing(hFacing, desiredHFacing, info.HorizontalRateOfTurn);
            vFacing = Util.TickFacing(vFacing, desiredVFacing, info.VerticalRateOfTurn);

            // Compute the projectile's guided displacement
            return new WVec(0, -1024 * speed, 0)
                .Rotate(new WRot(WAngle.FromFacing(vFacing), WAngle.Zero, WAngle.Zero))
                .Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(hFacing)))
                / 1024;
        }


        bool JammedBy(TraitPair<JamsMissiles> tp)
        {
            if ((tp.Actor.CenterPosition - pos).HorizontalLengthSquared > tp.Trait.Range.LengthSquared)
                return false;

            if (!tp.Trait.DeflectionStances.HasStance(tp.Actor.Owner.Stances[args.SourceActor.Owner]))
                return false;

            return tp.Actor.World.SharedRandom.Next(100 / tp.Trait.Chance) == 0;
        }

        int HomingInnerTick(int predClfDist, int diffClfMslHgt, int relTarHorDist, int lastHtChg, int lastHt,
            int nxtRelTarHorDist, int relTarHgt, int vFacing, bool targetPassedBy)
        {
            int desiredVFacing = vFacing;

            // Incline coming up -> attempt to reach the incline so that after predClfDist
            // the height above the terrain is positive but as close to 0 as possible
            // Also, never change horizontal facing and never travel backwards
            // Possible techniques to avoid close cliffs are deceleration, turning
            // as sharply as possible to travel directly upwards and then returning
            // to zero vertical facing as low as possible while still not hitting the
            // high terrain. A last technique (and the preferred one, normally used when
            // the missile hasn't been fired near a cliff) is simply finding the smallest
            // vertical facing that allows for a smooth climb to the new terrain's height
            // and coming in at predClfDist at exactly zero vertical facing
            if (info.TerrainHeightAware && diffClfMslHgt >= 0 && !allowPassBy)
                desiredVFacing = IncreaseAltitude(predClfDist, diffClfMslHgt, relTarHorDist, vFacing);
            else if (relTarHorDist <= 3 * loopRadius || state == States.Hitting)
            {
                // No longer travel at cruise altitude
                state = States.Hitting;

                if (lastHt >= targetPosition.Z)
                    allowPassBy = true;

                if (!allowPassBy && (lastHt < targetPosition.Z || targetPassedBy))
                {
                    // Aim for the target
                    var vDist = new WVec(-relTarHgt, -relTarHorDist, 0);
                    desiredVFacing = (sbyte)vDist.HorizontalLengthSquared != 0 ? vDist.Yaw.Facing : vFacing;

                    // Do not accept -1  as valid vertical facing since it is usually a numerical error
                    // and will lead to premature descent and crashing into the ground
                    if (desiredVFacing == -1)
                        desiredVFacing = 0;

                    // If the target has been passed by, limit the absolute value of
                    // vertical facing by the maximum vertical rate of turn
                    // Do this because the missile will be looping horizontally
                    // and thus needs smaller vertical facings so as not
                    // to hit the ground prematurely
                    if (targetPassedBy)
                        desiredVFacing = desiredVFacing.Clamp(-info.VerticalRateOfTurn, info.VerticalRateOfTurn);
                    else if (lastHt == 0)
                    { // Before the target is passed by, missile speed should be changed
                      // Target's height above loop's center
                        var tarHgt = (loopRadius * WAngle.FromFacing(vFacing).Cos() / 1024 - System.Math.Abs(relTarHgt)).Clamp(0, loopRadius);

                        // Target's horizontal distance from loop's center
                        var tarDist = Exts.ISqrt(loopRadius * loopRadius - tarHgt * tarHgt);

                        // Missile's horizontal distance from loop's center
                        var missDist = loopRadius * WAngle.FromFacing(vFacing).Sin() / 1024;

                        // If the current height does not permit the missile
                        // to hit the target before passing it by, lower speed
                        // Otherwise, increase speed
                        if (relTarHorDist <= tarDist - System.Math.Sign(relTarHgt) * missDist)
                            ChangeSpeed(-1);
                        else
                            ChangeSpeed();
                    }
                }
                else if (allowPassBy || (lastHt != 0 && relTarHorDist - lastHtChg < loopRadius))
                {
                    // Only activate this part if target too close to cliff
                    allowPassBy = true;

                    // Vector from missile's current position pointing to the loop's center
                    var radius = new WVec(loopRadius, 0, 0)
                        .Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(64 - vFacing)));

                    // Vector from loop's center to incline top hardcoded in height buffer zone
                    var edgeVector = new WVec(lastHtChg, lastHt - pos.Z, 0) - radius;

                    if (!targetPassedBy)
                    {
                        // Climb to critical height
                        if (relTarHorDist > 2 * loopRadius)
                        {
                            // Target's distance from cliff
                            var d1 = relTarHorDist - lastHtChg;
                            if (d1 < 0)
                                d1 = 0;
                            if (d1 > 2 * loopRadius)
                                return 0;

                            // Find critical height at which the missile must be once it is at one loopRadius
                            // away from the target
                            var h1 = loopRadius - Exts.ISqrt(d1 * (2 * loopRadius - d1)) - (pos.Z - lastHt);

                            if (h1 > loopRadius * (1024 - WAngle.FromFacing(vFacing).Cos()) / 1024)
                                desiredVFacing = WAngle.ArcTan(Exts.ISqrt(h1 * (2 * loopRadius - h1)), loopRadius - h1).Angle >> 2;
                            else
                                desiredVFacing = 0;

                            // TODO: deceleration checks!!!
                        }
                        else
                        {
                            // Avoid the cliff edge
                            if (info.TerrainHeightAware && edgeVector.Length > loopRadius && lastHt > targetPosition.Z)
                            {
                                int vFac;
                                for (vFac = vFacing + 1; vFac <= vFacing + info.VerticalRateOfTurn - 1; vFac++)
                                {
                                    // Vector from missile's current position pointing to the loop's center
                                    radius = new WVec(loopRadius, 0, 0)
                                        .Rotate(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(64 - vFac)));

                                    // Vector from loop's center to incline top + 64 hardcoded in height buffer zone
                                    edgeVector = new WVec(lastHtChg, lastHt - pos.Z, 0) - radius;
                                    if (edgeVector.Length <= loopRadius)
                                        break;
                                }

                                desiredVFacing = vFac;
                            }
                            else
                            {
                                // Aim for the target
                                var vDist = new WVec(-relTarHgt, -relTarHorDist, 0);
                                desiredVFacing = (sbyte)vDist.HorizontalLengthSquared != 0 ? vDist.Yaw.Facing : vFacing;
                                if (desiredVFacing < 0 && info.VerticalRateOfTurn < (sbyte)vFacing)
                                    desiredVFacing = 0;
                            }
                        }
                    }
                    else
                    {
                        // Aim for the target
                        var vDist = new WVec(-relTarHgt, relTarHorDist, 0);
                        desiredVFacing = (sbyte)vDist.HorizontalLengthSquared != 0 ? vDist.Yaw.Facing : vFacing;
                        if (desiredVFacing < 0 && info.VerticalRateOfTurn < (sbyte)vFacing)
                            desiredVFacing = 0;
                    }
                }
                else
                {
                    // Aim to attain cruise altitude as soon as possible while having the absolute value
                    // of vertical facing bound by the maximum vertical rate of turn
                    var vDist = new WVec(-diffClfMslHgt - info.CruiseAltitude.Length, -speed, 0);
                    desiredVFacing = (sbyte)vDist.HorizontalLengthSquared != 0 ? vDist.Yaw.Facing : vFacing;

                    // If the missile is launched above CruiseAltitude, it has to descend instead of climbing
                    if (-diffClfMslHgt > info.CruiseAltitude.Length)
                        desiredVFacing = -desiredVFacing;

                    desiredVFacing = desiredVFacing.Clamp(-info.VerticalRateOfTurn, info.VerticalRateOfTurn);

                    ChangeSpeed();
                }
            }
            else
            {
                // Aim to attain cruise altitude as soon as possible while having the absolute value
                // of vertical facing bound by the maximum vertical rate of turn
                var vDist = new WVec(-diffClfMslHgt - info.CruiseAltitude.Length, -speed, 0);
                desiredVFacing = (sbyte)vDist.HorizontalLengthSquared != 0 ? vDist.Yaw.Facing : vFacing;

                // If the missile is launched above CruiseAltitude, it has to descend instead of climbing
                if (-diffClfMslHgt > info.CruiseAltitude.Length)
                    desiredVFacing = -desiredVFacing;

                desiredVFacing = desiredVFacing.Clamp(-info.VerticalRateOfTurn, info.VerticalRateOfTurn);

                ChangeSpeed();
            }

            return desiredVFacing;
        }


        void ChangeSpeed(int sign = 1)
        {
            speed = (speed + sign * info.Acceleration.Length).Clamp(0, maxSpeed);

            //Compute the vertical loop radius
            loopRadius = LoopRadius(speed, info.VerticalRateOfTurn);
        }


        int IncreaseAltitude(int predClfDist, int diffClfMslHgt, int relTarHorDist, int vFacing)
        {
            var desiredVFacing = vFacing;

            // If missile is below incline top height and facing downwards, bring back
            // its vertical facing above zero as soon as possible
            if ((sbyte)vFacing < 0)
                desiredVFacing = info.VerticalRateOfTurn;

            // Missile will climb around incline top if bringing vertical facing
            // down to zero on an arc of radius loopRadius
            else if (IsNearInclineTop(vFacing, loopRadius, predClfDist)
                && WillClimbAroundInclineTop(vFacing, loopRadius, predClfDist, diffClfMslHgt, speed))
                desiredVFacing = 0;

            // Missile will not climb terrAltDiff w-units within hHeightChange w-units
            // all the while ending the ascent with vertical facing 0
            else if (!WillClimbWithinDistance(vFacing, loopRadius, predClfDist, diffClfMslHgt))

                // Find smallest vertical facing, attainable in the next tick,
                // for which the missile will be able to climb terrAltDiff w-units
                // within hHeightChange w-units all the while ending the ascent
                // with vertical facing 0
                for (var vFac = System.Math.Min(vFacing + info.VerticalRateOfTurn - 1, 63); vFac >= vFacing; vFac--)
                    if (!WillClimbWithinDistance(vFac, loopRadius, predClfDist, diffClfMslHgt)
                        && !(predClfDist <= loopRadius * (1024 - WAngle.FromFacing(vFac).Sin()) / 1024
                            && WillClimbAroundInclineTop(vFac, loopRadius, predClfDist, diffClfMslHgt, speed)))
                    {
                        desiredVFacing = vFac + 1;
                        break;
                    }

            // Attained height after ascent as predicted from upper part of incline surmounting manoeuvre
            var predAttHght = loopRadius * (1024 - WAngle.FromFacing(vFacing).Cos()) / 1024 - diffClfMslHgt;

            // Should the missile be slowed down in order to make it more manoeuverable
            var slowDown = info.Acceleration.Length != 0 // Possible to decelerate
                && ((desiredVFacing != 0 // Lower part of incline surmounting manoeuvre

                        // Incline will be hit before vertical facing attains 64
                        && (predClfDist <= loopRadius * (1024 - WAngle.FromFacing(vFacing).Sin()) / 1024

                            // When evaluating this the incline will be *not* be hit before vertical facing attains 64
                            // At current speed target too close to hit without passing it by
                            || relTarHorDist <= 2 * loopRadius * (2048 - WAngle.FromFacing(vFacing).Sin()) / 1024 - predClfDist))

                    || (desiredVFacing == 0 // Upper part of incline surmounting manoeuvre
                        && relTarHorDist <= loopRadius * WAngle.FromFacing(vFacing).Sin() / 1024
                            + Exts.ISqrt(predAttHght * (2 * loopRadius - predAttHght)))); // Target too close to hit at current speed

            if (slowDown)
                ChangeSpeed(-1);

            return desiredVFacing;
        }

        // This function checks if the missile's vertical facing is
        // nonnegative, and the incline top's horizontal distance from the missile is
        // less than loopRadius * (1024 - WAngle.FromFacing(vFacing).Sin()) / 1024
        static bool IsNearInclineTop(int vFacing,int loopRadius,int predClfDist)
        {
            return vFacing >= 0 && predClfDist <= loopRadius * (1024 - WAngle.FromFacing(vFacing).Sin()) / 1024;
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