using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace MTS.Engine
{
	public class ContentAttributesCollection
	{
		object[] attributes;
		Dictionary<string, object> _memos;

		internal ContentAttributesCollection(object[] attributes)
		{
			this.attributes = attributes;

			foreach (var attr in attributes)
			{
				var m = attr as MemoAttribute;
				if (m == null) continue;

				if (_memos == null) _memos = new Dictionary<string, object>();
				_memos[m.Key] = m.Value;
			}
		}

		/// <summary>
		/// Indicates whether this content has been manifested with the given attribute type
		/// TODO: collect/hide this under an Attributes member? probably a good idea; these will rarely be used.
		/// could put memos in there too.
		/// REMINDER: I need to transform memos into another data type. that's because, they will be serialized through the pipeline
		/// So this whole notion of accessing attributes directly is no good.
		/// </summary>
		public bool HasAttribute<T>() { return HasAttribute(typeof(T)); }

		public T GetAttribute<T>() where T: Attribute{ return (T)GetAttribute(typeof(T)); }

		public Attribute GetAttribute(Type type)
		{
			foreach (var attr in attributes)
				if (attr.GetType() == type)
					return (Attribute)attr;
			return null;
		}

		/// <summary>
		/// Indicates whether this content has been manifested with the given attribute type
		/// </summary>
		public bool HasAttribute(Type type)
		{
			if (attributes == null) return false;
			foreach (var attr in attributes)
				if (attr.GetType() == type)
					return true;
			return false;
		}

		/// <summary>
		/// All the memos defined on this content
		/// </summary>
		public Dictionary<string, object> Memos
		{
			get
			{
				if (_memos != null) return _memos;

				//well, the user wants memos, even though there are none (the _memos dict would have been made already)
				//build an empty memos struct, I guess
				return _memos = new Dictionary<string, object>();
			}
		}

	} //class ContentManager
}


