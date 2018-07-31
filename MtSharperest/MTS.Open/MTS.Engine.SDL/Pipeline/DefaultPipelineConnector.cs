using System;
using System.IO;

using MTS.Engine.ContentTypes;

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

		public override void EvaluateTexture(PipelineConnector_TextureEvaluation evaluation)
		{
			evaluation.Accept();
		}

		public override void BakeTexture(PipelineConnector_TextureBaking baking)
		{
			baking.Image.BakeTexture(baking);
		}
	}

	public class ShaderProgramPipeline : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context)
		{
			var na = context.Content.Attributes.GetAttribute<ShaderNamesAttribute>();

			if (na == null)
			{
				context.Failed = true;
				return;
			}

			var dir = System.IO.Path.GetDirectoryName(context.RawContentDiskPath);

			context.Depend("vs", dir + "/" + na.VsName + ".glsl");
			context.Depend("fs", dir + "/" + na.FsName + ".glsl");
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
