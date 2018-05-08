using System;
using System.IO;

namespace MTS.Engine.SDL
{
	public class DefaultPipelineConnector : PipelineConnectorBase
	{
		public override IContentPipeline GetPipeline(ContentBase content)
		{
			if (content is ShaderProgram)
				return new ShaderProgramPipeline();
			return base.GetPipeline(content);
		}
	}

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
			var vs_src = File.ReadAllText(context.Depends["vs"]);
			var fs_src = File.ReadAllText(context.Depends["fs"]);

			//reminder: could easily layer encryption into the pipeline output, to munge your shader sources
			context.BakedWriter.Write(vs_src);
			context.BakedWriter.Write(fs_src);

			return true;
		}
	}
}
