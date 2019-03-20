using System;

namespace Orange
{
	public interface IStage
	{
		void Action();
		string[] Extensions { get; }
	}

	public class CookStage: IStage
	{
		public string[] Extensions { get; protected set; }

		public CookStage()
		{
			SetExtensions();
		}

		public virtual void Action()
		{

		}

		protected virtual void SetExtensions()
		{

		}
	}
}
