using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace MTS.Engine.ContentTypes
{
	/// <summary>
	/// A comma-separated array of floats in a text file.
	/// Really this is just for super-rudimentary debugging vertex buffers.
	/// </summary>
	public unsafe class FloatArrayText : ContentBase, IBakedLoader
	{
		public float[] Array { get { return mArray; } }
		public int Length { get { return mArray.Length; } }

		float[] mArray = new float[0];

		protected override void ContentUnload()
		{
			mArray = new float[0];
		}

	
		bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			var br = context.BakedReader;

			int count = br.ReadInt32();
			mArray = new float[count];
			for (int i = 0; i < count; i++)
				mArray[i] = br.ReadSingle();

			return true;
		}

	}
}

