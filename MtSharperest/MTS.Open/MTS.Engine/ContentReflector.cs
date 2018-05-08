using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace MTS.Engine
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SubcontentAttribute : Attribute
	{
	}

	/// <summary>
	/// Reflects over an in-game content structure to produce the manifest
	/// </summary>
	public class ContentReflector
	{
		public static ContentManifestEntry Reflect(Type targetType)
		{
			return _Reflect(targetType, false);
		}

		static ContentManifestEntry _Reflect(Type targetType, bool inContent)
		{
			bool iAmDirectory = targetType.IsSubclassOf(typeof(ContentDirectory));

			ContentManifestEntry ret = new ContentManifestEntry()
			{
				Children = new List<ContentManifestEntry>()
			};

			foreach (var fi in targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var fieldType = fi.FieldType;
				bool isContentType = fieldType.IsSubclassOf(typeof(ContentBase));
				bool isContentDirectory = fieldType.IsSubclassOf(typeof(ContentDirectory));
				bool isSubcontentHolder = fi.GetCustomAttributes<SubcontentAttribute>().Any();

				if (isContentDirectory)
				{
					var entry = _Reflect(fi.FieldType, inContent);
					entry.Name = fi.Name;
					entry.Attributes = fi.GetCustomAttributes(false); //warning: this may affect how the directory is interpreted, so maybe it should do this
					ret.Children.Add(entry);
				}
				else if (isContentType)
				{
					//content can have subcontent holders, so we need to recurse into that
					var entry = _Reflect(fieldType, true);
					entry.Name = fi.Name;
					entry.Attributes = fi.GetCustomAttributes(false);
					ret.Children.Add(entry);
				}
				else if (isSubcontentHolder)
				{
					//subcontent must be under content; moreover it must be legal for that content type (todo)
					if (!inContent)
					{
						throw new Exception("Structural error: subcontent outside of content"); //todo: just make an informative log message
						//continue;
					}

					var entry = _Reflect(fieldType, true);
					entry.Name = fi.Name;
					entry.Attributes = fi.GetCustomAttributes(false);
					ret.Children.Add(entry);
				}
				else
				{
					if (inContent) { }
					else if (iAmDirectory) { }
					else throw new Exception("Non-content found in content manifest type");
				}
			}

			return ret;
		}
	}
}

