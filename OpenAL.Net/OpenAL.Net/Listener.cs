using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace OpenAL
{
    public class Listener
    {
        private readonly IntPtr _context = IntPtr.Zero;

        public Listener(IntPtr context)
        {
            _context = context;
        }

        /// <summary>
        /// Listener position in game-world coordinates.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                lock (typeof (PlaybackStream))
                {
                    API.alcMakeContextCurrent(_context);
                    float val1, val2, val3;
                    API.alGetListener3f(FloatSourceProperty.AL_POSITION, out val1, out val2, out val3);
                    return new Vector3() { X = val1, Y = val2, Z = val3 };
                }
            }
            set
            {
                lock (typeof (PlaybackStream))
                {
                    API.alcMakeContextCurrent(_context);
                    API.alListenerfv(FloatSourceProperty.AL_POSITION, new[] {value.X, value.Y, value.Z});
                }
            }
        }

        public Vector3 Velocity
        {
            get
            {
                lock (typeof(PlaybackStream))
                {
                    API.alcMakeContextCurrent(_context);
                    float val1, val2, val3;
                    API.alGetListener3f(FloatSourceProperty.AL_VELOCITY, out val1, out val2, out val3);
                    return new Vector3() { X = val1, Y = val2, Z = val3 };
                }
            }
            set
            {
                lock (typeof(PlaybackStream))
                {
                    API.alcMakeContextCurrent(_context);
                    API.alListenerfv(FloatSourceProperty.AL_VELOCITY, new[] { value.X, value.Y, value.Z });
                }
            }
        }

        public Orientation Orientation
        {
            set
            {
                lock (typeof (PlaybackStream))
                {
                    API.alcMakeContextCurrent(_context);
                    API.alListenerfv(FloatSourceProperty.AL_ORIENTATION, new[] { value.At.X, value.At.Y, value.At.Z, value.Up.X, value.Up.Y, value.Up.Z });
                }
            }
            get
            {
                lock (typeof (PlaybackStream))
                {
                    var vals = new float[6];
                    API.alcMakeContextCurrent(_context);
                    API.alGetListenerfv(FloatSourceProperty.AL_ORIENTATION, vals);
                    return new Orientation()
                        {
                            At = new Vector3() { X = vals[0], Y = vals[1], Z = vals[2] },
                            Up = new Vector3() { X = vals[3], Y = vals[4], Z = vals[5] }
                        };
                }
            }
        }
    }
}
