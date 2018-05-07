using System;
using System.IO;
using MTS.Engine;

namespace MTS.Engine.Pipelines
{
	public class NitroFontPipeline : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context)
		{
			var path = context.RawContentDiskPath;

			context.DependOptional("source", path + ".txt");

			//go ahead and check the existence of various source art formats we can read
			//TODO: make this common logic
			//TODO: make this not just png
			string ext = ".png";

			//add deps for all possible input pages
			for (int i = 0; i < 256; i++)
			{
				context.DependOptional(i, $"{path}_{i.ToString("X2")}{ext}");
			}
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