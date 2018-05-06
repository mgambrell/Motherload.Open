using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace MTS.Engine
{
	/// <summary>
	/// A comma-separated array of floats in a text file.
	/// Really this is just for super-rudimentary debugging vertex buffers.
	/// </summary>
	public unsafe class FloatArrayText : ContentBase, IContentBakeable
	{
		public float[] Array { get { return mArray; } }
		public int Length { get { return mArray.Length; } }

		float[] mArray = new float[0];

		protected override void ContentUnload()
		{
			mArray = new float[0];
		}

		//-------------------------
		//IContentBakeable implementation

		unsafe void IContentBakeable.Prepare(PipelineBakeContext context)
		{
			var path = context.RawContentDiskPath;
			path += ".txt";
			context.Depend("source",path);
		}

		unsafe bool IContentBakeable.Bake(PipelineBakeContext context)
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

		bool IContentBakeable.LoadBaked(PipelineLoadBakedContext context)
		{
			var br = context.BakedReader;

			int count = br.ReadInt32();
			mArray = new float[count];
			for (int i = 0; i < count; i++)
				mArray[i] = br.ReadSingle();

			return true;
		}


		//end IContentBakeable
		//-----------------------------

	}
}

