using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using IniParser;
using IniParser.Model;

namespace MTS.Engine
{
	public class NitroFont : ContentBase, IContentBakeable
	{
		Dictionary<int, PipelineLetterRecord> letterRecordsDict = new Dictionary<int, PipelineLetterRecord>();

		/// <summary>
		/// A bloated structure used just during processing
		/// </summary>
		class PipelineLetterRecord
		{
			public PipelineLetterRecord() { }
			public PipelineLetterRecord(int c, int x, int y, int width, int height, int logw, int xo, int yo) { this.c = c; this.x = x; this.y = y; this.width = width; this.height = height; this.logw = logw; this.xo = xo; this.yo = yo; }
			public PipelineLetterRecord(PipelineLetterRecord lr)
			{
				this.c = lr.c;
				this.x = lr.x;
				this.y = lr.y;
				this.width = lr.width;
				this.height = lr.height;
				this.logw = lr.logw;
				this.xo = lr.xo;
				this.yo = lr.yo;
			}
			public int c, x, y, width, height, logw, xo, yo;
			public bool missing;
			public PipelineLetterRecord lrReference;

			public TexAtlas.RectItem rectItem;
		}

		/// So for ex

		void IContentBakeable.Prepare(PipelineBakeContext context)
		{
			var path = context.RawContentDiskPath;

			context.DependOptional("source", path + ".txt");

			//go ahead and check the existence of various source art formats we can read
			//TODO: make this common logic
			//TODO: make this not just png
			string ext = ".png";

			//add deps for all possible input pages
			for (int i = 0; i < 256; i++)
			{
				context.DependOptional(i, $"{path}_{i.ToString("X2")}{ext}");
			}
		}

		bool IContentBakeable.Bake(PipelineBakeContext context)
		{
			for (int i = 0; i < 256; i++)
			{
				var resolved = context.Depends[i];
				if (resolved == null) continue;
				Console.WriteLine("Have font page: " + resolved);
			}
			return false;
		}

		bool IContentBakeable.LoadBaked(PipelineLoadBakedContext context)
		{
			return true;
		}

	}
}

