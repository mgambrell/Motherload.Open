using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using IniParser;
using IniParser.Model;

using MTS.Engine.ContentUtils;

namespace MTS.Engine
{
	public class NitroFont : ContentBase, IBakedLoader
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

			public RectItem rectItem;
		}

		

		bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			return true;
		}

	}
}

