//it would not be possible right now to have animation cells/anims organized in subdirectories...
//I should try making that happen

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace MTS.Engine.ContentTypes
{
	public class AnimSet : AnimSet<EMPTYDIR, EMPTYDIR>
	{
	}

	public class AnimSet<CELLSTYPE, ANIMSTYPE> : ContentBase, IBakedLoader
		where CELLSTYPE: ContentDirectory
		where ANIMSTYPE : ContentDirectory
	{
		/// <summary>
		/// The AnimSet's cells
		/// </summary>
		[Subcontent]
		public CELLSTYPE Cells;

		/// <summary>
		/// The AnimSet's animations
		/// </summary>
		[Subcontent]
		public ANIMSTYPE Anims;

		protected Dictionary<string, AnimCell> cells = new Dictionary<string, AnimCell>();

		internal override void ContentCreate()
		{
			texture = CreateContentProxy<Texture>();
		}

		/// <summary>
		/// the (currently, only one) texture for the animset
		/// </summary>
		protected Texture texture;


		bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			Console.WriteLine("AnimSet.LoadBaked: " + context.ContentPath);

			Manager.EmbeddedLoadBaked(texture, context);

			int nCells = context.BakedReader.ReadInt32();
			for (int i = 0; i < nCells; i++)
			{
				string name = context.BakedReader.ReadString();
				AnimCell cell = null;
				if (cells.ContainsKey(name)) cell = cells[name];
				else cell = CreateContentProxy<AnimCell>(name);
				cells[name] = cell;
				((PrivateInterfaces.IArtLoaders)cell.Art).LoadParamsOwned(3, 3, 0, 0, 1, 1, texture);
			}

			return true;
		}

		protected override void ContentUnload()
		{
			
		}

		protected override void BindManifest(ContentManifestEntry manifest)
		{
			base.BindManifest(manifest);

			//gain awareness of everything that was bound
			//(this is directory-like behaviour, is there anything we can reuse?)
			foreach (var cell in Cells.EnumerateContent()) cells[cell.Name] = (AnimCell)cell;
		}
	}

	//idea: put AnimCell inside AnimSet, but then, alias it out here with a friendlier name
	//see how well that works for making protected stuff work
	public class AnimCell : ContentBase
	{
		/// <summary>
		/// The cell's art
		/// </summary>
		public Art Art { get; private set; }

		internal override void ContentCreate()
		{
			Art = CreateContentProxy<Art>();
		}

		protected override void ContentUnload()
		{
		}
	}


}

