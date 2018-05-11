using System;
using System.IO;
using MTS.Engine;

namespace MTS.Engine.Pipeline.Pipelines
{
	public class TextFile : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context)
		{
			context.Depend("source", context.RawContentDiskPath + ".txt");
		}

		public bool Bake(PipelineBakeContext context)
		{
			var path = context.Depends["source"];

			var text = File.ReadAllText(path);
			context.BakedWriter.Write(text);

			return true;
		}
	}

}