using System;
using System.IO;

namespace MTS.Engine.Loaders
{
	/// <summary>
	/// can only load baked content
	/// PLANNED: a 3rd loader type that's capable of hotload content over host IO, for rapid iteration on-console
	/// not a high priority...
	/// </summary>
	class BakedLoader : IContentLoader
	{
		internal ContentDirectory directoryOwner;
		internal string name;

		ContentBase content;

		public BakedLoader(ContentBase content)
		{
			this.content = content;
		}

		public bool Load(ContentLoadContext context)
		{
			PipelineLoadBakedContext loadBakedContext = new PipelineLoadBakedContext();
			loadBakedContext.ContentPath = name;
			var fpBaked = Path.Combine(directoryOwner.BakedContentDiskRoot, name);


			var bakedLoader = content as IBakedLoader;

			//save time... assume this succeeds
			try
			{
				using (var fs = new FileStream(fpBaked, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					loadBakedContext.BakedReader = new BinaryReader(fs);
					return bakedLoader.LoadBaked(loadBakedContext);
				}
			}
			catch
			{
				return false;
			}
		}
	}

}