using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MTS.Engine
{
	public static class Native
	{
		[DllImport("MTS.Engine.native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int DllTest(int num);
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
			if (!System.IO.Directory.GetCurrentDirectory().StartsWith("B:"))
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
