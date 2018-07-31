using System;
using System.Collections.Generic;

namespace MTS.Engine.Modules
{
	public class BitPad
	{
		public ulong justreleased;
		public ulong justpressed;
		public ulong pressed;
		public ulong unpressed;
		public ulong down;

		public bool dead;

		public void UpdateWith(ulong buttons)
		{
			down = buttons;
			unpressed = unpressed & buttons;
			justpressed = (~pressed) & buttons;
			justpressed = justpressed & ~unpressed;
			justreleased = pressed & ~buttons;
			justreleased = justreleased & ~unpressed;
			pressed = buttons & ~unpressed;
		}

		public bool JustPressed(ulong button)
		{
			return (justpressed & button) != 0 && !dead;
		}
		public void ClearJustPressed(ulong button)
		{
			justpressed &= ~button;
		}
		public bool JustReleased(ulong button)
		{
			return (justreleased & button) != 0 && !dead;
		}
		public bool IsPressed(ulong button)
		{
			return (pressed & button) != 0 && !dead;
		}
		public bool IsDown(ulong button)
		{
			return (down & button) != 0 && !dead;
			//ignores unpress
		}
		public bool IsUnpressed(ulong button)
		{
			return (unpressed & button) != 0 && !dead;
		}
		public void Unpress(ulong button)
		{
			ulong pressed_unpressed = pressed & button;
			unpressed |= pressed_unpressed;
			pressed &= ~button;

			//REF:
			//if (IsPressed(button))
			//{
			//	unpressed |= button;
			//	pressed &= ~button;
			//}
			//justpressed &= ~button;
		}
		public void PreUnpress(ulong button)
		{
			unpressed |= button;
			pressed &= ~button;
		}
		public void RemoveUnpress(ulong button)
		{
			unpressed &= ~button;
		}


		public void Clear()
		{
			justreleased = justpressed = pressed = unpressed = down = 0;
		}
	}
}