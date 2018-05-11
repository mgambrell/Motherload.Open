using System;
using System.IO;
using MTS.Engine;

using IniParser;
using IniParser.Model;


namespace MTS.Engine.Pipeline.Pipelines
{
	public class ConfigFilePipeline : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context)
		{
			var configFile = context.Content as ConfigFile;

			//we're going to need reflection eventually... 
			configFile.Reflect();
			context.Depend("source", context.RawContentDiskPath + ".ini");
		}

		public bool Bake(PipelineBakeContext context)
		{
			var configFile = context.Content as ConfigFile;

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

			foreach (var mfi in configFile.reflectionData.Values)
			{
				string val;
				bool has = parsedData.TryGetKey(mfi.FieldInfo.Name, out val);
				if (!has) continue;
				switch (mfi.Type)
				{
					case ConfigFile.FieldType.String:
						DumpValue(bw, mfi);
						mfi.FieldInfo.SetValue(this, val);
						count++;
						break;
					case ConfigFile.FieldType.Int32:
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
					case ConfigFile.FieldType.Float:
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

		void DumpValue(BinaryWriter writer, ConfigFile.MyFieldInfo mfi)
		{
			writer.Write(mfi.FieldInfo.Name);
			writer.Write((int)mfi.Type); 
		}

	}

}