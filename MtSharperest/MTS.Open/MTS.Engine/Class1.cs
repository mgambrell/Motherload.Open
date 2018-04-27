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


namespace MTS.Engine
{
	public unsafe class Class1
	{
		public static void Test()
		{
			//placeholder for detecting where we're running
			Console.WriteLine("WINDOWS LOGIC");
			if (ConsoleEnvironment.Platform == PlatformType.Switch)
			{
				Console.WriteLine("Wrong platform");
			}
			else
			{
				Console.WriteLine(MTS.Engine.Native.DllTest(99));
			}

			byte[] test = new byte[10];
			byte[] crunched = new byte[100];
			fixed (byte* disarmingly = &test[0])
			fixed (byte* disheveled = &crunched[0])
				Native.zlib_compress(disheveled, disarmingly, 100, 10, 1);
		}
	}
}
