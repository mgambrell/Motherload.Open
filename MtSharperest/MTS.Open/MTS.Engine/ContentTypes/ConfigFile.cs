using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using IniParser;
using IniParser.Model;

//todo: support arrays? (comma separated)
//todo: support arrays? (Item0= Item1= Item2= etc.?) [would specify via attribute, some kind of flatten or merge command]


namespace MTS.Engine
{
	public class ConfigFile : ContentBase, IContentBakeable
	{
		Dictionary<string,MyFieldInfo> reflectionData;

		class MyFieldInfo
		{
			public FieldType Type;

			//keep this around so we can poke the values
			//this is not a lightweight apparatus!
			public FieldInfo FieldInfo;
		}

		enum FieldType
		{
			Unknown,
			String,
			Int32,
			Float,
		}

		void IContentBakeable.Prepare(PipelineBakeContext context)
		{
			//we're going to need reflection eventually... 
			if (reflectionData == null) Reflect();
			context.Depend("source",context.RawContentDiskPath + ".ini");
		}

		protected override void ContentUnload()
		{
			//nothing we HAVE to do here.. but...
			//we will clear out all the reference type fields (strings and arrays)
			//it's hard to beat that
			foreach (var kvp in reflectionData)
			{
				if (!kvp.Value.FieldInfo.FieldType.IsValueType)
					kvp.Value.FieldInfo.SetValue(this, null);
			}
		}

		unsafe bool IContentBakeable.Bake(PipelineBakeContext context)
		{
			var zoowriter = context.BakedWriter;

			var path = context.Depends["source"];

			var iniConfig = new IniParser.Model.Configuration.IniParserConfiguration()
			{
				CaseInsensitive = true
			};
			var iniParser = new IniParser.Parser.IniDataParser(iniConfig);
			FileIniDataParser fileIniData = new FileIniDataParser(iniParser);
			IniData parsedData = fileIniData.ReadFile(path);
			//foreach (var key in parsedData.Global)

			var ms = new MemoryStream();
			var bw = new BinaryWriter(ms);

			int count = 0;

			foreach (var mfi in reflectionData.Values)
			{
				string val;
				bool has = parsedData.TryGetKey(mfi.FieldInfo.Name, out val);
				if (!has) continue;
				switch (mfi.Type)
				{
					case FieldType.String:
						DumpValue(bw, mfi);
						mfi.FieldInfo.SetValue(this, val);
						count++;
						break;
					case FieldType.Int32:
					{
							int temp;
							if (int.TryParse(val, out temp))
							{
								count++;
								DumpValue(bw, mfi);
								bw.Write(temp);
							}
							break;
					}
					case FieldType.Float:
					{
							float temp;
							if (float.TryParse(val, out temp))
							{
								count++;
								DumpValue(bw, mfi);
								bw.Write(temp);
							}
							break;
					}
				}
			}

			bw.Flush();
			zoowriter.Write(count);
			zoowriter.Write(ms.ToArray());

			return true;
		}

		void DumpValue(BinaryWriter writer, MyFieldInfo mfi)
		{
			writer.Write(mfi.FieldInfo.Name);
			writer.Write((int)mfi.Type); 
		}

		bool IContentBakeable.LoadBaked(PipelineLoadBakedContext context)
		{
			var reader = context.BakedReader;

			int count = reader.ReadInt32();

			for (int i = 0; i < count; i++)
			{
				string name = reader.ReadString();
				FieldType ftReal = (FieldType)reader.ReadInt32();

				//we're doign a lot of double checking here, so nothing bad happens if we load old data on a newer config struct
				//This might not be necessary...
				//but I need to make config data 'depend' on the .dll 
				//right now if the dll changes, the content might not rebuild (because the output would only be depending on the input ini)
				//so that's one reason we need the extra security here
				MyFieldInfo mfi;
				reflectionData.TryGetValue(name, out mfi);

				object data;
				switch (ftReal)
				{
					case FieldType.String: data = reader.ReadString(); break;
					case FieldType.Int32: data = reader.ReadInt32(); break;
					case FieldType.Float: data = reader.ReadSingle(); break;
					default: throw new InvalidOperationException();
				}

				if (ftReal == mfi.Type)
					mfi.FieldInfo.SetValue(this, data);
			}

			return true;
		}

		void Reflect()
		{
			reflectionData = new Dictionary<string, MyFieldInfo>();
			HashSet<string> names = new HashSet<string>();

			foreach (var fi in GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var mfi = new MyFieldInfo();
				mfi.FieldInfo = fi;

				var lower = mfi.FieldInfo.Name.ToLowerInvariant();

				if (names.Contains(lower))
				{
					Console.WriteLine("Duplicate field name on ConfigFile. We're working case insensitive here, so dont use `Foo` and `foo` both");
					continue;
				}

				names.Add(lower);

				if (fi.FieldType == typeof(int)) mfi.Type = FieldType.Int32;
				if (fi.FieldType == typeof(string)) mfi.Type = FieldType.String;
				if (fi.FieldType == typeof(float)) mfi.Type = FieldType.Float;

				reflectionData[fi.Name] = mfi;
			}

		}

	}
}

