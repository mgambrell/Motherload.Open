using System;
using System.Collections.Generic;
using System.IO;
using MTS.Engine;


namespace MTS.Engine.Pipeline.Pipelines
{
	public class FloatArrayTextFile : IContentPipeline
	{
			public void Prepare(PipelineBakeContext context)
		{
			var path = context.RawContentDiskPath;
			path += ".txt";
			context.Depend("source",path);
		}

		public bool Bake(PipelineBakeContext context)
		{
			var path = context.Depends["source"];

			var text = File.ReadAllText(path);

			text = text.Replace("\n", "").Replace("\r", "");
			var parts = text.Split(',');
			List<float> floats = new List<float>();
			foreach (var part in parts)
			{
				var str = part.Trim();
				str = str.TrimEnd('f');
				if (str == "") continue;
				floats.Add(float.Parse(str));
			}

			context.BakedWriter.Write(floats.Count);
			foreach(var f in floats)
				context.BakedWriter.Write(f);

			return true;
		}


	}

}