using System;
using System.IO;

namespace MTS.Engine.ContentTypes
{
	/// <summary>
	/// A compiled shader program. It will take care of compiling itself when the source file changes.
	/// </summary>
	public unsafe class ShaderProgram : ContentBase, IBakedLoader
	{
		ContentConnectorContext_ShaderProgram connectorContext = new ContentConnectorContext_ShaderProgram();

		/// <summary>
		/// Backend-specific handle for the compiled program
		/// </summary>
		public object Handle;

		protected override void ContentUnload()
		{
			if (Handle == null) return;
			Manager.RuntimeConnector.UnloadShader(connectorContext);
		}

		bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			connectorContext.Reader = context.BakedReader;
			Manager.RuntimeConnector.LoadShader(connectorContext);
			Handle = connectorContext.Handle;
			return Handle != null;
		}


	}
}

