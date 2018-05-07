using System;
using System.IO;

namespace MTS.Engine
{
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

	public class RectItem
	{
		public RectItem(int width, int height, object item)
		{
			Width = width;
			Height = height;
			Item = item;
		}
		public int X, Y;
		public int Width, Height;
		public int TexIndex;
		public object Item;
	}

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

}