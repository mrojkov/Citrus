using System;

namespace Lime
{
	public interface IPresenter
	{
		void OnAssign(Node node);
		void Render();
		IPresenter Clone(Node node);
	}
}