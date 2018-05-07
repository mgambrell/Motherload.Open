//NOTE: this is a placeholder for a much more complex type which knows all about different texture formats
//knows how to analyze, generates mipmaps, crop, swizzle/encode for various platforms, etc.
//i've stripped it down from 1000s of lines in my old engine. We'll have to build it back up as needed
//(It can't support formats bigger than 32bpp argb though. anyone doing exotic stuff will need some alternate DDS handler probably)
//TODO: it would be cool if it could loads PSDs and PDNs too....

//IDEA: could be useful to expose this to the game logic too.. but it would be a lot of cruft. so maybe just, something like it.

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using MTS.Engine;

/// <summary>
/// a software-based bitmap, way easier (and faster) to use than .net's built-in bitmap.
/// Only supports a fixed rgba format, but knows how to export to a NativeBitmapBuffer (TODO-reimplement/think this)
/// </summary>
public unsafe class BitmapBuffer
{
	public int Width, Height;
	public int[] Pixels;

	public BitmapBuffer(int width, int height)
	{
		Width = width;
		Height = height;
		Pixels = new int[Width * Height];
	}

#if !BRUTED
	//TODO: remove this. it isnt clear what parameters should be used for loading
	//("parameters"? this could be complex.)

	public unsafe BitmapBuffer(string path)
	{
		using (var bmp = new Bitmap(path))
			LoadFrom(bmp);
	}

	//TODO: remove this. it isnt clear what parameters should be used
	void LoadFrom(Bitmap bmp)
	{
		//dump the supplied bitmap into our pixels array
		int width = bmp.Width;
		int height = bmp.Height;
		BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		int* ptr = (int*)bmpdata.Scan0.ToInt32();
		int stride = bmpdata.Stride / 4;
		LoadFrom(width, stride, height, (byte*)ptr);
		bmp.UnlockBits(bmpdata);
	}
#endif

	public void LoadFrom(int width, int stride, int height, byte* data)
	{
		Width = width;
		Height = height;
		Pixels = new int[width * height];
		fixed (int* pPtr = &Pixels[0])
		{
			for (int idx = 0, y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
				{
					int src = y * stride + x;
					int srcVal = ((int*)data)[src];
					
					//make transparent pixels turn into black to avoid filtering issues and other annoying issues with stray junk in transparent pixels
					//TODO - this should be configurable, of course
					if ((srcVal & 0xFF000000) == 0) srcVal = 0;

					pPtr[idx++] = srcVal;
				}
		}
	}


	public void Serialize(BinaryWriter writer)
	{
		writer.Write(Width);
		writer.Write(Height);

		int rawSize = Width * Height * 4;
		writer.Write(rawSize);

		//special handling for 0 size
		//note: something earlier should have made sure this can never happen. 

		if (Width == 0 || Height == 0)
		{
			writer.Write(0); //compressed length
			return;
		}

		MemoryStream msRawBytes = new MemoryStream(rawSize);
		var bw = new BinaryWriter(msRawBytes);
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				bw.Write(Pixels[y * Height + x]);
			}
		}

		//apply standard compression, for now
		int outbufferSize = (int)(rawSize * 1.02f) + 12;
		byte[] outBuffer = new byte[outbufferSize];
		fixed (byte* pOutBuf = outBuffer)
		fixed (byte* pInBuf = msRawBytes.GetBuffer())
		{
			int LEVEL = -1;
			int comprlen = MTS.Engine.Native.zlib_compress(pOutBuf, pInBuf, outbufferSize, rawSize, LEVEL);
			writer.Write(comprlen);
			writer.Write(outBuffer, 0, comprlen);
		}

	}


}