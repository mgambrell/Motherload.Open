using System;
using System.Linq;
using System.IO;
using MTS.Engine;

using MTS.Engine.ContentUtils;

namespace MTS.Engine.Pipeline.Pipelines
{
	public class ArtPipeline : IContentPipeline
	{
		public void Prepare(PipelineBakeContext context)
		{
			var path = context.RawContentDiskPath;
			path += ".png";
			context.Depend("source", path);
		}

		//TODO - this has really exposed the need to have this separated from instance methods on an art content instance
		//i mean, it really has nothing at all to do with the content
		//the baker should be a totally separate interface, so we're not encouraged to put it here (however, it is OK if we do)
		//unfortuately, right now only a content instance knows which object type can be used to bake it.
		//we would need some kind of reflection index of bakers for content types. can we put it in an attribute so it doesnt need to be manually registered?
		public unsafe bool Bake(PipelineBakeContext context)
		{
			Console.WriteLine("Art.Bake: " + context.ContentPath);

			float umin = 0;
			float vmin = 0;
			float umax = 1;
			float vmax = 1;

			var path = context.Depends["source"];

			var imageBuffer = ImageLoading.LoadImage(path);

			//TODO: we can only handle certain input formats here (no compressed formats)
			//but we can output any format (after we crop it)

			int width = imageBuffer.Width;
			int height = imageBuffer.Height;
			int physwidth = width;
			int physheight = height;
			int ox = 0, oy = 0;

			//NOTE: EVERYTHING BELOW IS EXPERIMENTAL. ITS A TOTAL MESS

			//TODO - apply art-specific operations (cropping, etc.)
			//TODO - make controllable
			//TODO - handle errors
			var conversionResult = imageBuffer.ConvertToAlphaProcessableFormat(false);

			bool doTrim = true;
			if (conversionResult.ConversionResult == ConversionResult.Error_InputFormatHasNoAlpha) doTrim = false;

			if (doTrim)
			{
				//accept the converted image
				imageBuffer = conversionResult.ResultImage;

				var alphaTrimResult = imageBuffer.AlphaTrim();
				imageBuffer = alphaTrimResult.ResultImage;

				ox = alphaTrimResult.x;
				oy = alphaTrimResult.y;
				physwidth = alphaTrimResult.Width;
				physheight = alphaTrimResult.Height;
			}

			bool doPadPow2 = true;
			if (!imageBuffer.IsAlphaProcessableFormat()) doPadPow2 = false;
			if (doPadPow2)
			{
				int widthRound = PipelineMath.TextureUptoPow2(physwidth);
				int heightRound = PipelineMath.TextureUptoPow2(physheight);
				if (widthRound != physwidth || heightRound != physheight)
				{
					imageBuffer = imageBuffer.ExpandDownRight(widthRound, heightRound);
				}
			}

			var fmtAttribute = context.Attributes.FirstOrDefault(a => a is TextureFormatAttribute);
			if (fmtAttribute != null)
			{
				var toFormat = ((TextureFormatAttribute)fmtAttribute).Format;

				ImageConversionContext imageContext = new ImageConversionContext();
				imageContext.From = imageBuffer;
				imageContext.NewAlpha = 0xFF;
				imageContext.ToFormat = toFormat;
				ImageLoading.Convert(imageContext);
				imageBuffer = imageContext.Output;
			}

			umax = (ox + physwidth) / imageBuffer.Width;
			vmax = (oy + physheight) / imageBuffer.Height;

			//the texture goes first...
			var textureBakingContext = new PipelineConnector_TextureBaking()
			{
				Image = imageBuffer,
				Writer = context.BakedWriter
			};
			context.PipelineConnector.BakeTexture(textureBakingContext);

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
	}

}