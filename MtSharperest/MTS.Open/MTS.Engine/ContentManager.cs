//TODO: pipeline stuff should have an option to hash dependencies, in case some of them are written spuriously

//TODO: must have custom binary reader (for reading to pointers, endian handling, etc.)

//TODO: must hash attributes and store in 

//TODO: as an option, automatically load all content when it's gotten (this would have to happen at binding time too)
//so.. im not sure if this is a good idea. my whole main idea of the content manifest system doesnt play well with it
//but ive made it so easy to load batches of stuff, you have no excuse

//IDEA: make a system that allows attributes to be entered in an accompanying .txt file
//they'll be automatically loaded just as if they were manifested in c#
//(this would be for content packs)
//RELATED IDEA: allow loading a .cs script from filesystem which acts as a content manifest, without needing it in-game
//RELATED IDEA: have an attribute for directories that will apply an attribute to all the contained content of a given type.. subject to a name glob/regex?
//that's.. advanced. Can definitely save that for later
//Honestly, this is so rare, that maybe I can count on someone making a tool to list the contents into .cs with all the correct metadata
//But I might find I want some of RELATED IDEAS for my own game (to avoid typing so many attributes
//RELATED IDEA: Attribute macros: group common attributes together in one place and apply them to content
//RELATEDLY -- YES, definitely. Applying special content options to just one subdirectory is a very common task.
//I must support that

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using MTS.Engine.Collections;

namespace MTS.Engine
{
	public class ContentManifestEntry
	{
		public string Name;

		public object[] Attributes;

		public List<ContentManifestEntry> Children;

		public void DebugTrace(Stream stream)
		{
			var writer = new StreamWriter(stream);
			_DebugTrace(writer, -1);
			writer.Flush();
		}

		void _DebugTrace(StreamWriter writer, int indent)
		{
			if (Name != null)
			{
				//huh? guess its the root
				for (int i = 0; i < indent; i++) writer.Write(' ');
				writer.WriteLine(Name);
			}

			foreach (var child in Children)
			{
				child._DebugTrace(writer, indent + 1);
			}
		}
	}

	/// <summary>
	/// Forwards IContentLoader to some other owner's methods
	/// </summary>
	class ContentLoaderProxy : IContentLoader
	{
		Func<ContentLoadContext, bool> load;

		public ContentLoaderProxy(Func<ContentLoadContext, bool> load)
		{
			this.load = load;
		}

		bool IContentLoader.Load(ContentLoadContext context)
		{
			return load(context);
		}
	}

	///// <summary>
	///// Forwards IContentLoader to an existing baked load context.
	///// Uhhh what? Let's try again.
	///// Allows you to immediately load one piece of content from the loading context of another
	///// </summary>
	//class ContentLoaderBakedEmbed : ContentLoaderProxy
	//{
	//	public ContentLoaderBakedEmbed(IContentBakeable target, PipelineLoadBakedContext context)
	//		: base(() => target.LoadBaked(context))
	//	{
	//	}
	//}


	public class PipelineBakeContext
	{
		/// <summary>
		/// The PipelineConnector in use
		/// </summary>
		public PipelineConnectorBase PipelineConnector;

		/// <summary>
		/// The content being baked
		/// </summary>
		public ContentBase Content;

		/// <summary>
		/// The path to the root directory of the raw content on disk
		/// </summary>
		public string RawContentDiskRoot;

		/// <summary>
		/// The logical path of the content to build
		/// </summary>
		public string ContentPath;

		/// <summary>
		/// Complete path to the expected source of the content
		/// (You may improvise the extension, or even gather several files together)
		/// You must use this as the current working directory your file IO (baking can be multi-threaded)
		/// </summary>
		public string RawContentDiskPath;

		/// <summary>
		/// The stream writer to be used for outputting the baked data
		/// </summary>
		public BinaryWriter BakedWriter;

		/// <summary>
		/// This is set if the baking is for the oven -- that is, don't _actually_ load anything, just bake it
		/// </summary>
		public bool ForOven;

