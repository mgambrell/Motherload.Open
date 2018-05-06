using System;
using System.IO;

namespace MTS.Engine
{
	/// <summary>
	/// Just a text file.
	/// Could rename as TextFile? along with BinaryFile?
	/// </summary>
	public unsafe class TextBlob : ContentBase, IContentBakeable
	{
		//we're considering empty content to be proxy content.. we dont want it to be null, just.. empty
		string mText = string.Empty;

		public string Text { get { return mText; } }

		protected override void ContentUnload()
		{
			mText = string.Empty;
		}

		unsafe void IContentBakeable.Prepare(PipelineBakeContext context)
		{
			context.Depend("source",context.RawContentDiskPath + ".txt");
		}

		unsafe bool IContentBakeable.Bake(PipelineBakeContext context)
		{
			var path = context.Depends["source"];

			var text = File.ReadAllText(path);
			context.BakedWriter.Write(text);

			return true;
		}

		bool IContentBakeable.LoadBaked(PipelineLoadBakedContext context)
		{
			var br = context.BakedReader;

			mText = br.ReadString();

			return true;
		}


	}
}

