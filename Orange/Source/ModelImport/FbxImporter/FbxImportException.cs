using System;

namespace Orange.FbxImporter
{
	public class FbxImportException : Exception
	{
		public FbxImportException(string message) : base(message)
		{
		}
	}

	public class FbxAtributeImportException : FbxImportException
	{
		private const string messageTemplate = "An error has occured when parsing a node of a type {0} in native library";

		public FbxAtributeImportException(FbxNodeAttribute.FbxNodeType type) : base(string.Format(messageTemplate, type))
		{

		}
	}
}
