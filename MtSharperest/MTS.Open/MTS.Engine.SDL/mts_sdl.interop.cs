#pragma warning disable 0169 //field is never used

using System;
using System.Runtime.InteropServices;

namespace MTS.Engine.Platforms.SDL
{

	public struct MTS_SDL_Shader { public IntPtr ptr; }
	public struct MTS_SDL_Program { public IntPtr ptr; }
	public struct MTS_SDL_VertexLayout { public IntPtr ptr; }
	public struct MTS_SDL_DynamicVertexBuffer { public IntPtr ptr; }
	public struct MTS_SDL_StaticVertexBuffer { public IntPtr ptr; }
	public struct MTS_SDL_RenderTarget { public IntPtr ptr; public static MTS_SDL_RenderTarget Null { get { return new MTS_SDL_RenderTarget() { ptr = IntPtr.Zero }; } } }
	public struct MTS_SDL_BlendState { public IntPtr ptr; }
	public struct MTS_SDL_SamplerState { public IntPtr ptr; }
	public struct MTS_SDL_Texture { public IntPtr ptr; }

	public enum MTS_SDL_ShaderType
	{
		Vertex,
		Fragment
	};

	public enum MTS_SDL_PrimitiveType
	{
		Triangles,
		TriangleStrip,
		Trianglefan
	};

	public enum MTS_SDL_BlendFunc
	{
		Zero, One,
		SrcColor, OneMinusSrcColor,
		DstColor, OneMinusDstColor,
		SrcAlpha, OneMinusSrcAlpha,
		DstAlpha, OneMinusDstAlpha,
		ConstantColor, OneMinusConstantColor,
		ConstantAlpha, OneMinusConstantAlpha,
		AlphaSaturate,
		Src1Color, OneMinusSrc1Color,
		Src1Alpha, OneMinusSrc1Alpha
	};

	public enum MTS_SDL_BlendEquation
	{
		Add, Subtract, ReverseSubtract,
		Min, Max
	};

	public enum MTS_SDL_ClearMask
	{
		Color = 1,
		DepthStencil = 2
	};

	public enum MTS_SDL_BlendWriteMask
	{
		Red = 1,
		Green = 2,
		Blue = 4,
		Alpha = 8,
		R = Red,
		G = Green,
		B = Blue,
		A = Alpha,
		RGBA = (R | G | B | A),
		RGB = (R | G | B),
	};


	//these are defined abstractly in hopes we can reuse the layout structure on other platforms
	public enum MTS_SDL_AttributeFormat
	{
		Float, Half,
		Byte, UnsignedByte,
		Short, UnsignedShort,
		Int, UnsignedInt
	};

	public enum MTS_SDL_TextureFormat
	{
		None,
		RGBA8,
		BGRA8,
		RGB8,
		BGR8,
		R8
	};

	//Hopefully we can make these semi-standardized across all platforms
	public enum MTS_SDL_SamplerFilter
	{
		Nearest,
		Linear,
		NearestMipmapNearest, LinearMipmapNearest,
		NearestMipmapLinear, LinearMipmapLinear
	};

	public enum MTS_SDL_SamplerWrap
	{
		ClampToEdge,
		MirroredRepeat,
		Repeat
	};
	public struct MTS_SDL_VertexLayoutDescr_Attribute
	{
		public int index;
		public int offset;
		public MTS_SDL_AttributeFormat format;
		public int size;
	};

	//top level description of a vertex layout
	public unsafe struct MTS_SDL_VertexLayoutDescr
	{
		public int stride;
		public int nAttributes;
		public MTS_SDL_VertexLayoutDescr_Attribute* attributes;
	};

	public struct MTS_SDL_BlendStateDescr
	{
		public bool enabled;
		public MTS_SDL_BlendFunc srcColor, srcAlpha;
		public MTS_SDL_BlendFunc dstColor, dstAlpha;
		public MTS_SDL_BlendEquation eqColor, eqAlpha;
		public MTS_SDL_BlendWriteMask writeMask;
	};


