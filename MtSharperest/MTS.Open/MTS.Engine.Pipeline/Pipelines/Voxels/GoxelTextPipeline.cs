using System;
using System.IO;
using MTS.Engine;

namespace MTS.Engine.Pipeline.Pipelines.Voxels
{
	public class GoxelTextPipeline : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context)
		{
			context.Depend("source", context.RawContentDiskPath + ".txt");
		}

		float PackUintColor(uint color)
		{
			int b = (int)color & 0xFF;
			int g = (int)(color>>8) & 0xFF;
			int r = (int)(color>>16) & 0xFF;
			return r + g * 256.0f + b * 256.0f * 256.0f;
		}

		public bool Bake(PipelineBakeContext context)
		{
			var path = context.Depends["source"];

			int voxelCount = 0;
			var lines = File.ReadAllLines(path);
			foreach (var line in lines)
			{
				if (line.StartsWith("#"))
					continue;
				voxelCount++;
			}

			context.BakedWriter.Write(voxelCount);
			foreach (var line in lines)
			{
				if (line.StartsWith("#"))
					continue;

				//TODO - make tokenizing framework
				var parts = line.Split(' ');

				var x = short.Parse(parts[0]);
				var y = short.Parse(parts[1]);
				var z = short.Parse(parts[2]);
				var ucolor = uint.Parse(parts[3], System.Globalization.NumberStyles.HexNumber);
				var fcolor = PackUintColor(ucolor);

				context.BakedWriter.Write(fcolor);
				context.BakedWriter.Write(x);
				context.BakedWriter.Write(y);
				context.BakedWriter.Write(z);
				context.BakedWriter.Write((short)0); //pad
			}

			return true;
		}
	}

}