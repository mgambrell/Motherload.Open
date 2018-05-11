using System;
using System.IO;
using MTS.Engine;

namespace MTS.Engine.Pipeline.Pipelines
{
	public class TexturePipeline : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context)
		{
			var path = context.RawContentDiskPath;
			path += ".png";
			context.Depend(path);
		}

		public bool Bake(PipelineBakeContext context)
		{
			Console.WriteLine("Texture.Bake: " + context.ContentPath);

			//I dont know... I dont know...
			var path = context.RawContentDiskPath;
			if (!File.Exists(path)) return false;

			var img = ImageLoading.LoadImage(path);
			img.Serialize(context.BakedWriter);

			return true;
		}
	}

}