		/// <summary>
		/// TODO: by setting this, we could allow some content loaders to skip serializing/deserializing
		/// this would speed things up! It takes special handling in each content type, but it will be worth it
		/// </summary>
		public bool ForHotload;

		/// <summary>
		/// Add an unnamed dependency to the given path
		/// </summary>
		public void Depend(string path)
		{
			Depend("unnamed",path);
		}

		/// <summary>
		/// Add a dependency to the given path, which is relative to the directory of the ContentPath
		/// In other words, if you're asked to build an Art called "jupiter", then Depend("jupiter.png");
		/// </summary>
		public void Depend(object tag, string path)
		{
			_Depend(false, tag, path);
		}

		/// <summary>
		/// Same as Depend(), but optional (no noise if file is missing)
		/// </summary>
		public void DependOptional(object tag, string path)
		{
			//TODO: make tag object
			_Depend(true, tag, path);
		}

		void _Depend(bool optional, object tag, string path)
		{
			var dr = new DependsRecord()
			{
				Path = path,
			};
			dependsBag.Add(tag, dr);

			bool check;
			if (optionalFlags.TryGetValue(tag, out check))
			{
				if (check != optional)
				{
					Console.WriteLine("Inconsistent optional flag for depends tag: " + tag);
				}
			}
			optionalFlags[tag] = optional;
		}

		internal class DependsRecord
		{
			public string Path;
		}

		internal Dictionary<object, bool> optionalFlags = new Dictionary<object, bool>();
		internal Bag<object, DependsRecord> dependsBag = new Bag<object, DependsRecord>();
		internal Dictionary<object, string> resolvedDependencies = new Dictionary<object, string>();
		DependsInterface resolvedDependenciesInterface;

		public class DependsInterface
		{
			Dictionary<object, string> resolvedDependencies;
			public DependsInterface(Dictionary<object, string> resolvedDependencies)
			{
				this.resolvedDependencies = resolvedDependencies;
			}

			public string this[object key]
			{
				get
				{
					string ret;
					if (resolvedDependencies.TryGetValue(key, out ret)) return ret;
					return null;
				}
			}
		}

		public DependsInterface Depends { get { return resolvedDependenciesInterface; } }

		public object[] Attributes;

		/// <summary>
		/// Set this if the operation failed
		/// </summary>
		public bool Failed;

		//lame, needs rethinking
		internal bool ResolveDependencies(string name)
		{
			//resolve dependencies -- look for first of each tag group and resolve to that if exactly one exists. Zero or many is an error
			bool didResolve = true;
			foreach (var key in dependsBag.Keys)
			{
				List<string> found = new List<string>();
				foreach (var val in dependsBag[key])
				{
					var path = val.Path;
					if (File.Exists(path))
						found.Add(path);
				}

				if (found.Count == 0)
				{
					bool flags;
					optionalFlags.TryGetValue(key, out flags);
					if (flags)
					{
						//it's optional, so no big deal
					}
					else
					{
						didResolve = false;
						Console.WriteLine($"{name}: No dependencies found");
					}
				}
				if (found.Count == 1)
				{
					//clean things up
					//actually we should normalize everything to front slashes, but thats a mess for another day
					//TODO: global path-cleaning logic?
					var depCleaned = found[0];
					depCleaned = depCleaned.Replace('/', '\\');
					resolvedDependencies[key] = depCleaned;
				}
				if (found.Count > 1)
				{
					didResolve = false;
					Console.WriteLine("Multiple dependency options found. This is an error.");
					foreach (var f in found)
						Console.WriteLine("* " + f);
				}
			}

			resolvedDependenciesInterface = new DependsInterface(resolvedDependencies);

			return didResolve;
		}
	}

	public class PipelineLoadBakedContext
	{
		/// <summary>
		/// The logical path of the content to build
		/// </summary>
		public string ContentPath;

		/// <summary>
		/// The stream reader to be used for reading the baked data
		/// </summary>
		public BinaryReader BakedReader;
	}

