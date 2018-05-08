using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MTS.Engine
{
	/// <summary>
	/// Intended to be something similar to System.Environment, except containing more pertinent stuff
	/// </summary>
	public static class ConsoleEnvironment
	{
		/// <summary>
		/// The Platform you're running under
		/// </summary>
		public static PlatformType Platform = (PlatformType)Native.GetPlatformType();
	}

	public class ProjectConfigPlatform
	{
		public BackendType Backend;
	}

	public class ProjectConfig
	{
		public ProjectConfig()
		{
			SetBackend(PlatformType.Proto, BackendType.SDL);
			SetBackend(PlatformType.Windows, BackendType.SDL);
			SetBackend(PlatformType.Switch, BackendType.Switch);
		}
		public Dictionary<PlatformType, ProjectConfigPlatform> Platforms = new Dictionary<PlatformType, ProjectConfigPlatform>();
		void SetBackend(PlatformType platform, BackendType backend)
		{
			Platforms[platform] = new ProjectConfigPlatform() { Backend = backend };
		}
	}

	/// <summary>
	/// Provisionally, this allows us to have backends independently of target platform
	/// For instance, on windows we might have SDL and d3d.
	/// Someone else making their own backend or forking ours could slot in here (we'd accept a patch with new backend enums)
	/// </summary>
	public enum BackendType
	{
		SDL,
		Switch
	}

	/// <summary>
	/// The MTS senses of platform (may not align completely with brute platforms)
	/// </summary>
	public enum PlatformType
	{
		Proto,
		Windows,
		Switch
	}

	public unsafe static class Native
	{
		[DllImport("MTS.Engine.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int DllTest(int num);

		[DllImport("MTS.Engine.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetPlatformType();

		[DllImport("MTS.Engine.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool zlib_uncompress(void *dest, void* src, int destLen, int srcLen);

		[DllImport("MTS.Engine.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int zlib_compress(void *dest, void* src, int destLen, int srcLen, int level);
	}
}

