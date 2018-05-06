//TODO - move to some kind of Proto directory

#if !BRUTED

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MTS.Engine
{
	public class HotloadManager
	{
		ContentManager contentManager;

		public HotloadManager(ContentManager contentManager)
		{
			this.contentManager = contentManager;
		}

		Dictionary<ContentDirectory, MTS.Engine.Host.SmartFileSystemWatcher> watchers = new Dictionary<ContentDirectory, Host.SmartFileSystemWatcher>();
		List<string> hotPaths = new List<string>();

		public void AddDirectory(ContentDirectory directory)
		{
			if (watchers.ContainsKey(directory)) throw new InvalidOperationException("Do not watch a ContentDirectory more than once");
			var watcher = new MTS.Engine.Host.SmartFileSystemWatcher(500, directory.RawContentDiskRoot);
			watchers[directory] = watcher;
		}

		public void RemoveDirectory(ContentDirectory directory)
		{
			if (!watchers.ContainsKey(directory)) throw new InvalidOperationException("Do not watch a ContentDirectory more than once");
			watchers[directory].Dispose();
			watchers.Remove(directory);
		}

		public void Tick()
		{
			foreach (var kvp in watchers)
				TickHotloadWatcher(kvp.Key, kvp.Value);
		}

		void TickHotloadWatcher(ContentDirectory cd, MTS.Engine.Host.SmartFileSystemWatcher watcher)
		{
			hotPaths.Clear();
			watcher.GetPathsInto(hotPaths);

			if (hotPaths.Count == 0) return;

			//build a list of all hotloadable content (i.e. all oven bakeable content -- that's loaded)
			var hotloadables = cd.EnumerateContent(true).ToList();
			hotloadables = hotloadables.Where(c => c._loader is OvenContentLoader && c.IsLoaded).ToList();

			List<ContentBase> toReload = new List<ContentBase>();

			foreach (var path in hotPaths)
			{
				//currently deps are tracked as full filenames
				//that may be better... or we may need both
				//var subpath = path.Substring(cd.RawContentDiskRoot.Length).Replace('\\','/');
				var subpath = path;

				Console.WriteLine("Hotload Candidate: " + subpath);

				//look for any content which depended (or might depend on?) this
				//UPDATE: we're kind of 'scanning' again so stuff can re-bind to the newly-changed filesystem
				//this might not be scalable. ALSO this shouldnt be done for every hot path
				//we should kick off a new asynchronous prepare job on all content, and queue a reload as needed after that's complete
				//IDEA: we could remember everything that the content tried to resolve (not what got finally resolved) and check against that instead.
				//But, in some rare cases (more complex level data) assumptions about the dependencies will be hard to fulfil
				//Oh well, spend the energy multi-threading it instead (even run it asynchronously.. should be safe.)
				foreach (var hotloadable in hotloadables)
				{
					OvenContentLoader loader = hotloadable._loader as OvenContentLoader;

					//TODO: this is bad, what if this never ran? need to rethink some things
					loader.bakeContext.resolvedDependencies = new Dictionary<object, string>();
					loader.bakeContext.dependsBag = new Bag<object, PipelineBakeContext.DependsRecord>();
					loader.bakeable.Prepare(loader.bakeContext);
					//junk
					bool resolvedDependencies = loader.bakeContext.ResolveDependencies(hotloadable.Name);
					if (!resolvedDependencies) continue;

					//TODO : check resolved dependencies
					var resolvedDeps = loader.bakeContext.resolvedDependencies.Values.ToArray();
					if (resolvedDeps.Contains(subpath))
					{
						toReload.Add(hotloadable);
					}
				}

				////look for the matching content
				//int index = hotloadable.FindIndex(HL => HL.ContentPath == subpath);
				//Console.WriteLine(index);
			}

			//reload all content (should happen in several threads)
			foreach (var content in toReload)
			{
				content.Unload();
				content.Load();
			}

		} //TickHotloadWatcher

	}
}


#endif