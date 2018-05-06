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


