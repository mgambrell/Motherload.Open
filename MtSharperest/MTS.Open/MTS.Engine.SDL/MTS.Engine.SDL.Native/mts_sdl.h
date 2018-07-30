#pragma once

//putting all the MTS apis for here now, and API reference

#include <MTS/MTS_Native.h>

extern "C"
{

//the type of a shader
enum class MTS_SDL_ShaderType
{
	Vertex,
	Fragment
};

enum class MTS_SDL_PrimitiveType
{
	Triangles,
	TriangleStrip,
	Trianglefan
};

enum class MTS_SDL_BlendFunc
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

enum class MTS_SDL_BlendEquation
{
	Add, Subtract, ReverseSubtract,
	Min, Max
};

enum class MTS_SDL_ClearMask
{
	Color = 1,
	DepthStencil = 2
};


enum class MTS_SDL_BlendWriteMask
{
	Red = 1,
	Green = 2,
	Blue = 4,
	Alpha = 8,
	R = Red,
	G = Green,
	B = Blue, 
	A = Alpha,
	RGBA = (R|G|B|A),
	RGB = (R|G|B),
};


//these are defined abstractly in hopes we can reuse the layout structure on other platforms
enum class MTS_SDL_AttributeFormat
{
	Float, Half,
	Byte, UnsignedByte,
	Short, UnsignedShort,
	Int, UnsignedInt
};

enum class MTS_SDL_TextureFormat
{
	None,
	RGBA8,
	BGRA8,
	RGB8,
	BGR8,
	R8
};

//Hopefully we can make these semi-standardized across all platforms
enum class MTS_SDL_SamplerFilter
{
	Nearest,
	Linear,
	NearestMipmapNearest, LinearMipmapNearest,
	NearestMipmapLinear, LinearMipmapLinear
};

enum class MTS_SDL_SamplerWrap
{
	ClampToEdge, 
	MirroredRepeat,
	Repeat
};

//TODO: havent completely worked out how this works with multiple streams and stuff yet

//vertex layout information for one attribute
//these fields are laid out for convienence defining, not compactness
struct MTS_SDL_VertexLayoutDescr_Attribute
{
	int index;
	int offset;
	MTS_SDL_AttributeFormat format;
	int size;
};

//top level description of a vertex layout
struct MTS_SDL_VertexLayoutDescr
{
	int stride;
	int nAttributes;
	const MTS_SDL_VertexLayoutDescr_Attribute* attributes;
};

struct MTS_SDL_BlendStateDescr
{
	bool enabled;
	MTS_SDL_BlendFunc srcColor, srcAlpha;
	MTS_SDL_BlendFunc dstColor, dstAlpha;
	MTS_SDL_BlendEquation eqColor, eqAlpha;
	MTS_SDL_BlendWriteMask writeMask;
};

struct MTS_SDL_SamplerStateDescr
{
	MTS_SDL_SamplerFilter minFilter, magFilter;
	float minLod, maxLod;
	MTS_SDL_SamplerWrap wrapS, wrapT; //GL_CLAMP_TO_EDGE, GL_MIRRORED_REPEAT, or GL_REPEAT
};

struct MTS_SDL_TextureDescr
{
	int width, height;
	int levels; //not really supported yet, save for a rainy day
	MTS_SDL_TextureFormat format;
	void* pixels;
};

struct MTS_SDL_ClearDescr
{
	MTS_SDL_ClearMask mask;
	float r,g,b,a;
	float depth;
	int stencil;
};

//forward declarations for opaque handles; ignore me
struct MTS_SDL_ShaderStruct;
struct MTS_SDL_DepthStencilStateStruct;
struct MTS_SDL_ProgramStruct;
struct MTS_SDL_VertexLayoutStruct;
struct MTS_SDL_DynamicVertexBufferStruct;
struct MTS_SDL_StaticVertexBufferStruct;
struct MTS_SDL_RenderTargetStruct;
struct MTS_SDL_BlendStateStruct;
struct MTS_SDL_SamplerStateStruct;
struct MTS_SDL_TextureStruct;
struct MTS_SDL_PolygonStateStruct;

//opaque handles for all the basic resource types
typedef MTS_SDL_ShaderStruct* MTS_SDL_Shader;
typedef MTS_SDL_DepthStencilStateStruct* MTS_SDL_DepthStencilState;
typedef MTS_SDL_ProgramStruct* MTS_SDL_Program;
typedef MTS_SDL_VertexLayoutStruct* MTS_SDL_VertexLayout;
typedef MTS_SDL_DynamicVertexBufferStruct* MTS_SDL_DynamicVertexBuffer;
typedef MTS_SDL_StaticVertexBufferStruct* MTS_SDL_StaticVertexBuffer;
typedef MTS_SDL_RenderTargetStruct* MTS_SDL_RenderTarget;
typedef MTS_SDL_BlendStateStruct* MTS_SDL_BlendState;
typedef MTS_SDL_SamplerStateStruct* MTS_SDL_SamplerState;
typedef MTS_SDL_TextureStruct* MTS_SDL_Texture;
typedef MTS_SDL_PolygonStateStruct* MTS_SDL_PolygonState;

enum class MTS_SDL_Comparison
{
	Invalid = 0,

	//Comparison always fails.
	Never = 1,

	//Comparison passes if the test value is less than the reference value.
	Less = 2,

	//Comparison passes if the test value is equal to the reference value.
	Equal = 3,

	//Comparison passes if the test value is less than or equal to the reference value.
	LEqual = 4,

	//Comparison passes if the test value is greater than the reference value.
	Greater = 5,

	//Comparison passes if the test value is not equal to reference value.
	NotEqual = 6,

	//Comparison passes if the test value is greater than or equal to the reference value.
	GEqual = 7,

	//Comparison always passes.
	Always = 8
};

//---------------------------------------------------------------------------------
//MTS_SDL_DepthStencilState
//---------------------------------------------------------------------------------

enum class MTS_SDL_StencilOp
{
	Invalid = 0,

	//Keeps the current value.
	Keep = 1,

	//Sets the stencil buffer value to 0.
	Zero = 2,

	//Sets the stencil buffer value to ref, as specified by glStencilFunc.
	Replace = 3,

	//Increments the current stencil buffer value. Clamps to the maximum representable unsigned value.
	Increment = 4,

	//Decrements the current stencil buffer value. Clamps to 0.
	Decrement = 5,

	//Bitwise inverts the current stencil buffer value.
	Invert = 6,

	//Increments the current stencil buffer value. Wraps stencil buffer value to zero when incrementing the maximum representable unsigned value.
	IncrementWrap = 7,

	//Decrements the current stencil buffer value. Wraps stencil buffer value to the maximum representable unsigned value when decrementing a stencil buffer value of zero.
	DecrementWrap = 8,
};

//specifications for depth stencil state.
struct MTS_SDL_DepthStencilStateDescr
{
	MTS_SDL_Comparison StencilFuncFront;
	MTS_SDL_StencilOp StencilFailFront, StencilDepthFailFront, StencilDepthPassFront;

	MTS_SDL_Comparison StencilFuncBack;
	MTS_SDL_StencilOp StencilFailBack, StencilDepthFailBack, StencilDepthPassBack;

	MTS_SDL_Comparison DepthFunc;

	bool DepthTestEnable, DepthWriteEnable;
	bool StencilTestEnable;

	void ToDefaults()
	{
		DepthFunc = MTS_SDL_Comparison::Never;
		DepthTestEnable = false;
		DepthWriteEnable = false;
		StencilTestEnable = false;
		StencilFuncFront = MTS_SDL_Comparison::Always;
		StencilFuncBack = MTS_SDL_Comparison::Always;
		StencilFailFront = StencilDepthFailFront = StencilDepthPassFront = MTS_SDL_StencilOp::Keep;
		StencilFailBack = StencilDepthFailBack = StencilDepthPassBack = MTS_SDL_StencilOp::Keep;
	}
};

//Creates the given type of shader. For convenience, the argument is a simple C-string
EXPORT MTS_SDL_DepthStencilState mts_sdl_DepthStencilState_Create(MTS_SDL_DepthStencilStateDescr* descr);

EXPORT void mts_sdl_Device_Bind_DepthStencilState(MTS_SDL_DepthStencilState depthStencilState);

//---------------------------------------------------------------------------------
//MTS_SDL_PolygonState
//---------------------------------------------------------------------------------

enum class MTS_SDL_CullFace
{
	None = 0,
	Front = 1,
	Back = 2,
	FrontAndBack = 3
};


enum class MTS_SDL_FrontFace
{
	//Clockwise primitives are considered front-facing.
	CW = 1,

	//Counter-clockwise primitives are considered front-facing.
	CCW = 2,
};

enum class MTS_SDL_PolygonMode
{
	Point = 0,
	Line = 1,
	Fill = 2,
};

struct MTS_SDL_PolygonStateDescr
{
	MTS_SDL_CullFace CullFace;
	MTS_SDL_FrontFace FrontFace;
	MTS_SDL_PolygonMode PolygonMode;

	void ToDefaults()
	{
		CullFace = MTS_SDL_CullFace::None; 
		FrontFace = MTS_SDL_FrontFace::CCW;
		PolygonMode = MTS_SDL_PolygonMode::Fill;
	}
};

//creates the polygon state.
EXPORT MTS_SDL_PolygonState mts_sdl_PolygonState_Create(MTS_SDL_PolygonStateDescr* descr);

//destroys the polygon state
EXPORT void mts_sdl_PolygonState_Destroy(MTS_SDL_PolygonState polygonState);

EXPORT void mts_sdl_Device_Bind_PolygonState(MTS_SDL_PolygonState polygonState);

//initializes render state to a common baseline
//hopefully this can be made the same between platforms
//notable settings:
//* winding is CCW (opengl default) (todo: why not make an engine config option for this? acknowledging that it's wrong for half of all users)
//* cull is NONE (sensible for 2d games, so you dont have to worry about winding)
//* blending is DISABLED (we'll have a highly managed mechanism for setting blending state later)
EXPORT void mts_sdl_InitialRenderState();

//Creates the given type of shader. arguments style matches opengl.
EXPORT MTS_SDL_Shader mts_sdl_Shader_CreateMulti(MTS_SDL_ShaderType type, int count, const char** codes, const int *lengths);

//Creates the given type of shader. For convenience, the argument is a simple C-string
EXPORT MTS_SDL_Shader mts_sdl_Shader_Create(MTS_SDL_ShaderType type, const char* code);

//Deletes the given shader (it may still be referenced by programs though)
EXPORT void mts_sdl_Shader_Destroy(MTS_SDL_Shader shader);

//Creates a program from the given shaders
EXPORT MTS_SDL_Program mts_sdl_Program_Create(MTS_SDL_Shader vertexShader, MTS_SDL_Shader fragmentShader);

//Destroys a program
EXPORT void mts_sdl_Program_Destroy(MTS_SDL_Program program);

//Binds this program as current - TODO: move onto device
EXPORT void mts_sdl_Program_Bind(MTS_SDL_Program pgm);

//Creates a vertex layout from the provided description
EXPORT MTS_SDL_VertexLayout mts_sdl_VertexLayout_Create(const MTS_SDL_VertexLayoutDescr* layout);

//destroys a vertex layout
EXPORT void mts_sdl_VertexLayout_Release(MTS_SDL_VertexLayout layout);

//signal to begin writing to the constant buffer. you should do this before setting values in it
//you will need to do this each frame or else it will run out eventually
EXPORT void mts_sdl_ConstantBuffer_BeginFrame(int bufferIndex);

//Sets the given data into a constant buffer
//if this mismatches the actual opengl Uniform Block size, you will be sorry.. you may also get an error
EXPORT void mts_sdl_ConstantBuffer_Set(int bufferIndex, void* data, int size);

//creates a dynamic vertex buffer
//it's assumed this will contain a specific type of vertex; addressing will be done with those units
//therefore the MTS_SDL_VertexLayout must be provided too
//really, it's just a MyVertexFormat[], so don't expect anything too powerful
EXPORT MTS_SDL_DynamicVertexBuffer mts_sdl_DynamicVertexBuffer_Create(MTS_SDL_VertexLayout vertexLayout, int nElements);

//signal to begin writing to the dynamic vertex buffer
EXPORT void mts_sdl_DynamicVertexBuffer_Begin(MTS_SDL_DynamicVertexBuffer dvb);

//sets the given data into the dynamic vertex buffer
//returns the initial vertex index written to (for use as a starting when drawing)
EXPORT int mts_sdl_DynamicVertexBuffer_SetElements(MTS_SDL_DynamicVertexBuffer dvb, void* data, int nElements);

//destroys the dynamic vertex buffer
EXPORT void mts_sdl_DynamicVertexBuffer_Destroy(MTS_SDL_DynamicVertexBuffer dvb);

//creates a static vertex buffer
EXPORT MTS_SDL_StaticVertexBuffer mts_sdl_StaticVertexBuffer_Create(MTS_SDL_VertexLayout vertexLayout, int nElements, void* elements);

//destroys the static vertex buffer
EXPORT void mts_sdl_StaticVertexBuffer_Destroy(MTS_SDL_StaticVertexBuffer svb);

//creates a texture
EXPORT MTS_SDL_Texture mts_sdl_Texture_Create(MTS_SDL_TextureDescr* descr);

//destroys a texture
EXPORT void mts_sdl_Texture_Destroy(MTS_SDL_Texture tex);

//creates a render target with the given dimensions
//this is probably oversimplified
EXPORT MTS_SDL_RenderTarget mts_sdl_RenderTarget_Create(int width, int height);

//binds this render target as current
EXPORT void mts_sdl_RenderTarget_Bind(MTS_SDL_RenderTarget rt);

//destroys the given render target
EXPORT void mts_sdl_RenderTarget_Destroy(MTS_SDL_RenderTarget rt);

//creates a blend state object
EXPORT MTS_SDL_BlendState mts_sdl_BlendState_Create(const MTS_SDL_BlendStateDescr* descr);

//destroys the given blend state object
EXPORT void mts_sdl_BlendState_Destroy(MTS_SDL_BlendState blendState);

//creates a sampler state object
EXPORT MTS_SDL_SamplerState mts_sdl_SamplerState_Create(const MTS_SDL_SamplerStateDescr* descr);

//destroys the given sampler state object
EXPORT void mts_sdl_SamplerState_Destroy(MTS_SDL_SamplerState samplerState);

//draws (equivalent of glDrawArrays)
EXPORT void mts_sdl_Draw(MTS_SDL_PrimitiveType primitiveType, int startIndex, int nVertices);

//binds the blend state object (target is usually 0)
EXPORT void mts_sdl_Device_Bind_BlendState(int target, MTS_SDL_BlendState blendState);

//sets the constant color on the blending unit 
EXPORT void mts_sdl_Device_Bind_BlendState_Color(float r, float g, float b, float a);

//binds the dynamic vertex buffer. binds the entire thing (there is no provision for binding a range; control the range through your draw calls instead)
//this binds its associated vertex layout as well
EXPORT void mts_sdl_Device_Bind_DynamicVertexBuffer(MTS_SDL_DynamicVertexBuffer dvb);

//binds the dynamic vertex buffer. binds the entire thing (there is no provision for binding a range; control the range through your draw calls instead)
//this binds its associated vertex layout as well
EXPORT void mts_sdl_Device_Bind_StaticVertexBuffer(MTS_SDL_StaticVertexBuffer svb);

//Binds this vertex layout as current
EXPORT void mts_sdl_Device_Bind_VertexLayout(MTS_SDL_VertexLayout layout);

//Binds the backbuffer as current render target
EXPORT void mts_sdl_Device_Bind_Backbuffer();

//Binds this texture to the given texture unit
EXPORT void mts_sdl_Device_Bind_Texture(int target, MTS_SDL_Texture tex);

//Binds this render target's texture to the given texture unit
EXPORT void mts_sdl_Device_Bind_TextureRT(int target, MTS_SDL_RenderTarget rt);

EXPORT void mts_sdl_Device_Bind_SamplerState(int target, MTS_SDL_SamplerState sampler);

//Sets device's current viewport
EXPORT void mts_sdl_Device_SetViewport(int x, int y, int width, int height);

//Sets device's current depth range
EXPORT void mts_sdl_Device_SetDepthRange(float hither, float yon);

//begins drawing for a frame.
//chiefly, makes sure once-per-frame processes run, whatever they are
EXPORT void mts_sdl_Device_BeginFrame();

//clears the framebuffer
EXPORT void mts_sdl_Device_Clear(MTS_SDL_ClearDescr* descr);

//ends frame and swaps buffers
EXPORT void mts_sdl_Device_EndFrame();

//startup the platform driver
EXPORT void mts_sdl_Init();

//exits the platform driver
EXPORT void mts_sdl_Exit();

} //extern "C"


#define MAKE_BITMASKS(X) \
	inline X operator |(const X &lhs, const X &rhs) { return (X)((int)lhs | (int)rhs); } \
	inline int operator &(const X &lhs, const X &rhs) { return ((int)lhs & (int)rhs); } \

MAKE_BITMASKS(MTS_SDL_ClearMask);
MAKE_BITMASKS(MTS_SDL_BlendWriteMask);

