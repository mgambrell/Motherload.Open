using System;
using System.IO;

namespace MTS.Engine
{
	/// <summary>
	/// A compiled shader program. It will take care of compiling itself when the source file changes.
	/// This would need to be substantially customized for each target platform (this file should be placed in the platform specific directories and only one selected)
	/// </summary>
	public unsafe class ShaderProgram : ContentBase, IBakedLoader
	{
		ContentConnectorContext_ShaderProgram connectorContext = new ContentConnectorContext_ShaderProgram();

		public object Handle;

		protected override void ContentUnload()
		{
			if (Handle == null) return;
			Manager.ContentConnector.UnloadShader(connectorContext);
		}

		bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			connectorContext.Reader = context.BakedReader;
			Manager.ContentConnector.LoadShader(connectorContext);
			Handle = connectorContext.Handle;
			return Handle != null;
		}


	}
}

