using System;
using System.IO;

namespace MTS.Engine.ContentTypes
{
	/// <summary>
	/// Just a text file.
	/// </summary>
	public unsafe class TextFile : ContentBase, IBakedLoader
	{
		//we're considering empty content to be proxy content.. we dont want it to be null, just.. empty
		string mText = string.Empty;

		public string Text { get { return mText; } }

		protected override void ContentUnload()
		{
			mText = string.Empty;
		}

		bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			var br = context.BakedReader;

			mText = br.ReadString();

			return true;
		}
	}
}