	public struct MTS_SDL_SamplerStateDescr
	{
		public MTS_SDL_SamplerFilter minFilter, magFilter;
		public float minLod, maxLod;
		public MTS_SDL_SamplerWrap wrapS, wrapT; //GL_CLAMP_TO_EDGE, GL_MIRRORED_REPEAT, or GL_REPEAT
	};

	public struct MTS_SDL_TextureDescr
	{
		public int width, height;
		public int levels; //not really supported yet, save for a rainy day
		public MTS_SDL_TextureFormat format;
		public IntPtr pixels;
	};

	public struct MTS_SDL_ClearDescr
	{
		public MTS_SDL_ClearMask mask;
		public float r, g, b, a;
		public float depth;
		public int stencil;
	};

	public static class Functions
	{
		//public static bool TryGetTextureFormat(mts.Engine.TextureFormat coreFormat, out MTS_SDL_TextureFormat sdlFormat)
		//{
		//	//maybe I can make a kind of uniform standard later and have everything line up (with null gaps in some kind of list for unsupported formats)
		//	//but for now...
		//	if (coreFormat == TextureFormat.RGBA8) { sdlFormat = MTS_SDL_TextureFormat.RGBA8; return true; }
		//	if (coreFormat == TextureFormat.BGRA8) { sdlFormat = MTS_SDL_TextureFormat.BGRA8; return true; }
		//	if (coreFormat == TextureFormat.RGB8) { sdlFormat = MTS_SDL_TextureFormat.RGB8; return true; }
		//	if (coreFormat == TextureFormat.BGR8) { sdlFormat = MTS_SDL_TextureFormat.BGR8; return true; }
		//	if (coreFormat == TextureFormat.R8) { sdlFormat = MTS_SDL_TextureFormat.R8; return true; }

		//	sdlFormat = MTS_SDL_TextureFormat.None;
		//	return false;
		//}

		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_InitialRenderState();

		//Creates the given type of shader. arguments style matches opengl.
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_Shader mts_sdl_Shader_CreateMulti(MTS_SDL_ShaderType type, int count, string[] codes, int[] lengths);

		//Creates the given type of shader. For convenience, the argument is a simple C-string
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_Shader mts_sdl_Shader_Create(MTS_SDL_ShaderType type, string code);

				//Creates the given type of shader. For convenience, the argument is a simple C-string
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Shader_Destroy(MTS_SDL_Shader shader);

		//Creates a program from the given shaders
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_Program mts_sdl_Program_Create(MTS_SDL_Shader vertexShader, MTS_SDL_Shader fragmentShader);

		//Binds this program as current
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Program_Bind(IntPtr pgm);

		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Program_Destroy(MTS_SDL_Program pgm);

		//Creates a vertex layout from the provided description
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_VertexLayout mts_sdl_VertexLayout_Create(ref MTS_SDL_VertexLayoutDescr layout);

		//destroys a vertex layout
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_VertexLayout_Release(MTS_SDL_VertexLayout layout);

		//signal to begin writing to the constant buffer. you should do this before setting values in it
		//you will need to do this each frame or else it will run out eventually
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_ConstantBuffer_BeginFrame(int bufferIndex);

		//Sets the given data into a constant buffer
		//if this mismatches the actual opengl Uniform Block size, you will be sorry.. you may also get an error
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_ConstantBuffer_Set(int bufferIndex, IntPtr data, int size);

		//creates a dynamic vertex buffer
		//it's assumed this will contain a specific type of vertex; addressing will be done with those units
		//therefore the MTS_SDL_VertexLayout must be provided too
		//really, it's just a MyVertexFormat[], so don't expect anything too powerful
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_DynamicVertexBuffer mts_sdl_DynamicVertexBuffer_Create(MTS_SDL_VertexLayout vertexLayout, int nElements);

