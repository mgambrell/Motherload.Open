using System;
using System.IO;
using MTS.Engine;

namespace MTS.Engine.Pipelines
{
	public class ShaderProgramPipeline : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context)
		{
			var vsna = context.Content.Attributes.GetAttribute<VertexShaderNameAttribute>();
			var fsna = context.Content.Attributes.GetAttribute<FragmentShaderNameAttribute>();

			if (vsna == null || fsna == null)
			{
				context.Failed = true;
				return;
			}

			var dir = System.IO.Path.GetDirectoryName(context.RawContentDiskPath);

			context.Depend("vs", dir + "/" + vsna.Name + ".glsl");
			context.Depend("fs", dir + "/" + fsna.Name + ".glsl");
		}

		public bool Bake(PipelineBakeContext context)
		{
			for (int i = 0; i < 256; i++)
			{
				var resolved = context.Depends[i];
				if (resolved == null) continue;
				Console.WriteLine("Have font page: " + resolved);
			}
			return false;
		}
	}

}