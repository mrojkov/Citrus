
namespace Lime
{
	public class ModifierMaskParser
	{
		private const ulong LShiftFlag = 0x20002;
		private const ulong RShiftFlag = 0x20004;
		private const ulong LCtrlFlag = 0x40001;
		private const ulong RCtrlFlag = 0x42000;
		private const ulong LAltFlag = 0x80020;
		private const ulong RAltFlag = 0x80040;
		private const ulong LWinFlag = 0x100008;
		private const ulong RWinFlag = 0x100010;

		private ulong mask;

		public ModifierMaskParser(ulong mask)
		{
			this.mask = mask;
		}

		public bool IsLShiftPressed()
		{
			return (mask & LShiftFlag) == LShiftFlag;
		}

		public bool IsRShiftPressed()
		{
			return (mask & RShiftFlag) == RShiftFlag;
		}

		public bool IsLCtrlPressed()
		{
			return (mask & LCtrlFlag) == LCtrlFlag;
		}

		public bool IsRCtrlPressed()
		{
			return (mask & RCtrlFlag) == RCtrlFlag;
		}

		public bool IsLAltPressed()
		{
			return (mask & LAltFlag) == LAltFlag;
		}

		public bool IsRAltPressed()
		{
			return (mask & RAltFlag) == RAltFlag;
		}

		public bool IsLWinPressed()
		{
			return (mask & LWinFlag) == LWinFlag;
		}

		public bool IsRWinPressed()
		{
			return (mask & RWinFlag) == RWinFlag;
		}
	}
}

