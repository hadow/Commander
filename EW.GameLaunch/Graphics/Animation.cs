﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Support;

namespace EW.Graphics
{
    /// <summary>
    /// 动画
    /// </summary>
    public class Animation
    {
        public ISpriteSequence CurrentSequence { get; private set; }

        public string Name { get; private set; }

        public bool IsDecoration { get; set; }

        readonly SequenceProvider sequenceProvider;

        readonly Func<int> facingFunc;

        readonly Func<bool> paused;

        int frame;
        bool backwards;
        bool tickAlways;
        int timeUntilNextFrame;

        Action tickFunc = () => { };

        public Animation(World world,string name) : this(world, name, () => 0) { }

        public Animation(World world,string name, Func<int> facingFunc) : this(world, name, facingFunc, null) { }

        public Animation(World world,string name,Func<bool> paused) : this(world, name, () => 0, paused) { }

        public Animation(World world,string name,Func<int> facingFunc,Func<bool> paused)
        {
            sequenceProvider = world.Map.Rules.Sequences;
            Name = name.ToLowerInvariant();
            this.facingFunc = facingFunc;
            this.paused = paused;
        }


        public string GetRandomExistingSequence(string[] sequences,MersenneTwister random)
        {
            return sequences.Where(s => HasSequence(s)).Random(random);
        }

        public void Tick()
        {
            if (paused == null || !paused())
                Tick(40);// tick one frame.
        }

        public void Tick(int t)
        {

            if (tickAlways)
                tickFunc();
            else
            {
                timeUntilNextFrame -= t;
                while(timeUntilNextFrame <= 0)
                {
                    tickFunc();
                    timeUntilNextFrame += CurrentSequenceTickOrDefault();
                }
            }


        }


        /// <summary>
        /// 循环播放
        /// </summary>
        /// <param name="sequenceName"></param>
        public void PlayRepeating(string sequenceName)
        {
            backwards = false;
            tickAlways = false;
            PlaySequence(sequenceName);

            frame = 0;
            tickFunc = () =>
            {
                ++frame;
                if (frame >= CurrentSequence.Length)
                    frame = 0;
            };
        }


        public void PlayBackwardsThen(string sequenceName,Action after)
        {
            PlayThen(sequenceName, after);
            backwards = true;
        }


        public void PlayFetchIndex(string sequenceName,Func<int> func)
        {
            backwards = false;
            tickAlways = true;
            PlaySequence(sequenceName);

            frame = func();
            tickFunc = () => frame = func();
        }

        void PlaySequence(string sequenceName)
        {
            CurrentSequence = GetSequence(sequenceName);
            timeUntilNextFrame = CurrentSequenceTickOrDefault();
        }

        public void PlayThen(string sequenceName,Action after)
        {
            backwards = false;
            tickAlways = false;
            PlaySequence(sequenceName);

            frame = 0;
            tickFunc = () =>
            {
                ++frame;
                if (frame >= CurrentSequence.Length)
                {
                    frame = CurrentSequence.Length - 1;
                    tickFunc = () => { };
                    if (after != null)
                        after();
                }
            };
        }

        public void Play(string sequenceName)
        {
            PlayThen(sequenceName, null);
        }

        public ISpriteSequence GetSequence(string sequenceName)
        {
            return sequenceProvider.GetSequence(Name, sequenceName);
        }

        int CurrentSequenceTickOrDefault()
        {
            const int DefaultTick = 40;//默认每秒25帧

            return CurrentSequence != null ? CurrentSequence.Tick : DefaultTick;
        }

        public bool HasSequence(string seq) { return sequenceProvider.HasSequence(Name, seq); }


        public int CurrentFrame { get { return backwards ? CurrentSequence.Start + CurrentSequence.Length - frame - 1 : frame; } }

        public Sprite Image { get { return CurrentSequence.GetSprite(CurrentFrame, facingFunc()); } }


        public IEnumerable<IRenderable> Render(WPos pos,PaletteReference palette)
        {
            return Render(pos, WVec.Zero, 0, palette, 1f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="offset"></param>
        /// <param name="zOffset"></param>
        /// <param name="palette"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public IEnumerable<IRenderable> Render(WPos pos,WVec offset,int zOffset,PaletteReference palette,float scale)
        {
            var imageRenderable = new SpriteRenderable(Image, pos,offset, CurrentSequence.ZOffset + zOffset, palette, scale, IsDecoration);

            if(CurrentSequence.ShadowStart >= 0)
            {
                var shadow = CurrentSequence.GetShadow(CurrentFrame, facingFunc());
                var shadowRenderable = new SpriteRenderable(shadow, pos, offset, CurrentSequence.ShadowZOffset + zOffset, palette, scale, true);
                return new IRenderable[] { shadowRenderable, imageRenderable };
            }
            return new IRenderable[] { imageRenderable };
        }

        public Rectangle ScreenBounds(WorldRenderer wr,WPos pos,WVec offset,float scale)
        {

            var xy = wr.ScreenPxPosition(pos) + wr.ScreenPxOffset(offset);

            var cb = CurrentSequence.Bounds;

            return Rectangle.FromLTRB(xy.X + (int)(cb.Left * scale),
                                        xy.Y + (int)(cb.Top * scale),
                                        xy.X + (int)(cb.Right * scale),
                                        xy.Y + (int)(cb.Bottom * scale));
        }


        public bool ReplaceAnim(string sequenceName)
        {
            if (!HasSequence(sequenceName))
                return false;

            CurrentSequence = GetSequence(sequenceName);
            timeUntilNextFrame = Math.Min(CurrentSequenceTickOrDefault(), timeUntilNextFrame);
            frame %= CurrentSequence.Length;
            return true;
        }

    }
}