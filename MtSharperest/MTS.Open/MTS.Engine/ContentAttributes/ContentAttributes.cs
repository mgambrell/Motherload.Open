////TODO: must have custom binary reader (for reading to pointers, endian handling, etc.)

using System;

namespace MTS.Engine
{
	//TODO: put in a Content class? so I can use Art.MemoForArt and Content.MemoForAnyContent?
	[AttributeUsage(AttributeTargets.Field, AllowMultiple=true)]
	public class MemoAttribute : Attribute
	{
		public string Key;
		public object Value;
		public MemoAttribute(string key, object value)
		{
			this.Key = key;
			this.Value = value;
		}
	}

	/// <summary>
	/// Sets this content (should only be a directory for now) to bounce to a subdirectory named after the backend
	/// Or you could look at it this way: chooses one subdirectory only, and imports its contents into this one
	/// Intended to be used for shaders..
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class BackendSubdirectory : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class TextureFormatAttribute : Attribute
	{
		public TextureFormat Format;
		public TextureFormatAttribute(TextureFormat format)
		{
			this.Format = format;
		}
	}



}


