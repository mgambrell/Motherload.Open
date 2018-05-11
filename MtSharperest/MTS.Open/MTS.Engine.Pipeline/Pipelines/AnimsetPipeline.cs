using System;
using System.IO;
using MTS.Engine;

using MTS.Engine.ContentUtils;

namespace MTS.Engine.Pipeline.Pipelines
{
	public class AnimsetPipeline : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context) { }

		public bool Bake(PipelineBakeContext context)
		{
			Console.WriteLine("AnimSet.Bake: " + context.ContentPath);

			var path = context.RawContentDiskPath;
			path += ".txt";
			context.Depend(path);

			//I dont know... I dont know...
			if (!File.Exists(path)) return false;

			var img = ImageBuffer.Create(TextureFormat.Color, 64, 64);
			img.Serialize(context.BakedWriter);

			var cellLines = File.ReadAllLines(path);
			context.BakedWriter.Write(cellLines.Length);
			foreach (var line in cellLines)
				context.BakedWriter.Write(line);

			return true;
		}

	}

}