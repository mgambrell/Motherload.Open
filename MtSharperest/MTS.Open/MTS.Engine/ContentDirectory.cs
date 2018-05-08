using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace MTS.Engine
{
	/// <summary>
	/// A directory of content. Contains a flat list of other content (possibly subdirectories) but no grandchildren nodes
	/// Is not directly enumerable, because we don't want linq to fill up intellisense with garbage (the focus should be on the contained content items)
	/// There are other mechanisms provided for enumeration though.
	/// </summary>
	public class ContentDirectory : ContentBase, IContentLoader
	{
		Dictionary<string, ContentBase> childContent = new Dictionary<string, ContentBase>();
		Dictionary<string, ContentManifestEntry> Manifest = new Dictionary<string, ContentManifestEntry>();

		protected ContentDirectory() {
			_loader = this;
		}

		/// <summary>
		/// The path to the root directory of the raw content on disk
		/// </summary>
		internal string RawContentDiskRoot;

		/// <summary>
		/// The path to the root directory of the baked content on disk
		/// </summary>
		internal string BakedContentDiskRoot;

		/// <summary>
		/// The ContentManager that owns this
		/// </summary>
		internal ContentManager RootContentManager;

		/// <summary>
		/// Logical path of this directory (should be empty string for top level directory; we don't use a leading / anywhere, but maybe we should swallow those later)
		/// </summary>
		public string LogicalPath { get; private set; }

		/// <summary>
		/// Enumerates all the child content.
		/// These arent KeyValuePairs because the content's name is whatever the key would be
		/// There isn't any further information to return, so this just returns the content directly
		/// TODO: yield enumerator is overkill, probably.
		/// </summary>
		public IEnumerable<ContentBase> EnumerateContent(bool recurse = false)
		{
			foreach (var content in childContent.Values)
			{
				if (recurse)
				{
					if (content is ContentDirectory)
					{
						foreach (var child in ((ContentDirectory)content).EnumerateContent(true))
							yield return child;
					}
					yield return content;
				}
				else 
					yield return content;
			}
		}

		bool IContentLoader.Load(ContentLoadContext context)
		{
			bool ok = true;

			//load all child content
			foreach (var item in childContent.Values)
				ok &= item.Load();

			return ok;
		}

		protected override void ContentUnload()
		{
			//unload all child content
			foreach (var item in childContent.Values)
				item.Unload();
		}

		protected override ContentBase CreateBoundContentProxy(Type type, string name, ContentManifestEntry manifestEntry)
		{
			//special handling for directories--we need to create subdirectories specially
			//EDIT: do we really? can't it get dealt with, like, by tracking the directory owner, and pulling information from that?
			var content = base.CreateBoundContentProxy(type, name, manifestEntry);


			if (typeof(ContentDirectory).IsAssignableFrom(type))
			{
				var newSubdir = (ContentDirectory)content;
				newSubdir.RawContentDiskRoot = System.IO.Path.Combine(RawContentDiskRoot, name);
				newSubdir.BakedContentDiskRoot = System.IO.Path.Combine(BakedContentDiskRoot, name);

				//special handling:
				var backendSubdirAttrib = content.Attributes.GetAttribute<BackendSubdirectory>();
				if (backendSubdirAttrib != null)
				{
					newSubdir.RawContentDiskRoot = System.IO.Path.Combine(newSubdir.RawContentDiskRoot, Manager.SelectedBackend.ToString());
					newSubdir.BakedContentDiskRoot = System.IO.Path.Combine(newSubdir.BakedContentDiskRoot, Manager.SelectedBackend.ToString());
				}


				newSubdir.LogicalPath = $"{LogicalPath}/{name}";

				//it's responsible for loading itself.. kind of weird..
				//that's different from other content (which is concretely realized on the filesystem)
				newSubdir._loader = newSubdir;
			}

			//we need to track all the content we create, that's one of the purposes of a directory
			childContent[name] = content;

			return content;
		}


		//man, I really think.. maybe this should be some kind of external utility to resolve a nested path
		//I did.. kind of want to establish this approach, but.. I dont know. let's find out how useful it is.
		//THIS IS OLD AND CRUSTY, NOT SURE ABOUT IT
		public ContentBase Get(string path)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentException("Null or empty path");

			var pathParts = path.Split('/');

			ContentBase o;
			childContent.TryGetValue(pathParts[0], out o);

			if (o == null)
			{
				throw new InvalidOperationException("Unknown path...");
			}

			//if we have more than one part, then we need to recurse into a subdirectory
			if (pathParts.Length > 1)
			{
				ContentDirectory subdir = o as ContentDirectory;

				//we better have a subdirectory
				if (subdir == null)
					throw new InvalidOperationException("Attempt to reference a subdirectory name known to be some other content type");

				path = string.Join("/", pathParts, 1, pathParts.Length - 1);
				return subdir.Get(path);
			}

			if (o == null)
			{
				throw new InvalidOperationException("nonexistent path");
			}

			return o;
		}

		/// <summary>
		/// Loads content at the given path, and returns it
		/// This is a special feature of directory... I'm not sure how useful it will be
		/// Originally I thought any content might support it.. maybe it could, if I make content support a child enumeration feature
		/// The idea was, it could be like unity and the content tree or something. Well.. worry about it later
		/// </summary>
		public object Load(string path)
		{
			var content = Get(path);
			content.Load();
			return content;
		}

		/// <summary>
		/// See comment on non-generic version
		/// </summary>
		public T Load<T>(string path)
		{
			return (T)Load(path);
		}

		internal void DoBindManifest(ContentManifestEntry manifest)
		{
			BindManifest(manifest);
		}
	}

}

