using System;
using System.Runtime.InteropServices;


namespace MTS.Engine.SDL
{
	public unsafe class DefaultRuntimeConnector : RuntimeConnectorBase
	{
		public override IntPtr LoadTexture(RuntimeConncetor_TextureContext context)
		{
			SDL.MTS_SDL_TextureDescr texdescr = new SDL.MTS_SDL_TextureDescr();

			//we should have made sure this is legal when baking
			Functions.TryGetTextureFormat(context.ImageBuffer.Format, out texdescr.format);

			texdescr.width = context.ImageBuffer.Width;
			texdescr.height = context.ImageBuffer.Height;
			texdescr.levels = 1;

			//var rawData = context.Reader.ReadBytes(context.ImageInfo);
			var rawData = context.ImageBuffer.Data;

			//SDL.MTS_SDL_Texture tex;
			//fixed (byte* pTexbuf = rawData)
			//{
			//	texdescr.pixels = new IntPtr(pTexbuf);
			//	tex = SDL.Functions.mts_sdl_Texture_Create(ref texdescr);
			//}
			//return tex.ptr;

			//quick workaround for brute bug
			fixed (byte* pTexbuf = rawData)
			{
				texdescr.pixels = new IntPtr(pTexbuf);
				var tex = SDL.Functions.mts_sdl_Texture_Create(ref texdescr);
				return tex.ptr;
			}
		}

		public override void UnloadShader(ContentConnectorContext_ShaderProgram context)
		{
			SDL.Functions.mts_sdl_Program_Destroy((MTS_SDL_Program)context.Handle);
		}

		public override void LoadShader(ContentConnectorContext_ShaderProgram context)
		{
			var br = context.Reader;

			var vs_src = br.ReadString();
			var fs_src = br.ReadString();

			var vs = SDL.Functions.mts_sdl_Shader_Create(SDL.MTS_SDL_ShaderType.Vertex, vs_src);
			var fs = SDL.Functions.mts_sdl_Shader_Create(SDL.MTS_SDL_ShaderType.Fragment, fs_src);

			context.Handle = SDL.Functions.mts_sdl_Program_Create(vs,fs);

			SDL.Functions.mts_sdl_Shader_Destroy(vs);
			SDL.Functions.mts_sdl_Shader_Destroy(fs);
		}
	}

}
