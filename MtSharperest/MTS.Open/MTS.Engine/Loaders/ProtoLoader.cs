using System;
using System.IO;

namespace MTS.Engine.Loaders
{
	/// <summary>
	/// can load baked content, and bake content on the fly if needed
	/// </summary>
	class ProtoLoader : IContentLoader
	{
		//internal IContentBakeable bakeable;
		internal ContentDirectory directoryOwner;
		internal string name;
		internal PipelineBakeContext bakeContext;

		internal ContentBase content;

		public ProtoLoader(ContentBase content)
		{
			this.content = content;
		}

		public bool Load(ContentLoadContext context)
		{
			bool loaded = false;

			//try loading the baked content
			PipelineLoadBakedContext loadBakedContext = new PipelineLoadBakedContext();
			loadBakedContext.ContentPath = name;
			var fpBaked = Path.Combine(directoryOwner.BakedContentDiskRoot, name);

			var bakeable = content.Manager.PipelineConnector.GetPipeline(content);
			var bakedLoader = content as IBakedLoader;

			//what is this hot garbage?? I would like a special context for preparing which has more limited information
			bakeContext = new PipelineBakeContext()
			{
				Content = content,
				RawContentDiskRoot = directoryOwner.RawContentDiskRoot,
				ContentPath = name,
				RawContentDiskPath = Path.Combine(directoryOwner.RawContentDiskRoot, name),
				ForOven = directoryOwner.Manager.DumpBakedContent,
				Attributes = content.attributes ?? new object[0]
			};
			bakeable.Prepare(bakeContext);

			//TODO - dont even do this unless we support hot loading or arent bruted or something, I dont know
			bool resolvedDependencies = bakeContext.ResolveDependencies(name);

			//analyze whether we need to bake
			bool satisfied = true;

			//if the output doesnt exist, we do need to bake
			var fiTo = new FileInfo(fpBaked);
			if (!fiTo.Exists) satisfied = false;

			//check timestamps - we may need to bake if deps are out of date
			if (satisfied)
			{
				foreach (var dep in bakeContext.resolvedDependencies.Values)
				{
					var fiFrom = new FileInfo(dep);
					var fiFromTime = fiFrom.LastWriteTimeUtc;
					if (fiTo.LastWriteTimeUtc < fiFromTime)
					{
						satisfied = false;
						break;
					}
				}
			}

			//TODO: if we're the oven AND we're doing a clean operation, force to be unsatisfied

			if (satisfied)
			{
				if (directoryOwner.Manager.DumpBakedContent)
				{
					//it's already there. nothing to do
					//i GUESS this is what we can return
					return true;
				}

				//TODO: engine special functions to open files and report existence without two operations
				using (var fs = new FileStream(fpBaked, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					loadBakedContext.BakedReader = new BinaryReader(fs);
					loaded = bakedLoader.LoadBaked(loadBakedContext);
				}
			}

			//if it loaded, we're done
			if (loaded) return true;

			//if it didnt load, we need to load it raw
			//that consists actually of baking it, and if that succeeded, loading that as baked

			//if we couldnt resolve dependencies, we can't bake
			if (!resolvedDependencies)
				return false;

			var msBake = new MemoryStream();
			var bwBake = new BinaryWriter(msBake);

			bakeContext.BakedWriter = bwBake;

			bool baked = bakeable.Bake(bakeContext);

			//if it didnt bake, we definitely can't load it
			if (!baked) return false;

			bwBake.Flush();
			msBake.Position = 0;

			//dump newly baked content (depending on configuration)
			if (content.Manager.DumpBakedContent)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(fpBaked));
				using (var fs = new FileStream(fpBaked, FileMode.Create, FileAccess.Write, FileShare.None))
					fs.Write(msBake.GetBuffer(), 0, (int)msBake.Length);

				//unclear what default timestamp should be in case there's no inputs
				//probably need to set it equal to the build time of the executing assembly - pipe that in from the oven?
				DateTime timestamp = DateTime.MinValue;
				foreach (var dep in bakeContext.resolvedDependencies.Values)
				{
					var fiFrom = new FileInfo(dep);
					DateTime nextTs = fiFrom.LastWriteTimeUtc;
					if (nextTs > timestamp) timestamp = nextTs;
				}
				if (timestamp != DateTime.MinValue)
					fiTo.LastWriteTimeUtc = timestamp;

				//no actual loading to do in this case
				return true;
			}

			//now, load the baked content
			var bakedReader = new BinaryReader(msBake);
			loadBakedContext.BakedReader = bakedReader;
			loaded = bakedLoader.LoadBaked(loadBakedContext);

			//TODO: add sophisticated diagnostics log system and report this as a FAILED load

			//well, whether or not it worked, that's all we can do

			return loaded;
		}
	}

}