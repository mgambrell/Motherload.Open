#pragma once

//putting all the MTS apis for here now, and API reference

#define MTS_EXPORT __declspec(dllexport)

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

struct MTS_SDL_ShaderStruct;
struct MTS_SDL_ProgramStruct;
struct MTS_SDL_VertexLayoutStruct;
struct MTS_SDL_DynamicVertexBufferStruct;
struct MTS_SDL_StaticVertexBufferStruct;
struct MTS_SDL_RenderTargetStruct;
struct MTS_SDL_BlendStateStruct;
struct MTS_SDL_SamplerStateStruct;
struct MTS_SDL_TextureStruct;

//opaque handle for a compiled shader resource
typedef MTS_SDL_ShaderStruct* MTS_SDL_Shader;

//opaque handle for a compiled program resource
typedef MTS_SDL_ProgramStruct* MTS_SDL_Program;

//opaque handle for a vertex layout resource
typedef MTS_SDL_VertexLayoutStruct* MTS_SDL_VertexLayout;

//opaque handle for a dynamic vertex buffer resource
typedef MTS_SDL_DynamicVertexBufferStruct* MTS_SDL_DynamicVertexBuffer;

//opaque handle for a static vertex buffer resource
typedef MTS_SDL_StaticVertexBufferStruct* MTS_SDL_StaticVertexBuffer;

//opaque handle for a render target resource
typedef MTS_SDL_RenderTargetStruct* MTS_SDL_RenderTarget;

//opaque handle for a blend state resource
typedef MTS_SDL_BlendStateStruct* MTS_SDL_BlendState;

//opaque handle for a blend state resource
typedef MTS_SDL_SamplerStateStruct* MTS_SDL_SamplerState;

//opaque handle fot a texture resource
typedef MTS_SDL_TextureStruct* MTS_SDL_Texture;

//initializes render state to a common baseline
//hopefully this can be made the same between platforms
//notable settings:
//* winding is CCW (opengl default) (todo: why not make an engine config option for this? acknowledging that it's wrong for half of all users)
//* cull is NONE (sensible for 2d games, so you dont have to worry about winding)
//* blending is DISABLED (we'll have a highly managed mechanism for setting blending state later)
MTS_EXPORT void mts_sdl_InitialRenderState();

//Creates the given type of shader. arguments style matches opengl.
MTS_EXPORT MTS_SDL_Shader mts_sdl_Shader_CreateMulti(MTS_SDL_ShaderType type, int count, const char** codes, const int *lengths);

//Creates the given type of shader. For convenience, the argument is a simple C-string
MTS_EXPORT MTS_SDL_Shader mts_sdl_Shader_Create(MTS_SDL_ShaderType type, const char* code);

//Deletes the given shader (it may still be referenced by programs though)
MTS_EXPORT void mts_sdl_Shader_Destroy(MTS_SDL_Shader shader);

//Creates a program from the given shaders
MTS_EXPORT MTS_SDL_Program mts_sdl_Program_Create(MTS_SDL_Shader vertexShader, MTS_SDL_Shader fragmentShader);

//Destroys a program
MTS_EXPORT void mts_sdl_Program_Destroy(MTS_SDL_Program program);

//Binds this program as current - TODO: move onto device
MTS_EXPORT void mts_sdl_Program_Bind(MTS_SDL_Program pgm);

//Creates a vertex layout from the provided description
MTS_EXPORT MTS_SDL_VertexLayout mts_sdl_VertexLayout_Create(const MTS_SDL_VertexLayoutDescr* layout);

//destroys a vertex layout
MTS_EXPORT void mts_sdl_VertexLayout_Release(MTS_SDL_VertexLayout layout);

//signal to begin writing to the constant buffer. you should do this before setting values in it
//you will need to do this each frame or else it will run out eventually
MTS_EXPORT void mts_sdl_ConstantBuffer_BeginFrame(int bufferIndex);

//Sets the given data into a constant buffer
//if this mismatches the actual opengl Uniform Block size, you will be sorry.. you may also get an error
MTS_EXPORT void mts_sdl_ConstantBuffer_Set(int bufferIndex, void* data, int size);

//creates a dynamic vertex buffer
//it's assumed this will contain a specific type of vertex; addressing will be done with those units
//therefore the MTS_SDL_VertexLayout must be provided too
//really, it's just a MyVertexFormat[], so don't expect anything too powerful
MTS_EXPORT MTS_SDL_DynamicVertexBuffer mts_sdl_DynamicVertexBuffer_Create(MTS_SDL_VertexLayout vertexLayout, int nElements);

//signal to begin writing to the dynamic vertex buffer
MTS_EXPORT void mts_sdl_DynamicVertexBuffer_Begin(MTS_SDL_DynamicVertexBuffer dvb);

//sets the given data into the dynamic vertex buffer
//returns the initial vertex index written to (for use as a starting when drawing)
MTS_EXPORT int mts_sdl_DynamicVertexBuffer_SetElements(MTS_SDL_DynamicVertexBuffer dvb, void* data, int nElements);

