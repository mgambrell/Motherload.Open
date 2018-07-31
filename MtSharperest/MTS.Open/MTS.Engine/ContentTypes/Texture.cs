using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using MTS.Engine.ContentUtils;

namespace MTS.Engine
{
public enum TextureFormat
	{
		/// <summary>
		/// in memory as RR GG BB AA
		/// </summary>
		RGBA8,

		/// <summary>
		/// in memory as BB GG RR AA
		/// </summary>
		BGRA8,

		/// <summary>
		/// in memory as RR GG BB
		/// </summary>
		RGB8,

		/// <summary>
		/// in memory as BB GG AA
		/// </summary>
		BGR8,

		/// <summary>
		/// in memory as RR
		/// /// </summary>
		R8,

		/// <summary>
		/// In memory as II.. should appear to shaders as would format `R8` but signifies extra handling for the palette
		/// </summary>
		I8,

		/// <summary>
		/// The texture format you should use 99% of the time
		/// </summary>
		Color = RGBA8,
	}
}

namespace MTS.Engine.ContentTypes
{
	/// <summary>
	/// The basic texture content.
	/// Textures can be NP2 in any platform we support, but there may be other rules about their dimensions depending on hardware platform.
	/// If you want to automatically pad it, use Art--that's so information about the original content size is preserved.
	/// Should support mipmaps, I guess, but in the most unobtrusive way possible.
	/// </summary>
	public unsafe class Texture : ContentBase, IBakedLoader, PrivateInterfaces.ITextureLoaders
	{
		IntPtr mHandle;
		bool mOwnsHandle;

		ImageInfo info;

		public int Width { get { return info.Width; } }
		public int Height { get { return info.Height; } }

		public IntPtr Handle { get { return mHandle; } }

		protected override void ContentUnload()
		{
			Console.WriteLine("Texture.MyUnload: " + "???");
			if(mOwnsHandle)
				Manager.RuntimeConnector.DestroyTexture(mHandle);
			mHandle = IntPtr.Zero;
			mOwnsHandle = false;
		}

		bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			Console.WriteLine("Texture.LoadBaked: " + context.ContentPath);

			var br = context.BakedReader;

			info = new ImageInfo();
			info.Width = br.ReadInt32();
			info.Height = br.ReadInt32();
			TextureFormat format = (TextureFormat)br.ReadInt32();

			int rawSize = br.ReadInt32();
			int comprLen = br.ReadInt32();

			byte[] temp = br.ReadBytes(comprLen);
			byte[] rawbuf = new byte[rawSize];

			//apply standard uncompression, for now
			fixed (byte* pOutBuf = rawbuf)
			fixed (byte* pInBuf = temp)
			{
				MTS.Engine.Native.zlib_uncompress(pOutBuf, pInBuf, rawSize, comprLen);
			}

			var ms = new MemoryStream(rawbuf);
			var brRaw = new BinaryReader(ms);
			var resLoaderContext = new RuntimeConnector_TextureContext();
			resLoaderContext.ImageBuffer = new ImageBuffer();
			//resLoaderContext.Reader = brRaw; //meaningless now, should be restored later
			resLoaderContext.ImageBuffer.Data = rawbuf;
			resLoaderContext.ImageBuffer.Width = info.Width;
			resLoaderContext.ImageBuffer.Height = info.Height;
			resLoaderContext.ImageBuffer.Format = format;
			mHandle = this.Manager.RuntimeConnector.LoadTexture(resLoaderContext);
			mOwnsHandle = true;
			return true;
		}

		/// <summary>
		/// You can create a Texture to wrap your own texture handle (i.e. a rendertarget)
		/// This texture will not own the handle; when it's unloaded, the handle will remain untouched.
		/// </summary>
		public static Texture CreateFromTextureHandle(ContentManager manager, ImageInfo info, IntPtr handle)
		{
			var textureLoader = new ContentLoaderProxy(
				(ContentLoadContext context) =>
				{
					var texture = context.Content as Texture;
					texture.mHandle = handle;
					texture.info = info;
					return true;
				}
			);
			
			return manager.CreateWith<Texture>(textureLoader);
		}

		void PrivateInterfaces.ITextureLoaders.LoadSolidColor(int width, int height, uint color)
		{
			info = new ImageInfo();
			info.Width = width;
			info.Height = height;

			int nPixels = width * height;

			//well, this was a nice experiment, but it's hard to really do this generally
			//uhhh it's possible I could only support a certain texture format, perhaps, that I know how to synthesize the input for (i.e. RGBA8)

			MemoryStream ms = new MemoryStream(width * height * 4);
			BinaryWriter bw = new BinaryWriter(ms);
			for (int i = 0; i < nPixels; i++)
				bw.Write(color);
			bw.Flush();
			ms.Position = 0;

			var resLoaderContext = new RuntimeConnector_TextureContext();
			resLoaderContext.ImageBuffer = new ImageBuffer();
			//resLoaderContext.Reader = brRaw; //meaningless now, should be restored later
			resLoaderContext.ImageBuffer.Data = ms.GetBuffer();
			resLoaderContext.ImageBuffer.Width = info.Width;
			resLoaderContext.ImageBuffer.Height = info.Height;
			resLoaderContext.ImageBuffer.Format = TextureFormat.RGBA8;
			mHandle = this.Manager.RuntimeConnector.LoadTexture(resLoaderContext);
		}

	}
}

