#pragma warning disable 0169 //field is never used

using System;
using System.Runtime.InteropServices;

namespace MTS.Engine.SDL.PADL
{
	public static class EasyPadButton
	{
		public const uint UP = (1 << 0);
		public const uint DOWN = (1 << 1);
		public const uint LEFT = (1 << 2);
		public const uint RIGHT = (1 << 3);
		public const uint START = (1 << 4);
		public const uint BACK = (1 << 5);
		public const uint LEFT_THUMB = (1 << 6);
		public const uint RIGHT_THUMB = (1 << 7);
		public const uint LEFT_SHOULDER = (1 << 8);
		public const uint RIGHT_SHOULDER = (1 << 9);
		public const uint A = (1 << 10);
		public const uint B = (1 << 11);
		public const uint X = (1 << 12);
		public const uint Y = (1 << 13);
	}

	public struct EasyPadState
	{
		public uint buttons;
		public bool attached;

		[DllImport("MTS.Engine.SDL.Native.dll", EntryPoint = "PADL_EasyPadPoll")]
		static extern void EasyPadPoll(int index, ref EasyPadState outState);

		public void Poll(int index)
		{
			EasyPadPoll(index, ref this);
		}
	}
}