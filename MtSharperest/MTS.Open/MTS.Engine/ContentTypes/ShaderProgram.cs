//using System;
//using System.IO;

//using SDL = mts.Engine.Platforms.SDL;

//namespace MTS.Engine
//{
//	/// <summary>
//	/// A compiled shader program. It will take care of compiling itself when the source file changes.
//	/// This would need to be substantially customized for each target platform (this file should be placed in the platform specific directories and only one selected)
//	/// </summary>
//	public unsafe class ShaderProgram : ContentBase, IContentBakeable
//	{
//		SDL.MTS_SDL_Program pgm;

//		public IntPtr Handle { get { return pgm.ptr; } }

//		protected override void ContentUnload()
//		{
//			SDL.Functions.mts_sdl_Program_Destroy(pgm);
//		}

//		unsafe void IContentBakeable.Prepare(PipelineBakeContext context)
//		{
//			var vsna = Attributes.GetAttribute<VertexShaderNameAttribute>();
//			var fsna = Attributes.GetAttribute<FragmentShaderNameAttribute>();

//			if (vsna == null || fsna == null)
//			{
//				context.Failed = true;
//				return;
//			}

//			var dir = System.IO.Path.GetDirectoryName(context.RawContentDiskPath);

//			context.Depend("vs", dir + "/" + vsna.Name + ".glsl");
//			context.Depend("fs", dir + "/" + fsna.Name + ".glsl");
//		}

//		unsafe bool IContentBakeable.Bake(PipelineBakeContext context)
//		{
//			var vs_src = File.ReadAllText(context.Depends["vs"]);
//			var fs_src = File.ReadAllText(context.Depends["fs"]);

//			//reminder: could easily layer encryption into the pipeline output, to munge your shader sources
//			context.BakedWriter.Write(vs_src);
//			context.BakedWriter.Write(fs_src);

//			return true;
//		}

//		bool IContentBakeable.LoadBaked(PipelineLoadBakedContext context)
//		{
//			var br = context.BakedReader;

//			var vs_src = br.ReadString();
//			var fs_src = br.ReadString();

//			var vs = SDL.Functions.mts_sdl_Shader_Create(SDL.MTS_SDL_ShaderType.Vertex, vs_src);
//			var fs = SDL.Functions.mts_sdl_Shader_Create(SDL.MTS_SDL_ShaderType.Fragment, fs_src);

//			pgm = SDL.Functions.mts_sdl_Program_Create(vs,fs);

//			SDL.Functions.mts_sdl_Shader_Destroy(vs);
//			SDL.Functions.mts_sdl_Shader_Destroy(fs);

//			return true;
//		}


//	}
//}