	public class EMPTYDIR : ContentDirectory { }

	/// <summary>
	/// it's unclear what the responsibilities here will be
	/// Maybe it's what manages the filesystem change monitoring and stuff
	/// Dependency checking / clean forcing, and stuff? There's a lot to do there.
	/// </summary>
	public class ContentManager
	{
		//Notice how the this is conditional on BRUTED.
		//This way you can work with the dumb default working directory of visual studio, 
		//while choosing something different for bruted console builds. 
		//One less thing to figure out when opening someone's game sln!
#if BRUTED
		//public static readonly string DefaultContentDirectory = "content"; //TODO: this is nonsense when bruted.
		//public static readonly string DefaultDataDirectory = "data";

			//for @#*@#('s sake
			public static readonly string DefaultContentDirectory = "/rom:/content"; //TODO: this is nonsense when bruted.
			public static readonly string DefaultDataDirectory = "/rom:/data";
#else
		public static readonly string DefaultContentDirectory = Directory.Exists(@"..\..\content") ? @"..\..\content" : "content";
		public static readonly string DefaultDataDirectory = "data";
#endif

		ProjectConfig ProjectConfig;
		internal PlatformType SelectedPlatform;
		internal BackendType SelectedBackend;

		public ContentManager()
		{
		#if PROTO
			hotloadManager = new HotloadManager(this);
		#endif

			//automatically select ProjectConfig from the entry assembly (not ideal)
			SelectProjectConfig(Assembly.GetEntryAssembly(), ConsoleEnvironment.Platform);
		}

		void SelectProjectConfig(Assembly fromAssembly, PlatformType forPlatform)
		{
			SelectedPlatform = forPlatform;

			//try loading ProjectConfig from the specified assembly; fallback to this assembly (use the default project config)
			var projectConfigType = fromAssembly.GetType("ProjectConfig", false);
			if (projectConfigType == null)
				projectConfigType = typeof(ProjectConfig);

			//now, we better have a project config
			ProjectConfig = (ProjectConfig)Activator.CreateInstance(projectConfigType);

			//choose the backend for the current platform
			SelectedBackend = ProjectConfig.Platforms[forPlatform].Backend;

			//try to set the default content connector
			//user can override it in a minute, in case that's important
			Type runtimeConnectorType = null;
			Type pipelineConnectorType = null;
			switch (SelectedBackend)
			{
				case BackendType.SDL:
					runtimeConnectorType = AppDomain.CurrentDomain.Load("MTS.Engine.SDL").GetType("MTS.Engine.SDL.DefaultRuntimeConnector", false);
					pipelineConnectorType = AppDomain.CurrentDomain.Load("MTS.Engine.SDL").GetType("MTS.Engine.SDL.DefaultPipelineConnector", false);
					break;
				case BackendType.Switch:
					runtimeConnectorType = AppDomain.CurrentDomain.Load("MTS.Engine.Switch").GetType("MTS.Engine.Switch.DefaultRuntimeConnector", false);
					pipelineConnectorType = AppDomain.CurrentDomain.Load("MTS.Engine.Switch").GetType("MTS.Engine.Switch.DefaultPipelineConnector", false);
					break;
			}

			//unless we're the oven, we need a runtime connector
			if (!ForOven && runtimeConnectorType != null)
			{
				RuntimeConnector = (RuntimeConnectorBase)Activator.CreateInstance(runtimeConnectorType);
			}

			//if we're the oven or proto, we need a pipeline connector (probably)
			if ((ForOven || forPlatform == PlatformType.Proto) && pipelineConnectorType != null)
			{
				//default pipeline connector needs these.. really this assembly should be referencing that one, but it would be circular
				Assembly.Load("MTS.Engine.Pipeline");
				PipelineConnector = (PipelineConnectorBase)Activator.CreateInstance(pipelineConnectorType);
			}
		}

		internal void StartupOven(Assembly fromAssembly, PlatformType forPlatform)
		{
			ForOven = true;
			SelectProjectConfig(fromAssembly, forPlatform);
			DumpBakedContent = true;
		}

