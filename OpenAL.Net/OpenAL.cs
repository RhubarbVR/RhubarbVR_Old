using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Reflection;

namespace OpenAL
{
    /// <summary>
    /// Helper class for working with OpenAL devices.
    /// </summary>
    public class OpenALHelper
    {
        public static void Start()
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), API.DllImportResolver);
        }

        private static CaptureDevice[] _captureDevices;
        public static CaptureDevice[] CaptureDevices
        {
            get
            {
                if (_captureDevices == null)
                {
                    var strings =
                        ReadStringsFromMemory(API.alcGetString(IntPtr.Zero,
                                                               (int) ALCStrings.ALC_CAPTURE_DEVICE_SPECIFIER));
                    _captureDevices = new CaptureDevice[strings.Length];
                    for (var i = 0; i < _captureDevices.Length; i++)
                    {
                        _captureDevices[i] = new CaptureDevice(strings[i]);
                    }
                }
                return _captureDevices;
            }
        }

        private static PlaybackDevice[] _playbackDevices;
        public static PlaybackDevice[] PlaybackDevices
        {
            get
            {
                if (_playbackDevices == null)
                {
                    var strings = new string[0];
                    if (GetIsExtensionPresent("ALC_ENUMERATE_ALL_EXT"))
                    {
                        strings =
                            ReadStringsFromMemory(API.alcGetString(IntPtr.Zero,
                                                                   (int) ALCStrings.ALC_ALL_DEVICES_SPECIFIER));
                    }
                    else if (GetIsExtensionPresent("ALC_ENUMERATION_EXT"))
                    {
                        strings =
                            ReadStringsFromMemory(API.alcGetString(IntPtr.Zero, (int) ALCStrings.ALC_DEVICE_SPECIFIER));
                    }
                    _playbackDevices = new PlaybackDevice[strings.Length];
                    for (var i = 0; i < _playbackDevices.Length; i++)
                    {
                        _playbackDevices[i] = new PlaybackDevice(strings[i]);
                    }
                }
                return _playbackDevices;
            }
        }

        internal static string[] ReadStringsFromMemory(IntPtr location)
        {
            List<string> strings = new List<string>();

            bool lastNull = false;
            int i = -1;
            byte c;
            while (!((c = Marshal.ReadByte(location, ++i)) == '\0' && lastNull))
            {
                if (c == '\0')
                {
                    lastNull = true;

                    strings.Add(Marshal.PtrToStringAnsi(location, i));
                    location = new IntPtr((long)location + i + 1);
                    i = -1;
                }
                else
                    lastNull = false;
            }

            return strings.ToArray();
        }

        internal static bool GetIsExtensionPresent(string extension)
        {
            sbyte result;
            if (extension.StartsWith("ALC"))
            {
                result = API.alcIsExtensionPresent(IntPtr.Zero, extension);
            }
            else
            {
                result = API.alIsExtensionPresent(extension);
                //  todo: check for errors here
            }

            return (result == 1);
        }
    }
}