		//signal to begin writing to the dynamic vertex buffer
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_DynamicVertexBuffer_Begin(MTS_SDL_DynamicVertexBuffer dvb);

		//sets the given data into the dynamic vertex buffer
		//returns the initial vertex index written to (for use as a starting when drawing)
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int mts_sdl_DynamicVertexBuffer_SetElements(MTS_SDL_DynamicVertexBuffer dvb, IntPtr data, int nElements);

		//destroys the dynamic vertex buffer
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_DynamicVertexBuffer_Destroy(MTS_SDL_DynamicVertexBuffer dvb);

		//creates a static vertex buffer
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_StaticVertexBuffer mts_sdl_StaticVertexBuffer_Create(MTS_SDL_VertexLayout vertexLayout, int nElements, IntPtr elements);

		//destroys the static vertex buffer
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_StaticVertexBuffer_Destroy(MTS_SDL_StaticVertexBuffer svb);

		//creates a texture
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_Texture mts_sdl_Texture_Create(ref MTS_SDL_TextureDescr descr);

		//destroys a texture
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Texture_Destroy(MTS_SDL_Texture tex);

		//creates a render target with the given dimensions
		//this is probably oversimplified
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_RenderTarget mts_sdl_RenderTarget_Create(int width, int height);

		//binds this render target as current
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_RenderTarget_Bind(MTS_SDL_RenderTarget rt);

		//destroys the given render target
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_RenderTarget_Destroy(MTS_SDL_RenderTarget rt);

		//creates a blend state object
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_BlendState mts_sdl_BlendState_Create(ref MTS_SDL_BlendStateDescr descr);

		//destroys the given blend state object
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_BlendState_Destroy(MTS_SDL_BlendState blendState);

		//creates a sampler state object
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern MTS_SDL_SamplerState mts_sdl_SamplerState_Create(ref MTS_SDL_SamplerStateDescr descr);

		//destroys the given sampler state object
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_SamplerState_Destroy(MTS_SDL_SamplerState samplerState);

		//draws (equivalent of glDrawArrays)
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Draw(MTS_SDL_PrimitiveType primitiveType, int startIndex, int nVertices);

		//binds the blend state object (target is usually 0)
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_BlendState(int target, MTS_SDL_BlendState blendState);

		//sets the constant color on the blending unit 
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_BlendState_Color(float[] rgba_array);

		//binds the dynamic vertex buffer. binds the entire thing (there is no provision for binding a range; control the range through your draw calls instead)
		//this binds its associated vertex layout as well
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_DynamicVertexBuffer(MTS_SDL_DynamicVertexBuffer dvb);

		//binds the dynamic vertex buffer. binds the entire thing (there is no provision for binding a range; control the range through your draw calls instead)
		//this binds its associated vertex layout as well
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_StaticVertexBuffer(MTS_SDL_StaticVertexBuffer svb);

		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_SamplerState(int target, MTS_SDL_SamplerState sampler);

		//Binds this vertex layout as current
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_VertexLayout(MTS_SDL_VertexLayout layout);

		//Binds the backbuffer as current render target
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_Backbuffer();

		//Binds this texture to the given texture unit
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_Texture(int target, MTS_SDL_Texture tex);

		//Binds this render target's texture to the given texture unit
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Bind_TextureRT(int target, MTS_SDL_RenderTarget rt);

		//Sets device's current viewport
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_SetViewport(int x, int y, int width, int height);

		//Sets device's current depth range
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_SetDepthRange(float hither, float yon);

		//begins drawing for a frame.
		//chiefly, makes sure once-per-frame processes run, whatever they are
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_BeginFrame();

		//clears the framebuffer
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_Clear(ref MTS_SDL_ClearDescr descr);

		//ends frame and swaps buffers
		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Device_EndFrame();

		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Init();

		[DllImport("MTS.Engine.SDL.Native.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mts_sdl_Exit();
	}
}