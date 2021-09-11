namespace SteamAudio
{
	public static partial class IPL
	{
#if Windows
		public const string Library = "Natives\\Windows64\\phonon.dll";
#elif Linux
        public const string Library = "Natives/Linux64/libphonon.so";
#elif OSX
        public const string Library = "Natives/OSX64/libphonon.dylib";
#endif

		static IPL() => DllManager.PrepareResolver();

		public partial struct Vector3
		{
			public Vector3(float x, float y, float z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
		}
	}
}
