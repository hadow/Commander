using System;


namespace EW.Framework.Touch
{
    /// <summary>
    /// ´¥¿ØÎ»ÖÃ
    /// </summary>
    public struct TouchLocation:IEquatable<TouchLocation>
    {
        private int _id;
        private Vector2 _position;
        private Vector2 _previousPosition;
        private TouchLocationState _state;
        private TouchLocationState _previousState;

        //Only used in Android,for now
        private float _pressure;
        private float _previousPressure;

        private Vector2 _velocity;
        private Vector2 _pressPosition;
        private TimeSpan _pressTimeStamp;
        private TimeSpan _timeStamp;




        public TouchLocation(int id,TouchLocationState state,Vector2 position) : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero)
        {

        }
        public TouchLocation(int id,TouchLocationState state,Vector2 position,TouchLocationState previousState,Vector2 previousPos) : this(id, state, position, previousState, previousPos, TimeSpan.Zero)
        {

        }
        internal TouchLocation(int id,TouchLocationState state,Vector2 position,TimeSpan timeStamp):this(id,state,position,TouchLocationState.Invalid,Vector2.Zero,timeStamp) { }

        internal TouchLocation(int id,TouchLocationState state,Vector2 position,TouchLocationState previousState,Vector2 previousPosition,TimeSpan timeStamp)
        {
            _id = id;
            _state = state;
            _position = position;
            _pressure = 0.0f;

            _previousState = previousState;
            _previousPosition = previousPosition;
            _previousPressure = 0.0f;

            _timeStamp = timeStamp;
            _velocity = Vector2.Zero;

            if(state == TouchLocationState.Pressed)
            {
                _pressPosition = _position;
                _pressTimeStamp = _timeStamp;
            }
            else
            {
                _pressPosition = Vector2.Zero;
                _pressTimeStamp = TimeSpan.Zero;
            }

            
        }

        public bool Equals(TouchLocation other)
        {
            return false;
        }
    }
}