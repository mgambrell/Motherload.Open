using System;
using System.Linq;
using System.IO;
using MTS.Engine;

namespace MTS.Engine.Pipelines
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
	}

}