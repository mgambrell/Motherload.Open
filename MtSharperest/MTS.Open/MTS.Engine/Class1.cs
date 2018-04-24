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

	public static class Native
	{
		[DllImport("MTS.Engine.native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int DllTest(int num);

		[DllImport("MTS.Engine.native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetPlatformType();
	}
}


namespace MTS.Engine
{
	public class Class1
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
		}
	}
}
