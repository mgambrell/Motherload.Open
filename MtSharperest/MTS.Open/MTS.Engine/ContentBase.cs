using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace MTS.Engine
{
	/// <summary>
	/// note: content is meant to be read-only
	/// to create content at runtime you should use a loader designed for the purpose?
	/// </summary>
	public abstract class ContentBase : IContentLoadable
	{
		/// <summary>
		/// Whether this content is loaded
		/// </summary>
		public bool IsLoaded { get; protected set; }

		/// <summary>
		/// Returns the short file name of the content (no extension, no path)
		/// </summary>
		public string Name { get { return _name; } }

		/// <summary>
		/// Returns the full content path of the content (no extension, but maybe /art/intro/flyingsaucer )
		/// TODO: this was a bad name
		/// TODO: can I put all this namespacing and pathing stuff inside some other kind of interface?
		/// Maybe load and unload too, put it inside some kind of .meta member?
		/// </summary>
		public string Path { get { return _path; } }

		/// <summary>
		/// Loads the content with its default loader
		/// </summary>
		public bool Load()
		{
			if (IsLoaded) return true;

			ContentLoadContext loadContext = new ContentLoadContext();
			loadContext.Content = this;

			IsLoaded = _loader.Load(loadContext);

			return IsLoaded;
		}

		internal bool LoadFromBakedInProgress(PipelineLoadBakedContext context)
		{
			((IContentBakeable)this).LoadBaked(context);
			return IsLoaded;
		}

		/// <summary>
		/// Unloads the content
		/// </summary>
		public void Unload()
		{
			//the loader stays set even after it's unloaded, because we can always load it again immediately.
			//setting up the loader is a more rare thing
			if (!IsLoaded) return;
			ContentUnload();
			IsLoaded = false;
		}

		//-------------------------------------------
		//inner operations
		//-------------------------------------------

		protected string _name, _path;
		internal object[] attributes;
		protected ContentBase _parent;
		internal IContentLoader _loader;
		ContentAttributesCollection _attributesCollection;

		internal ContentManager Manager { get; set; }

		/// <summary>
		/// The user sees Unload(), but that's managed by this ContentBase.
		/// Real implementations of content respond to ContentUnload
		/// </summary>
		protected virtual void ContentUnload() { }

		/// <summary>
		/// Approximately the constructor, but... this is executed immediately after the content is constructed
		/// This is required because the content needs to be setup by the manager before it can create child content.
		/// We could pass the loader we intend to use, which could be used as a hint about how to start creating.
		/// but that's getting kind of sloppy.
		/// </summary>
		internal virtual void ContentCreate() { }

		/// <summary>
		/// Accesses the attributes defined on the bound content fields
		/// </summary>
		public ContentAttributesCollection Attributes
		{
			get
			{
				if (_attributesCollection == null)
					_attributesCollection = new ContentAttributesCollection(attributes);
				return _attributesCollection;
			}
		}

		/// <summary>
		/// Please define 'bind'. I think it means.. fields are bound to other things
		/// </summary>
		protected virtual void BindManifest(ContentManifestEntry manifest)
		{
			//TODO: consider: prevent from being bound more than once. That's just illegal.
			//just keep a bool for that purpose? What a waste.
			//What if the binding is just so internal to the engine that it's not possible for a user to do it more than once?
			//That would be preferred

			var myType = GetType();

			attributes = manifest.Attributes;

			//bind all children
			if (manifest.Children != null)
			{
				foreach (var child in manifest.Children)
				{
					var name = child.Name;

					//even though the manifest may have come from reflection...
					//it also may NOT have. so we need to reflect again in order to do the binding
					var fieldInfo = myType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					//check the current value. if it's null, we need to instantiate a proxy
					var o = fieldInfo.GetValue(this);
					if (o == null)
					{
						o = CreateBoundContentProxy(fieldInfo.FieldType, name);
						fieldInfo.SetValue(this, o);
					}

					//bind this child
					var childContentField = o as ContentBase;
					if (childContentField == null)
					{
						//if it isn't content, then it is a subcontent holder
						//but what to do about that? we need to set its members
						//our old concept was to automatically create directories here. but that's actually inappropriate..
						//if the holder were also a directory, we would automatically do it..
						//--although I would need to make sure directories hosted on other things work
						//and really, that's pretty bad
						//maybe I need a special process here that goes off into the great beyond filling in unknown types...
						//but.. uh.. it could be pretty cool to have these be directories. maybe lets try it and see how it goes.
						//or maybe a new type that can enumerate its members
					}
					else
						childContentField.BindManifest(child);
				}
			}
		}

		protected T CreateContentProxy<T>(string name = null) where T:ContentBase
		{
			var type = typeof(T);
			return (T)CreateContentProxy(type, name ?? type.Name);
		}

		protected ContentBase CreateContentProxy(Type type, string name = null)
		{
			var content = Manager.CreateContentProxy(type, name);
			content._name = name ?? type.Name;
			content._path = this.Name + "/" + (name ?? type.Name);
			content._parent = this;
			return content;
		}

		/// <summary>
		/// Creates a content proxy which is.. 'bound'.. to a field?
		/// That's the intent, but it looks like the different logic in here isn't directly related to that.
		/// It should be possible to do oven stuff without... man, I dont know
		/// </summary>
		protected virtual ContentBase CreateBoundContentProxy(Type type, string name)
		{
			ContentBase content = CreateContentProxy(type, name);

			var bakeable = content as IContentBakeable;

			//if it's bakeable, set it up with an oven
			//this will be our loader for the future, but we do not load it yet!
			//therefore we have to set the loader by setting the secret internal field
			if (bakeable != null)
			{
				content._loader = new OvenContentLoader(content)
				{
					bakeable = bakeable,
					directoryOwner = this as ContentDirectory, //I guess we know this is true
					name = name
				};
			}

			return content;
		}

	} 

}


