using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace MTS.Engine
{
	public class ImageConversionContext
	{
		public ImageBuffer From;

		public TextureFormat ToFormat;

		/// <summary>
		/// the alpha value to be used when adding an alpha channel
		/// </summary>
		public byte NewAlpha;

		public ImageBuffer Output;
	}

	/// <summary>
	/// Descriptive information about image data: format, dimensions, etc.
	/// </summary>
	public class ImageInfo
	{
		public TextureFormat Format = TextureFormat.RGBA8;

		public int MipLevels;

		public int Width, Height;

		/// <summary>
		/// the palette (if there is one), in canonical format ABGR (that is, the format that blits to RR GG BB AA as little endian)
		/// </summary>
		public uint[] Palette;
	}

	public enum ConversionResult
	{
		Success_Converted,
		Success_AlreadyOK,
		Error_InputFormatHasNoAlpha
	}

	public class ConvertToAlphaProcessableFormatResult
	{
		public ImageBuffer ResultImage;
		public bool Success;
		public ConversionResult ConversionResult;
	}

	public class AlphaTrimResult
	{
		public ImageBuffer ResultImage;
		public int Width, Height;
		public int x, y;
	}

	/// <summary>
	/// An ImageInfo, along with a buffer
	/// </summary>
	public unsafe class ImageBuffer : ImageInfo
	{
		public byte[] Data;

		public static ImageBuffer Create(TextureFormat format, int width, int height)
		{
			if (!IsAlphaProcessableFormat(format)) throw new InvalidOperationException();
			ImageBuffer ret = new ImageBuffer
			{
				Format = format,
				MipLevels = 1,
				Width = width,
				Height = height,
				Data = new byte[width*height*4]
			};
			return ret;
		}

		public ConvertToAlphaProcessableFormatResult ConvertToAlphaProcessableFormat(bool canPalette)
		{
			//palette.. not supported.. yet

			ConvertToAlphaProcessableFormatResult ret = new ConvertToAlphaProcessableFormatResult();
			ret.Success = true;
			ret.ResultImage = this;
			ret.ConversionResult = ConversionResult.Success_AlreadyOK;

			if (IsAlphaProcessableFormat(Format)) return ret;

			//formats without alpha can't be handled here
			//OOPS, THATS EVERY OTHER FORMAT RIGHT NOW

			ret.ConversionResult = ConversionResult.Error_InputFormatHasNoAlpha;
			ret.Success = false;
			return ret;
		}

		public bool IsAlphaProcessableFormat()
		{
			return IsAlphaProcessableFormat(Format);
		}

		public static bool IsAlphaProcessableFormat(TextureFormat format)
		{
			if (format == TextureFormat.RGBA8) return true;
			if (format == TextureFormat.BGRA8) return true; //yeah.. we can handle this, I guess, for whatever it's worth
			return false;
		}

		public AlphaTrimResult AlphaTrim()
		{
			if (!IsAlphaProcessableFormat(Format)) throw new InvalidOperationException();

			int minx = int.MaxValue;
			int maxx = int.MinValue;
			int miny = int.MaxValue;
			int maxy = int.MinValue;

			int sidx = 3;
			for (int y = 0;  y < Height; y++)
				for (int x = 0; x < Width; x++)
				{
					int a = Data[sidx];
					sidx += 4;
					if (a != 0)
					{
						minx = Math.Min(minx, x);
						maxx = Math.Max(maxx, x);
						miny = Math.Min(miny, y);
						maxy = Math.Max(maxy, y);
					}
				}

			//in case it was 100% transparent...
			if (minx == int.MaxValue || maxx == int.MinValue || miny == int.MaxValue || minx == int.MinValue)
			{
				return new AlphaTrimResult
				{
					Width = 0, Height = 0,
					ResultImage = Create(TextureFormat.Canonical, 0, 0),
					x = 0,
					y = 0
				};
			}

			int w = maxx - minx + 1;
			int h = maxy - miny + 1;

			var img = Create(Format, w, h);

			BitmapBuffer bbRet = new BitmapBuffer(w, h);
			sidx = miny * Width + minx;
			int didx = 0;
			fixed (byte* pSrc = Data)
			fixed (byte* pDst = img.Data)
				for (int y = 0; y < h; y++)
				{
					for (int x = 0; x < w; x++)
					{
						((uint*)pDst)[didx++] = ((uint*)pSrc)[sidx++];
					}
					sidx += Width - w;
				}

			return new AlphaTrimResult()
			{
				Width = w,
				Height = h,
				x = minx,
				y = miny,
				ResultImage = img
			};
		}

		public ImageBuffer ExpandDownRight(int w, int h)
		{
			var img = Create(Format, w, h);

			BitmapBuffer bbRet = new BitmapBuffer(w, h);
			int sidx = 0;
			int didx = 0;
			fixed (byte* pSrc = Data)
			fixed (byte* pDst = img.Data)
				for (int y = 0; y < Height; y++)
				{
					for (int x = 0; x < Width; x++)
					{
						((uint*)pDst)[didx++] = ((uint*)pSrc)[sidx++];
					}
					didx += w - Width;
				}

			//bottom will be filled with 0 already

			return img;
		}


		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Width);
			writer.Write(Height);
			writer.Write((int)Format);

			writer.Write(Data.Length);

			//special handling for 0 size
			//note: something earlier should have made sure this can never happen. 

			if (Width == 0 || Height == 0)
			{
				writer.Write(0); //compressed length
				return;
			}

			//apply standard compression, for now
			int outbufferSize = (int)(Data.Length * 1.02f) + 12;
			byte[] outBuffer = new byte[outbufferSize];
			fixed (byte* pOutBuf = outBuffer)
			fixed (byte* pInBuf = Data)
			{
				int LEVEL = -1;
				int comprlen = MTS.Engine.Native.zlib_compress(pOutBuf, pInBuf, outbufferSize, Data.Length, LEVEL);
				writer.Write(comprlen);
				writer.Write(outBuffer, 0, comprlen);
			}

		}
	}

	class ImageAnalyzer
	{

	#if !BRUTED
		/// <summary>
		/// a new argument `TextureFormat desiredFormat` could be used to do some work earlier on, swizzling rgb order most likely.
		/// TODO: this whole enchilada is a good candidate for moving to c++, of course. 
		/// </summary>
		public static unsafe ImageBuffer AnalyzeImageInfo(string path)
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
	#endif

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