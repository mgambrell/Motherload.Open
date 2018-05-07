using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace MTS.Engine
{
	/// <summary>
	/// Art is basically a texture, with a little extra information.
	/// This includes texture coordinates, and information about automatic cropping that's done at bake-time.
	/// For instance a small piece of art in a large 1080p bitmap will end up being a small texture, with an offset
	/// TODO: experiment with a mechanism to allow us to configure alternate importers, possibly pre- and post-processing paths (whatever that means)
	/// (but couldnt we do that with a content type that inherits from art?)
	/// </summary>
	public class Art : ContentBase, IBakedLoader, PrivateInterfaces.IArtLoaders
	{
		protected int mWidth, mHeight;

		//meh, i want to make sure trivial getters optimized out in brute
		//leaving these here as an ugly reminder to check that
		public float umin, vmin, umax, vmax;
		public int ox, oy;

		/// <summary>
		/// if the art is owend by something else, it should not unload the texture
		/// maybe I should standardize a concept of ownership and do that automatically? not sure
		/// </summary>
		protected bool owned;

		/// <summary>
		/// The logical width of the art
		/// </summary>
		public int Width { get { return mWidth; } }

		/// <summary>
		/// The logical height of the art
		/// </summary>
		public int Height { get { return mHeight; } }

		/// <summary>
		/// The texture used by this art
		/// </summary>
		public Texture Texture { get; protected set; }

		internal override void ContentCreate()
		{
			//we manage our own texture
			//that is, unless this is going to be owned by something else... then this is a waste of time... ugh.
			Texture = CreateContentProxy<Texture>();
		}

		protected override void ContentUnload()
		{
			Console.WriteLine("Art.MyUnload: " + "???");
			if (!owned)
				Texture.Unload();
		}

		bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			Console.WriteLine("Art.LoadBaked: " + context.ContentPath);

			var br = context.BakedReader;

			Manager.EmbeddedLoadBaked(Texture, context);

			mWidth = br.ReadInt32();
			mHeight = br.ReadInt32();
			ox = br.ReadInt32();
			oy = br.ReadInt32();
			umin = br.ReadSingle();
			vmin = br.ReadSingle();
			umax = br.ReadSingle();
			vmax = br.ReadSingle();

			return true;
		}


		// <summary>
		// Creates a solid colored Art resource with these characteristics
		// </summary>
		public static Art CreateSolidColor(ContentManager manager, int width, int height, uint color)
		{
			var textureLoader = new ContentLoaderProxy(
				(ContentLoadContext context) =>
				{
					var texture = context.Content as Texture;
					((PrivateInterfaces.ITextureLoaders)texture).LoadSolidColor(width, height, color);
					return true;
				}
			);

			var artLoader = new ContentLoaderProxy(
				(ContentLoadContext context) =>
				{
					var art = context.Content as Art;
					art.mWidth = width;
					art.mHeight = height;
					manager.LoadWith(art.Texture, textureLoader);
					return true;
				}
			);

			return manager.CreateWith<Art>(artLoader);
		}


		void PrivateInterfaces.IArtLoaders.LoadParamsOwned(int width, int height, float umin, float vmin, float umax, float vmax, Texture texture)
		{
			this.mWidth = width;
			this.mHeight = height;
			this.umin = umin;
			this.vmin = vmin;
			this.umax = umax;
			this.vmax = vmax;
			this.owned = true;
			this.Texture = texture;
		}
	}

}

