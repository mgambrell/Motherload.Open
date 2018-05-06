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
	public class Art : ContentBase, IContentBakeable, PrivateInterfaces.IArtLoaders
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

		//-------------------------
		//IContentBakeable implementation

		void IContentBakeable.Prepare(PipelineBakeContext context)
		{
			var path = context.RawContentDiskPath;
			path += ".png";
			context.Depend("source", path);
		}

#if !BRUTED
		//TODO - this has really exposed the need to have this separated from instance methods on an art content instance
		//i mean, it really has nothing at all to do with the content
		//the baker should be a totally separate interface, so we're not encouraged to put it here (however, it is OK if we do)
		//unfortuately, right now only a content instance knows which object type can be used to bake it.
		//we would need some kind of reflection index of bakers for content types. can we put it in an attribute so it doesnt need to be manually registered?
		unsafe bool IContentBakeable.Bake(PipelineBakeContext context)
		{
			Console.WriteLine("Art.Bake: " + context.ContentPath);

			float umin = 0;
			float vmin = 0;
			float umax = 1;
			float vmax = 1;

			var path = context.Depends["source"];

			var imageInfo = ImageAnalyzer.AnalyzeImageInfo(path);

			//TODO: we can only handle certain input formats here (no compressed formats)
			//but we can output any format (after we crop it)

			int width = imageInfo.Width;
			int height = imageInfo.Height;
			int physwidth = width;
			int physheight = height;
			int ox = 0, oy = 0;

			//NOTE: EVERYTHING BELOW IS EXPERIMENTAL. ITS A TOTAL MESS

			//TODO - apply art-specific operations (cropping, etc.)
			//TODO - make controllable
			//TODO - handle errors
			var conversionResult = imageInfo.ConvertToAlphaProcessableFormat(false);

			bool doTrim = true;
			if (conversionResult.ConversionResult == ConversionResult.Error_InputFormatHasNoAlpha) doTrim = false;

			if (doTrim)
			{
				//accept the converted image
				imageInfo = conversionResult.ResultImage;

				var alphaTrimResult = imageInfo.AlphaTrim();
				imageInfo = alphaTrimResult.ResultImage;

				ox = alphaTrimResult.x;
				oy = alphaTrimResult.y;
				physwidth = alphaTrimResult.Width;
				physheight = alphaTrimResult.Height;
			}

			bool doPadPow2 = true;
			if (!imageInfo.IsAlphaProcessableFormat()) doPadPow2 = false;
			if (doPadPow2)
			{
				int widthRound = PipelineMath.TextureUptoPow2(physwidth);
				int heightRound = PipelineMath.TextureUptoPow2(physheight);
				if (widthRound != physwidth || heightRound != physheight)
				{
					imageInfo = imageInfo.ExpandDownRight(widthRound, heightRound);
				}
			}

			var fmtAttribute = context.Attributes.FirstOrDefault(a => a is TextureFormatAttribute);
			if (fmtAttribute != null)
			{
				var toFormat = ((TextureFormatAttribute)fmtAttribute).Format;

				ImageConversionContext imageContext = new ImageConversionContext();
				imageContext.From = imageInfo;
				imageContext.NewAlpha = 0xFF;
				imageContext.ToFormat = toFormat;
				ImageAnalyzer.Convert(imageContext);
				imageInfo = imageContext.Output;
			}


			umax = (ox + physwidth) / imageInfo.Width;
			vmax = (oy + physheight) / imageInfo.Height;

			//the bitmap goes first...
			imageInfo.Serialize(context.BakedWriter);

			//..then art-specific stuff
			context.BakedWriter.Write(width);
			context.BakedWriter.Write(height);
			context.BakedWriter.Write(ox);
			context.BakedWriter.Write(oy);
			context.BakedWriter.Write(umin);
			context.BakedWriter.Write(vmin);
			context.BakedWriter.Write(umax);
			context.BakedWriter.Write(vmax);

			return true;
		}
#else
		unsafe bool IContentBakeable.Bake(PipelineBakeContext context) { return false; }
#endif
		bool IContentBakeable.LoadBaked(PipelineLoadBakedContext context)
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

		//end IContentBakeable
		//-----------------------------

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