		/// <summary>
		/// intended for use by the oven
		/// </summary>
		internal ContentManager(bool forOven)
		{
		}

		void SetBackend(BackendType backend)
		{
		
		}

		/// <summary>
		/// mounts everything with the default directories 'content' and 'data'
		/// </summary>
		public T Mount<T>() where T : ContentDirectory
		{
			return Mount<T>(DefaultContentDirectory, DefaultDataDirectory);
		}

		public T Mount<T>(string rawContentDiskRoot, string bakedContentDiskRoot) where T: ContentDirectory
		{
			return (T)Mount(typeof(T), rawContentDiskRoot, bakedContentDiskRoot);
		}

		public ContentDirectory Mount(Type t, string rawContentDiskRoot, string bakedContentDiskRoot)
		{
			var directoryInst = (ContentDirectory)Activator.CreateInstance(t);
			directoryInst.RawContentDiskRoot = rawContentDiskRoot;
			directoryInst.BakedContentDiskRoot = bakedContentDiskRoot;
			directoryInst.RootContentManager = this;
			directoryInst.Manager = this;

			var reflected = ContentReflector.Reflect(t);
			//using (var stdout = Console.OpenStandardOutput()) reflected.DebugTrace(stdout);
			directoryInst.DoBindManifest(reflected);

			return directoryInst;
		}

		/// <summary>
		/// ContentManager is responsible for creating proxies, because all content is ultimately owned by a ContentManager
		/// And also probably an owner ContentBase (except for the top-level directory)
		/// </summary>
		internal ContentBase CreateContentProxy(Type type, string name)
		{
			ContentBase content = (ContentBase)Activator.CreateInstance(type, true);
			content.Manager = this;
			content.ContentCreate();
			return content;
		}

		bool ForOven;
		public RuntimeConnectorBase RuntimeConnector;
		public PipelineConnectorBase PipelineConnector;

		/// <summary>
		/// Whether content is dumped when it's baked (this is lame, needs to be replaced with more sophistication)
		/// </summary>
		public bool DumpBakedContent;

		public CT CreateWith<CT>(IContentLoader loader) where CT : ContentBase
		{
			var type = typeof(CT);
			var content = (CT)CreateContentProxy(type, "dynamic " + type.Name);
			content._loader = loader;
			content.Load();
			return content;
		}

		public bool LoadWith(ContentBase content, IContentLoader loader)
		{
			content._loader = loader;
			return content.Load();
		}

		public bool EmbeddedLoadBaked(ContentBase content, PipelineLoadBakedContext context)
		{
			//older approach
			//var shim = new ContentLoaderBakedEmbed((IContentBakeable)content, context);
			//return LoadWith(content, shim);

			//this way we don't set the loader.. is that a problem? It's only a problem if we unload it.. and then call load again...
			//that's really not sensible in this case, so I guess it's OK to not have a loader
			//in that case also really only the owner should be able to unload it. we need some way to enforce that
			//for instance, what happens if you unload an animset cell? should be nothing.

			if (content.IsLoaded) content.Unload();
			return content.LoadFromBakedInProgress(context);
		}

#if PROTO
		HotloadManager hotloadManager;
		/// <summary>
		/// Call this (once per frame most likely) to reload changed content
		/// TODO: subpath is wrong for subdirectories, need to standardize path to /initial/slash
		/// </summary>
		public void TickHotload()
		{
			hotloadManager.Tick();
		}

		public void StartHotloadMonitor(ContentDirectory directory)
		{
			hotloadManager.AddDirectory(directory);
		}

		public void StopHotloadMonitor(ContentDirectory directory)
		{
			hotloadManager.RemoveDirectory(directory);
		}
#else
		public void StartHotloadMonitor(ContentDirectory directory) { }
		public void StopHotloadMonitor(ContentDirectory directory) { }
#endif

		[System.Diagnostics.Conditional("PROTO")]
		public void Bake()
		{
		}

	} //class ContentManager
}


