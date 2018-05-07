using System;
using System.IO;
using MTS.Engine;

namespace MTS.Engine.Pipelines
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

			var bb = new BitmapBuffer(64, 64);
			bb.Serialize(context.BakedWriter);

			var cellLines = File.ReadAllLines(path);
			context.BakedWriter.Write(cellLines.Length);
			foreach (var line in cellLines)
				context.BakedWriter.Write(line);

			return true;
		}

	}

}