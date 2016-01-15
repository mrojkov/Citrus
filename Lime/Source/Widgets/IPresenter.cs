using System;

namespace Lime
{
	public interface IPresenter
	{
		void Render();
		IPresenter Clone(Node newNode);
	}
}