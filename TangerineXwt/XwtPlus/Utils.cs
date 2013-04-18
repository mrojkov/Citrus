using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus
{
	public interface IUtilsBackend : Xwt.Backends.IBackend
	{
		Xwt.Point GetPointerPosition(Xwt.Widget widget);
		bool GetPointerButtonState(Xwt.PointerButton button);
		void CaptureMouse(Xwt.Widget widget);
		void ReleaseMouse();
	}

	[Xwt.Backends.BackendType(typeof(IUtilsBackend))]
	public class Utils : Xwt.XwtComponent
	{
		public static readonly Utils Instance = new Utils();

		IUtilsBackend Backend
		{
			get { return (IUtilsBackend)BackendHost.Backend; }
		}

		public Xwt.Point GetPointerPosition(Xwt.Widget widget)
		{
			return Backend.GetPointerPosition(widget);
		}

		public bool GetPointerButtonState(Xwt.PointerButton button)
		{
			return Backend.GetPointerButtonState(button);
		}

		public void CaptureMouse(Xwt.Widget widget)
		{
			Backend.CaptureMouse(widget);
		}

		public void ReleaseMouse()
		{
			Backend.ReleaseMouse();
		}
	}
}
