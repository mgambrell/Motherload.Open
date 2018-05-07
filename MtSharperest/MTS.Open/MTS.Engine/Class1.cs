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