//destroys the dynamic vertex buffer
MTS_EXPORT void mts_sdl_DynamicVertexBuffer_Destroy(MTS_SDL_DynamicVertexBuffer dvb);

//creates a static vertex buffer
MTS_EXPORT MTS_SDL_StaticVertexBuffer mts_sdl_StaticVertexBuffer_Create(MTS_SDL_VertexLayout vertexLayout, int nElements, void* elements);

//destroys the static vertex buffer
MTS_EXPORT void mts_sdl_StaticVertexBuffer_Destroy(MTS_SDL_StaticVertexBuffer svb);

//creates a texture
MTS_EXPORT MTS_SDL_Texture mts_sdl_Texture_Create(MTS_SDL_TextureDescr* descr);

//destroys a texture
MTS_EXPORT void mts_sdl_Texture_Destroy(MTS_SDL_Texture tex);

//creates a render target with the given dimensions
//this is probably oversimplified
MTS_EXPORT MTS_SDL_RenderTarget mts_sdl_RenderTarget_Create(int width, int height);

//binds this render target as current
MTS_EXPORT void mts_sdl_RenderTarget_Bind(MTS_SDL_RenderTarget rt);

//destroys the given render target
MTS_EXPORT void mts_sdl_RenderTarget_Destroy(MTS_SDL_RenderTarget rt);

//creates a blend state object
MTS_EXPORT MTS_SDL_BlendState mts_sdl_BlendState_Create(const MTS_SDL_BlendStateDescr* descr);

//destroys the given blend state object
MTS_EXPORT void mts_sdl_BlendState_Destroy(MTS_SDL_BlendState blendState);

//creates a sampler state object
MTS_EXPORT MTS_SDL_SamplerState mts_sdl_SamplerState_Create(const MTS_SDL_SamplerStateDescr* descr);

//destroys the given sampler state object
MTS_EXPORT void mts_sdl_SamplerState_Destroy(MTS_SDL_SamplerState samplerState);

//draws (equivalent of glDrawArrays)
MTS_EXPORT void mts_sdl_Draw(MTS_SDL_PrimitiveType primitiveType, int startIndex, int nVertices);

//binds the blend state object (target is usually 0)
MTS_EXPORT void mts_sdl_Device_Bind_BlendState(int target, MTS_SDL_BlendState blendState);

//sets the constant color on the blending unit 
MTS_EXPORT void mts_sdl_Device_Bind_BlendState_Color(float r, float g, float b, float a);

//binds the dynamic vertex buffer. binds the entire thing (there is no provision for binding a range; control the range through your draw calls instead)
//this binds its associated vertex layout as well
MTS_EXPORT void mts_sdl_Device_Bind_DynamicVertexBuffer(MTS_SDL_DynamicVertexBuffer dvb);

//binds the dynamic vertex buffer. binds the entire thing (there is no provision for binding a range; control the range through your draw calls instead)
//this binds its associated vertex layout as well
MTS_EXPORT void mts_sdl_Device_Bind_StaticVertexBuffer(MTS_SDL_StaticVertexBuffer svb);

//Binds this vertex layout as current
MTS_EXPORT void mts_sdl_Device_Bind_VertexLayout(MTS_SDL_VertexLayout layout);

//Binds the backbuffer as current render target
MTS_EXPORT void mts_sdl_Device_Bind_Backbuffer();

//Binds this texture to the given texture unit
MTS_EXPORT void mts_sdl_Device_Bind_Texture(int target, MTS_SDL_Texture tex);

//Binds this render target's texture to the given texture unit
MTS_EXPORT void mts_sdl_Device_Bind_TextureRT(int target, MTS_SDL_RenderTarget rt);

MTS_EXPORT void mts_sdl_Device_Bind_SamplerState(int target, MTS_SDL_SamplerState sampler);

//Sets device's current viewport
MTS_EXPORT void mts_sdl_Device_SetViewport(int x, int y, int width, int height);

//Sets device's current depth range
MTS_EXPORT void mts_sdl_Device_SetDepthRange(float hither, float yon);

//begins drawing for a frame.
//chiefly, makes sure once-per-frame processes run, whatever they are
MTS_EXPORT void mts_sdl_Device_BeginFrame();

//clears the framebuffer
MTS_EXPORT void mts_sdl_Device_Clear(MTS_SDL_ClearDescr* descr);

//ends frame and swaps buffers
MTS_EXPORT void mts_sdl_Device_EndFrame();

//startup the platform driver
MTS_EXPORT void mts_sdl_Init();

//exits the platform driver
MTS_EXPORT void mts_sdl_Exit();

} //extern "C"


#define MAKE_BITMASKS(X) \
	inline X operator |(const X &lhs, const X &rhs) { return (X)((int)lhs | (int)rhs); } \
	inline int operator &(const X &lhs, const X &rhs) { return ((int)lhs & (int)rhs); } \

MAKE_BITMASKS(MTS_SDL_ClearMask);
MAKE_BITMASKS(MTS_SDL_BlendWriteMask);

