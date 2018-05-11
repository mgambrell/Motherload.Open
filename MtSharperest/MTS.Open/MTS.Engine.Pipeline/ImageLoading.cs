using System;
using System.Drawing;
using System.Drawing.Imaging;

using MTS.Engine.ContentUtils;

namespace MTS.Engine.Pipeline
{
	public static unsafe class ImageLoading
	{
		public static ImageBuffer LoadImage(string path)
		{
			var ret = new ImageBuffer();

			using (var bmp = new Bitmap(path))
			{
				int width = ret.Width = bmp.Width;
				int height = ret.Height = bmp.Height;

				if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
				{
					byte[] data = ret.Data = new byte[width * height * 4];
					BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, ret.Width, ret.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
					uint* ptr = (uint*)bmpdata.Scan0.ToPointer();
					int stride = bmpdata.Stride / 4;
					for (int idx = 0, y = 0; y < height; y++)
						for (int x = 0; x < width; x++)
						{
							int src = y * stride + x;
							int srcVal = ((int*)ptr)[src];
							data[idx++] = (byte)(srcVal);
							data[idx++] = (byte)(srcVal >> 8);
							data[idx++] = (byte)(srcVal >> 16);
							data[idx++] = (byte)(srcVal >> 24);
						}
					bmp.UnlockBits(bmpdata);

					ret.Format = TextureFormat.BGRA8;
				}
				else if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
				{
					byte[] data = ret.Data = new byte[width * height * 3];
					BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, ret.Width, ret.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
					byte* ptr = (byte*)bmpdata.Scan0.ToPointer();
					int stride = bmpdata.Stride;
					for (int didx = 0, y = 0; y < height; y++)
					{
						int sidx = (y * stride);
						for (int x = 0; x < width; x++)
						{
							data[didx++] = (byte)(ptr[sidx++]);
							data[didx++] = (byte)(ptr[sidx++]);
							data[didx++] = (byte)(ptr[sidx++]);
						}
					}
					bmp.UnlockBits(bmpdata);

					ret.Format = TextureFormat.BGR8;
				}
				else if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
				{
					byte[] data = ret.Data = new byte[width * height];
					BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, ret.Width, ret.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
					byte* ptr = (byte*)bmpdata.Scan0.ToPointer();
					int stride = bmpdata.Stride;
					for (int didx = 0, y = 0; y < height; y++)
					{
						int sidx = (y * stride);
						for (int x = 0; x < width; x++)
						{
							data[didx++] = (byte)(ptr[sidx++]);
						}
					}
					bmp.UnlockBits(bmpdata);

					var paletteColors = bmp.Palette.Entries;
					ret.Palette = new uint[256];
					for (int i = 0; i < 256; i++)
					{
						uint c = (uint)paletteColors[i].ToArgb();
						byte a = (byte)(c >> 24);
						byte r = (byte)(c >> 16);
						byte g = (byte)(c >> 8);
						byte b = (byte)(c >> 0);
						ret.Palette[i] = (uint)((a << 24) | (b << 16) | (g << 8) | (r << 0));
					}

					ret.Format = TextureFormat.R8;
				}
				else throw new InvalidOperationException("not handled gdi+ bitmap format yet");
			}

			ret.MipLevels = 1;
			return ret;
		}

		public static void Convert(ImageConversionContext context)
		{
			if (context.From.Format == context.ToFormat) return;

			var ret = new ImageBuffer();
			ret.Format = context.ToFormat;

			int width = ret.Width = context.From.Width;
			int height = ret.Height = context.From.Height;

			//reminder: we should intermediate-convert where reasonable to a standard format (i.e. RGBA8) so that we have fewer permutations to code
			//(needs to be done in a better way than this)
			//we can end up with permutations for the basic formats anyway. it will be more rare stuff that's obviously ridiculous to do without the intermediate stage
			if (context.From.Format == TextureFormat.BGRA8 && context.ToFormat == TextureFormat.RGB8)
			{
				var intermediateContext = new ImageConversionContext();
				intermediateContext.From = context.From;
				intermediateContext.ToFormat = TextureFormat.RGBA8;
				Convert(intermediateContext);
				context.From = intermediateContext.Output;
			}

			var src = context.From.Data;

			if (context.From.Format == TextureFormat.R8 && context.ToFormat == TextureFormat.RGBA8)
			{
				context.Output = ret;

				byte[] dst = ret.Data = new byte[width * height * 4];
				uint[] palette = context.From.Palette;

				for (int didx = 0, sidx = 0, y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						uint c = palette[src[sidx++]];
						dst[didx++] = (byte)(c>>0);
						dst[didx++] = (byte)(c>>8);
						dst[didx++] = (byte)(c>>16);
						dst[didx++] = (byte)(c>>24);
					}
				}
			}

			if (context.From.Format == TextureFormat.BGRA8 && context.ToFormat == TextureFormat.RGBA8)
			{
				context.Output = ret;

				byte[] dst = ret.Data = new byte[width * height * 4];

				for (int didx = 0, sidx = 0, y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						byte b = (byte)(src[sidx++]);
						byte g = (byte)(src[sidx++]);
						byte r = (byte)(src[sidx++]);
						byte a = (byte)(src[sidx++]);
						dst[didx++] = r;
						dst[didx++] = g;
						dst[didx++] = b;
						dst[didx++] = a;
					}
				}
			}

			if (context.From.Format == TextureFormat.BGR8 && context.ToFormat == TextureFormat.RGB8)
			{
				context.Output = ret;

				byte[] dst = ret.Data = new byte[width * height * 3];

				for (int didx = 0, sidx = 0, y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						byte b = (byte)(src[sidx++]);
						byte g = (byte)(src[sidx++]);
						byte r = (byte)(src[sidx++]);
						dst[didx++] = r;
						dst[didx++] = g;
						dst[didx++] = b;
					}
				}
			}

			if (context.From.Format == TextureFormat.BGR8 && context.ToFormat == TextureFormat.RGBA8)
			{
				context.Output = ret;

				byte a = context.NewAlpha;
				byte[] dst = ret.Data = new byte[width * height * 4];

				for (int didx = 0, sidx = 0, y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						byte b = (byte)(src[sidx++]);
						byte g = (byte)(src[sidx++]);
						byte r = (byte)(src[sidx++]);
						dst[didx++] = r;
						dst[didx++] = g;
						dst[didx++] = b;
						dst[didx++] = a;
					}
				}
			}

			if (context.From.Format == TextureFormat.RGBA8 && context.ToFormat == TextureFormat.RGB8)
			{
				context.Output = ret;

				byte[] dst = ret.Data = new byte[width * height * 3];

				for (int didx = 0, sidx = 0, y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						byte r = (byte)(src[sidx++]);
						byte g = (byte)(src[sidx++]);
						byte b = (byte)(src[sidx++]);
						sidx++;
						dst[didx++] = r;
						dst[didx++] = g;
						dst[didx++] = b;
					}
				}
			}

		} //Convert()

	}
}