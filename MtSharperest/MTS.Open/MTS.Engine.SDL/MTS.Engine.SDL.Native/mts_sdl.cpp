//https://www.khronos.org/opengl/wiki/Common_Mistakes

#include <stdio.h>
#include <string.h>
#include <stdint.h>
#include <stdlib.h>

//needed from winnt.h in x64 for some reason
#include <intrin.h>

//this is required to get epoxy .h matching the .cpp it was built with. weird, I dont know. it's complicated, but its got a complicated job to do..
#include <epoxy/../../src/config.h>

#include <epoxy/gl.h>

#include "SDL.h"

#include "mts_sdl.h"

static SDL_Window* sdl_window;

static int _Attribute_Format_Lookup[] = {
	GL_FLOAT, GL_HALF_FLOAT,
	GL_BYTE, GL_UNSIGNED_BYTE,
	GL_SHORT, GL_UNSIGNED_SHORT,
	GL_INT, GL_UNSIGNED_INT
};

static MTS_SDL_VertexLayout _currLayout;

struct MTS_SDL_ShaderStruct
{
	GLuint glid;
	MTS_SDL_ShaderType type;
};

struct MTS_SDL_ProgramStruct
{
	GLuint glid;
};

struct MTS_SDL_BlendStateStruct
{
	MTS_SDL_BlendStateDescr descr;
};

struct MTS_SDL_SamplerStateStruct
{
	GLuint glid;
	MTS_SDL_SamplerStateDescr descr;
};

struct MTS_SDL_DepthStencilStateStruct
{
	MTS_SDL_DepthStencilStateDescr descr;
};

struct MTS_SDL_PolygonStateStruct
{
	MTS_SDL_PolygonStateDescr descr;
};

struct MTS_SDL_TextureStruct
{
	GLuint glid;

	//naturally, the pixels in here don't belong to us anymore
	MTS_SDL_TextureDescr descr;
};

struct MTS_SDL_VertexLayoutStruct
{
	GLuint glid;
	
	MTS_SDL_VertexLayoutDescr descr;
	int refcount;

	~MTS_SDL_VertexLayoutStruct()
	{
		delete[] descr.attributes;
	}
};

struct MTS_SDL_DynamicVertexBufferStruct
{
	GLuint glid;
	int size;
	int cursor;
	MTS_SDL_VertexLayout layout;
};

struct MTS_SDL_StaticVertexBufferStruct
{
	GLuint glid;
	MTS_SDL_VertexLayout layout;
};

struct MTS_SDL_RenderTargetStruct
{
	int width, height;
	GLuint fb_id, dsb_id, tex_id;
};

MTS_SDL_VertexLayout mts_sdl_VertexLayout_Create(const MTS_SDL_VertexLayoutDescr* layoutDescr)
{
	//some references: https://www.khronos.org/opengl/wiki/Tutorial2:_VAOs,_VBOs,_Vertex_and_Fragment_Shaders_(C_/_SDL)

	const auto &nAttributes = layoutDescr->nAttributes;
	const auto &attributes = layoutDescr->attributes;

	GLuint vaoid;
	glGenVertexArrays(1, &vaoid);

	//TODO - what about the current binding?
	glBindVertexArray(vaoid);

	int attribIndexUsed = 0;
	for(int i=0;i<nAttributes;i++)
	{
		const auto &attr = attributes[i];
		
		//track which indexes have been used
		int attribIndexMask = 1<<attr.index;
		if(attribIndexMask & attribIndexUsed)
		{
			printf("Reuse of vertex layout attribute index %d\n",attr.index);
			glDeleteVertexArrays(1, &vaoid);
			return nullptr;
		}
		attribIndexUsed |= attribIndexMask;

		//TODO - look for invalid values
		auto gl_format = _Attribute_Format_Lookup[(int)attr.format];

		//OOPS - this requires a relatively new opengl version. Oh well, one thing at a time.

		//TODO - handle normalizing
		glEnableVertexAttribArray(attr.index);
		//glVertexAttribPointer(attr.index, attr.size, glFormat, GL_FALSE, layoutDescr->stride, (void*)attr.offset);
		glVertexAttribFormat(attr.index, attr.size, gl_format, GL_FALSE, attr.offset);
		
		//this basically sets which stream will be used for this attribute
		//we dont support that yet
		glVertexAttribBinding(attr.index, 0); //needs GL 4.3
	}

	//do I need to copy the attributes? Maybe to support older GL versions. that's why I'm holding onto them
	//we might want to bake it down into a more compact representation though, for that purpose
	MTS_SDL_VertexLayoutStruct* ret = new MTS_SDL_VertexLayoutStruct();
	ret->glid = vaoid;
	ret->refcount = 1;
	ret->descr = *layoutDescr;
	ret->descr.attributes = new MTS_SDL_VertexLayoutDescr_Attribute[nAttributes];
	memcpy((void*)ret->descr.attributes, layoutDescr->attributes, nAttributes*sizeof(MTS_SDL_VertexLayoutDescr_Attribute));

	return ret;
}

void mts_sdl_VertexLayout_Release(MTS_SDL_VertexLayout layout)
{
	layout->refcount--;
	if(layout->refcount == 0)
	{
		if(_currLayout == layout)
			glBindVertexArray(0);
		delete layout;
	}
}

void mts_sdl_Device_Bind_VertexLayout(MTS_SDL_VertexLayout layout)
{
	glBindVertexArray(layout->glid);
	_currLayout = layout;
}

static void PrintInfoLog(GLuint glid, bool pgm)
{
	GLchar *infoLog;
	int infoLogLen = 0;
	if(pgm)
		glGetProgramiv(glid, GL_INFO_LOG_LENGTH, &infoLogLen);
	else
		glGetShaderiv(glid, GL_INFO_LOG_LENGTH, &infoLogLen);
	int charsWritten = 0;
	if (infoLogLen > 0)
	{
		infoLog = new GLchar[infoLogLen];
		if(pgm)
			glGetProgramInfoLog(glid, infoLogLen, &charsWritten, infoLog);
		else
			glGetShaderInfoLog(glid, infoLogLen, &charsWritten, infoLog);
		printf("%s\n",infoLog);
		delete [] infoLog;
	}
}

MTS_SDL_Shader mts_sdl_Shader_CreateMulti(MTS_SDL_ShaderType type, int count, const char** codes, const int *lengths)
{
	//some references: https://www.khronos.org/opengl/wiki/Tutorial1:_Rendering_shapes_with_glDrawRangeElements,_VAO,_VBO,_shaders_(C%2B%2B_/_freeGLUT)
	auto glShaderType = type == MTS_SDL_ShaderType::Vertex ? GL_VERTEX_SHADER : GL_FRAGMENT_SHADER;
	auto shaderId = glCreateShader(glShaderType);

	glShaderSource(shaderId, count, codes, lengths);

	GLint compiled;
	glCompileShader(shaderId);
	glGetShaderiv(shaderId, GL_COMPILE_STATUS, &compiled);
	if(compiled==FALSE)
	{
		static const char* _shaderTypeStrings[] = { "VERTEX", "FRAGMENT" };
		printf("ERROR COMPILING %s SHADER\n", _shaderTypeStrings[(int)type]);
		PrintInfoLog(shaderId,false);
		glDeleteShader(shaderId);
		return nullptr;
	}

	//shader compiled OK

	MTS_SDL_ShaderStruct* ret = new MTS_SDL_ShaderStruct();
	ret->type = type;
	ret->glid = shaderId;

	return ret;
}

MTS_SDL_Shader mts_sdl_Shader_Create(MTS_SDL_ShaderType type, const char* code)
{
	const char* codes[] = { code };
	return mts_sdl_Shader_CreateMulti(type, 1, codes, nullptr);
}

void mts_sdl_Shader_Destroy(MTS_SDL_Shader shader)
{
	glDeleteShader(shader->glid);
	delete shader;
}

MTS_SDL_Program mts_sdl_Program_Create(MTS_SDL_Shader vertexShader, MTS_SDL_Shader fragmentShader)
{
	auto pgmId = glCreateProgram();

	glAttachShader(pgmId, vertexShader->glid);
	glAttachShader(pgmId, fragmentShader->glid);

	//test
	glBindAttribLocation(pgmId, 0, "InVertex");

	glLinkProgram(pgmId);
	
	GLint IsLinked;
	glGetProgramiv(pgmId, GL_LINK_STATUS, (GLint *)&IsLinked);

	glDetachShader(pgmId, vertexShader->glid);
	glDetachShader(pgmId, fragmentShader->glid);

	if(IsLinked==FALSE)
	{
		printf("ERROR LINKING PROGRAM\n");
		PrintInfoLog(pgmId,true);
		glDeleteProgram(pgmId);
		return nullptr;
	}

	MTS_SDL_ProgramStruct* ret = new MTS_SDL_ProgramStruct();
	ret->glid = pgmId;

	return ret;
}

void mts_sdl_Program_Bind(MTS_SDL_Program pgm)
{
	glUseProgram(pgm->glid);
}

void mts_sdl_Program_Destroy(MTS_SDL_Program program)
{
	glDeleteProgram(program->glid);
	delete program;
}

MTS_SDL_DepthStencilState mts_sdl_DepthStencilState_Create(MTS_SDL_DepthStencilStateDescr* descr)
{
	auto ret = new MTS_SDL_DepthStencilStateStruct();
	ret->descr = *descr;
	return ret;
}

MTS_SDL_PolygonState mts_sdl_PolygonState_Create(MTS_SDL_PolygonStateDescr* descr)
{
	auto ret = new MTS_SDL_PolygonStateStruct();
	ret->descr = *descr;
	return ret;
}

void mts_sdl_PolygonState_Destroy(MTS_SDL_PolygonState polygonState)
{
	delete polygonState;
}

struct ConstantBufferStream
{
	GLuint cbid;
	int cbBufferCursor;
	int cbSize;
} constantBuffers[16];

static GLint uniformBufferAlignSize = 0;

void _engine_init()
{
	//for now i'm managing the constant buffers internally
	for(int i=0;i<16;i++)
	{
		static const int cbSize = 16*1024*1024;
		GLuint cbid;
		glGenBuffers(1,&cbid);
		glBindBuffer(GL_UNIFORM_BUFFER, cbid);
		glBufferData(GL_UNIFORM_BUFFER, cbSize, nullptr, GL_STREAM_DRAW);


		constantBuffers[i].cbid = cbid;
		constantBuffers[i].cbSize = 16*1024*1024;
		constantBuffers[i].cbBufferCursor = 0;
	}

	//find out how the uniform buffers need to be aligned
	glGetIntegerv(GL_UNIFORM_BUFFER_OFFSET_ALIGNMENT, &uniformBufferAlignSize);

	//https://www.opengl.org/discussion_boards/showthread.php/197854-GL_MAP_UNSYNCHRONIZED_BIT-and-glFenceSync

	//note: this won't work:
	//glMapBufferRange(GL_UNIFORM_BUFFER, 0, cbsize, GL_MAP_UNSYNCHRONIZED_BIT | GL_MAP_INVALIDATE_RANGE_BIT | GL_MAP_WRITE_BIT | GL_MAP_FLUSH_EXPLICIT_BIT | GL_MAP_PERSISTENT_BIT);
	//(leave the whole buffer mapped, and manually flush the needed pieces)
	//that's because it requires use of glBufferStorage instead of glBufferData, which was not available until gl 4.4
	//could enable that under the right circumstances though.
}

void mts_sdl_ConstantBuffer_Set(int bufferIndex, void* data, int size)
{
	ConstantBufferStream& cb = constantBuffers[bufferIndex];

	//make sure we have room in our buffer; otherwise just fail
	if(cb.cbBufferCursor + size > cb.cbSize)
		return;

	int err = glGetError();
	glBindBuffer(GL_UNIFORM_BUFFER,cb.cbid);
	
	void* ptr = glMapBufferRange(GL_UNIFORM_BUFFER, cb.cbBufferCursor, size, GL_MAP_UNSYNCHRONIZED_BIT | GL_MAP_INVALIDATE_RANGE_BIT | GL_MAP_WRITE_BIT);
	memcpy(ptr, data, size);
	glUnmapBuffer(GL_UNIFORM_BUFFER);
	
	//bind this new range as active for the uniforms
	glBindBufferRange(GL_UNIFORM_BUFFER, bufferIndex, cb.cbid, cb.cbBufferCursor, size);
	
	//move to the next legal slot in the uniform buffer
	cb.cbBufferCursor += size;
	cb.cbBufferCursor = (cb.cbBufferCursor + uniformBufferAlignSize - 1)&~(uniformBufferAlignSize-1);
}

void mts_sdl_ConstantBuffer_BeginFrame(int bufferIndex)
{
	constantBuffers[bufferIndex].cbBufferCursor = 0;
}

MTS_SDL_DynamicVertexBuffer mts_sdl_DynamicVertexBuffer_Create(MTS_SDL_VertexLayout vertexLayout, int nElements)
{
	const int size = vertexLayout->descr.stride * nElements;

	GLuint glid;

	glGenBuffers(1,&glid);
	glBindBuffer(GL_ARRAY_BUFFER, glid);
	glBufferData(GL_ARRAY_BUFFER, size, nullptr, GL_STREAM_DRAW);

	vertexLayout->refcount++;

	MTS_SDL_DynamicVertexBufferStruct* ret = new MTS_SDL_DynamicVertexBufferStruct();
	ret->glid = glid;
	ret->size = size;
	ret->cursor = 0;
	ret->layout = vertexLayout;
	return ret;
}

//signal to begin writing to the dynamic vertex buffer
void mts_sdl_DynamicVertexBuffer_Begin(MTS_SDL_DynamicVertexBuffer dvb)
{
	dvb->cursor = 0;
}

int mts_sdl_DynamicVertexBuffer_SetElements(MTS_SDL_DynamicVertexBuffer dvb, void* data, int nElements)
{
	const int stride = dvb->layout->descr.stride;
	const int size = nElements * stride;

	//make sure we have room in our buffer; otherwise just fail
	if(dvb->cursor * stride + size > dvb->size)
		return -1;

	int err = glGetError();
	glBindBuffer(GL_ARRAY_BUFFER,dvb->glid);

	void* ptr = glMapBufferRange(GL_ARRAY_BUFFER, dvb->cursor * stride, size, GL_MAP_UNSYNCHRONIZED_BIT | GL_MAP_INVALIDATE_RANGE_BIT | GL_MAP_WRITE_BIT);
	memcpy(ptr, data, size);
	glUnmapBuffer(GL_ARRAY_BUFFER);

	//bind this new range as active for the uniforms
	//TODO: need user-supplied mechanism for this
	//glBindBufferRange(GL_ARRAY_BUFFER, 0, cbid, cbBufferCursor, size);

	int ret = dvb->cursor;
	
	dvb->cursor += nElements;

	return ret;
}

void mts_sdl_Device_Bind_DynamicVertexBuffer(MTS_SDL_DynamicVertexBuffer dvb)
{
	//todo - glVertexArrayVertexBuffer - check for this, its more efficient
	mts_sdl_Device_Bind_VertexLayout(dvb->layout);
	
	//oops, along with glVertexAttribFormat, this is probably too new. oh well, im modeling newer hardware
	glBindVertexBuffer(0, dvb->glid, 0, dvb->layout->descr.stride);
}

void mts_sdl_Device_Bind_StaticVertexBuffer(MTS_SDL_StaticVertexBuffer svb)
{
	mts_sdl_Device_Bind_VertexLayout(svb->layout);

	//oops, along with glVertexAttribFormat, this is probably too new. oh well, im modeling newer hardware
	glBindVertexBuffer(0, svb->glid, 0, svb->layout->descr.stride);
}


void mts_sdl_DynamicVertexBuffer_Destroy(MTS_SDL_DynamicVertexBuffer dvb)
{
	mts_sdl_VertexLayout_Release(dvb->layout);
	glDeleteBuffers(1,&dvb->glid);
	delete dvb;
}

MTS_SDL_StaticVertexBuffer mts_sdl_StaticVertexBuffer_Create(MTS_SDL_VertexLayout vertexLayout, int nElements, void* elements)
{
	const int size = vertexLayout->descr.stride * nElements;

	GLuint glid;

	glGenBuffers(1,&glid);
	glBindBuffer(GL_ARRAY_BUFFER, glid);
	glBufferData(GL_ARRAY_BUFFER, size, elements, GL_STATIC_DRAW);
	
	vertexLayout->refcount++;

	MTS_SDL_StaticVertexBufferStruct* ret = new MTS_SDL_StaticVertexBufferStruct();
	ret->glid = glid;
	ret->layout = vertexLayout;

	return ret;
}

void mts_sdl_StaticVertexBuffer_Destroy(MTS_SDL_StaticVertexBuffer svb)
{
	mts_sdl_VertexLayout_Release(svb->layout);
	glDeleteBuffers(1,&svb->glid);
	delete svb;
}

MTS_SDL_Texture mts_sdl_Texture_Create(MTS_SDL_TextureDescr* descr)
{
	//https://www.khronos.org/opengl/wiki/Texture_Storage
	GLuint glid;
	glGenTextures(1, &glid);
	glBindTexture(GL_TEXTURE_2D, glid);
	static const struct GlTextureParameters {
		GLint internalFormat, format, type;
	} gltexparams[] = {
		{ 0, 0, 0}, //None
		{ GL_RGBA8, GL_RGBA, GL_UNSIGNED_BYTE },  //RGBA8
		{ GL_RGBA8, GL_BGRA, GL_UNSIGNED_BYTE },  //BGRA8
		{ GL_RGB8, GL_RGB, GL_UNSIGNED_BYTE },  //RGB8
		{ GL_RGB8, GL_BGR, GL_UNSIGNED_BYTE },  //BGR8
		{ GL_RED, GL_RED, GL_UNSIGNED_BYTE },  //R8
	};

	const auto &theseparams = gltexparams[(int)descr->format];

	glTexImage2D(GL_TEXTURE_2D, 0, theseparams.internalFormat, descr->width, descr->height, 0, theseparams.format, theseparams.type, descr->pixels);
	
	MTS_SDL_TextureStruct* ret = new MTS_SDL_TextureStruct();
	ret->glid = glid;
	ret->descr = *descr;
	return ret;
}

void mts_sdl_Texture_Destroy(MTS_SDL_Texture tex)
{
	glDeleteTextures(1, &tex->glid);
	delete tex;
}


MTS_SDL_RenderTarget mts_sdl_RenderTarget_Create(int width, int height)
{
	MTS_SDL_RenderTargetStruct rt;

	rt.width = width;
	rt.height = height;

	//create the framebuffer and bind it.
	glGenFramebuffers(1, &rt.fb_id);
	glBindFramebuffer(GL_FRAMEBUFFER, rt.fb_id);

	//Create a depth or depth/stencil renderbuffer, allocate storage for it, and attach it to the framebuffer’s depth attachment point.
	//(TODO: this needs to be more controllable..)
	glGenRenderbuffers(1, &rt.dsb_id);
	glBindRenderbuffer(GL_RENDERBUFFER, rt.dsb_id);
	glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, width, height);
	glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, rt.dsb_id);

	//Create the destination texture, and attach it to the framebuffer's color attachment point.
	glGenTextures(1, &rt.tex_id);
	glBindTexture(GL_TEXTURE_2D, rt.tex_id);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, rt.tex_id, 0);

	auto status = glCheckFramebufferStatus(GL_FRAMEBUFFER);
	if(status != GL_FRAMEBUFFER_COMPLETE)
		return nullptr;

	MTS_SDL_RenderTargetStruct* ret = new MTS_SDL_RenderTargetStruct();
	*ret = rt;
	return ret;
}

void mts_sdl_RenderTarget_Bind(MTS_SDL_RenderTarget rt)
{
	if(rt)
		glBindFramebuffer(GL_FRAMEBUFFER, rt->fb_id);
	else 
		mts_sdl_Device_Bind_Backbuffer(); //shortcut

	//notes for future reference:
	//https://stackoverflow.com/questions/7207422/setting-up-opengl-multiple-render-targets
	//GLenum buffers[] = { GL_COLOR_ATTACHMENT0_EXT, GL_COLOR_ATTACHMENT1_EXT };
	//glDrawBuffers(2, buffers);
}

void mts_sdl_RenderTarget_Destroy(MTS_SDL_RenderTarget rt)
{
	if(!rt) return;
	glDeleteTextures(1, &rt->tex_id);
	glDeleteRenderbuffers(1, &rt->dsb_id);
	glDeleteFramebuffers(1, &rt->fb_id);
	delete rt;
}

MTS_SDL_BlendState mts_sdl_BlendState_Create(const MTS_SDL_BlendStateDescr* descr)
{
	MTS_SDL_BlendStateStruct* ret = new MTS_SDL_BlendStateStruct();
	ret->descr = *descr;
	return ret;
}

void mts_sdl_BlendState_Destroy(MTS_SDL_BlendState blendState)
{
	delete blendState;
}

MTS_SDL_SamplerState mts_sdl_SamplerState_Create(const MTS_SDL_SamplerStateDescr* descr)
{
	GLuint glid;

	glGenSamplers(1, &glid);

	static const GLint filterLut[] = {
		GL_NEAREST,
		GL_LINEAR,
		GL_NEAREST_MIPMAP_NEAREST, GL_LINEAR_MIPMAP_NEAREST,
		GL_NEAREST_MIPMAP_LINEAR, GL_LINEAR_MIPMAP_LINEAR
	};

	static const GLint wrapLut[] = {
		GL_CLAMP_TO_EDGE,
		GL_MIRRORED_REPEAT,
		GL_REPEAT
	};


	glSamplerParameteri(glid, GL_TEXTURE_MIN_FILTER, filterLut[(int)descr->minFilter]);
	glSamplerParameteri(glid, GL_TEXTURE_MAG_FILTER, filterLut[(int)descr->magFilter]);
	glSamplerParameteri(glid, GL_TEXTURE_WRAP_S, wrapLut[(int)descr->wrapS]);
	glSamplerParameteri(glid, GL_TEXTURE_WRAP_T, wrapLut[(int)descr->wrapT]);
	glSamplerParameterf(glid, GL_TEXTURE_MIN_LOD, descr->minLod);
	glSamplerParameterf(glid, GL_TEXTURE_MAX_LOD, descr->maxLod);

	MTS_SDL_SamplerStateStruct* ret = new MTS_SDL_SamplerStateStruct();
	ret->glid = glid;
	ret->descr = *descr;
	return ret;
}

void mts_sdl_SamplerState_Destroy(MTS_SDL_SamplerState samplerState)
{
	glDeleteSamplers(1, &samplerState->glid);
	delete samplerState;
}

void mts_sdl_Device_Bind_SamplerState(int target, MTS_SDL_SamplerState samplerState)
{
	glBindSampler(target, samplerState->glid);
}

void mts_sdl_Device_Bind_DepthStencilState(MTS_SDL_DepthStencilState depthStencilState)
{
	const auto& descr = depthStencilState->descr;

	static const GLuint comparisonLut[] = {
		0xFFFF, //Invalid
		GL_NEVER,
		GL_LESS, GL_EQUAL, GL_LEQUAL, GL_GREATER, GL_NOTEQUAL, GL_GEQUAL,
		GL_ALWAYS
	};

	static const GLenum stencilOpLut[] = {
		0xFFFF, //Invalid
		GL_KEEP, GL_ZERO,
		GL_REPLACE,
		GL_INCR,GL_DECR,
		GL_INVERT,
		GL_INCR_WRAP, GL_DECR_WRAP
	};

	if(descr.DepthWriteEnable) glDepthMask(GL_TRUE); else glDepthMask(GL_FALSE);

	if(descr.DepthTestEnable) {
		glEnable(GL_DEPTH_TEST); 
		glDepthFunc(comparisonLut[(int)descr.DepthFunc]);
	}
	else glDisable(GL_DEPTH_TEST);
	
	if(descr.StencilTestEnable) {
		glEnable(GL_STENCIL_TEST);
		glStencilOpSeparate(GL_FRONT, stencilOpLut[(int)descr.StencilFailFront], stencilOpLut[(int)descr.StencilDepthFailFront], stencilOpLut[(int)descr.StencilDepthPassFront]);
		glStencilOpSeparate(GL_BACK, stencilOpLut[(int)descr.StencilFailBack], stencilOpLut[(int)descr.StencilDepthFailBack], stencilOpLut[(int)descr.StencilDepthPassBack]);
		glStencilFuncSeparate(GL_FRONT, comparisonLut[(int)descr.StencilFuncFront], 0, 0); //TODO - mask and ref. need to cache last set values
		glStencilFuncSeparate(GL_BACK, comparisonLut[(int)descr.StencilFuncBack], 0, 0); //TODO - mask and ref. need to cache last set values
	}
	else glDisable(GL_STENCIL_TEST);
}

void mts_sdl_Device_Bind_PolygonState(MTS_SDL_PolygonState polygonState)
{
	const auto& descr = polygonState->descr;

	static const GLuint modeLut[] = {
		GL_POINT, GL_LINE, GL_FILL
	};

	static const GLuint cullFaceLut[] = {
		0xFFFF, //Invalid
		GL_FRONT, GL_BACK, GL_FRONT_AND_BACK
	};

	static const GLuint frontFaceLut[] = {
		GL_CW, GL_CCW,
	};

	glFrontFace(frontFaceLut[(int)descr.FrontFace]);
	glPolygonMode(GL_FRONT_AND_BACK, modeLut[(int)descr.PolygonMode]);

	if(descr.CullFace == MTS_SDL_CullFace::None) {
		glDisable(GL_CULL_FACE);
	} else {
		glEnable(GL_CULL_FACE);
		glCullFace(cullFaceLut[(int)descr.CullFace]);
	}
}

void mts_sdl_Device_Bind_BlendState(int target, MTS_SDL_BlendState blendState)
{
	const auto& descr = blendState->descr;

	static const GLuint eqLut[] = {
		GL_FUNC_ADD, GL_FUNC_SUBTRACT, GL_FUNC_REVERSE_SUBTRACT, 
		GL_MIN, GL_MAX
	};
	static const GLuint funcLut[] = {
		GL_ZERO, GL_ONE,
		GL_SRC_COLOR, GL_ONE_MINUS_SRC_COLOR,
		GL_DST_COLOR, GL_ONE_MINUS_DST_COLOR,
		GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA,
		GL_DST_ALPHA, GL_ONE_MINUS_DST_ALPHA,
		GL_CONSTANT_COLOR, GL_ONE_MINUS_CONSTANT_COLOR,
		GL_CONSTANT_ALPHA, GL_ONE_MINUS_CONSTANT_ALPHA,
		GL_SRC_ALPHA_SATURATE,
		GL_SRC1_COLOR, GL_ONE_MINUS_SRC1_COLOR,
		GL_SRC1_ALPHA, GL_ONE_MINUS_SRC_ALPHA
	};

	if(!blendState->descr.enabled) {
		glDisable(GL_BLEND);
		return;
	}

	int red = descr.writeMask & MTS_SDL_BlendWriteMask::Red;
	int green = descr.writeMask & MTS_SDL_BlendWriteMask::Green;
	int blue = descr.writeMask & MTS_SDL_BlendWriteMask::Blue;
	int alpha = descr.writeMask & MTS_SDL_BlendWriteMask::Alpha;

	glColorMaski(target, red, green, blue, alpha);

	glEnable(GL_BLEND);

	//todo - could validate / apply LUT in creation
	glBlendEquationSeparatei(target, eqLut[(int)descr.eqColor], eqLut[(int)descr.eqAlpha]);
	glBlendFuncSeparatei(target, 
		funcLut[(int)descr.srcColor], funcLut[(int)descr.dstColor],
		funcLut[(int)descr.srcAlpha], funcLut[(int)descr.dstAlpha]
		);
	
}

void mts_sdl_Device_Bind_BlendState_Color(float r, float g, float b, float a)
{
	glBlendColor(r,g,b,a);
}

void mts_sdl_Device_Bind_Backbuffer()
{
	glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

void mts_sdl_Device_Bind_Texture(int target, MTS_SDL_Texture tex)
{
	glActiveTexture(GL_TEXTURE0 + target);
	glBindTexture(GL_TEXTURE_2D, tex->glid);
}

void mts_sdl_Device_Bind_TextureRT(int target, MTS_SDL_RenderTarget rt)
{
	glActiveTexture(GL_TEXTURE0 + target);
	glBindTexture(GL_TEXTURE_2D, rt->tex_id);
}

void mts_sdl_Device_SetViewport(int x, int y, int width, int height)
{
	glViewport(x,y,width,height);
}

void mts_sdl_Device_SetDepthRange(float hither, float yon)
{
	glDepthRange(hither, yon);
}

void mts_sdl_Device_BeginFrame()
{
	mts_sdl_Device_Bind_Backbuffer();
}

void mts_sdl_Device_Clear(MTS_SDL_ClearDescr* descr)
{
	GLbitfield glmask = 0;

	if(descr->mask & MTS_SDL_ClearMask::Color)
	{
		glmask |= GL_COLOR_BUFFER_BIT;
		glClearColor(descr->r, descr->g, descr->b, descr->a);
	}
	if(descr->mask & MTS_SDL_ClearMask::DepthStencil)
	{
		glmask |= GL_DEPTH_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT;
		glClearDepth(descr->depth);
		glClearStencil(descr->stencil);
	}

	glClear(glmask);
}

void mts_sdl_Device_EndFrame()
{
	SDL_GL_SwapWindow(sdl_window);

	SDL_Event Event;
	while(SDL_PollEvent(&Event))
	{
		// Later, you'll be adding your code that handles keyboard / mouse input here
	}
}

void mts_sdl_Draw(MTS_SDL_PrimitiveType primitiveType, int startIndex, int nVertices)
{
	//map primitive type
	GLenum glPrimitiveType = 0;
	if(primitiveType == MTS_SDL_PrimitiveType::Triangles) glPrimitiveType = GL_TRIANGLES;
	if(primitiveType == MTS_SDL_PrimitiveType::TriangleStrip) glPrimitiveType = GL_TRIANGLE_STRIP;
	if(primitiveType == MTS_SDL_PrimitiveType::Trianglefan) glPrimitiveType = GL_TRIANGLE_FAN;

	//check for error
	if(glPrimitiveType == 0) return;

	glDrawArrays(glPrimitiveType, startIndex, nVertices);
}

void __stdcall MessageCallback( GLenum source,
	GLenum type,
	GLuint id,
	GLenum severity,
	GLsizei length,
	const GLchar* message,
	const void* userParam )
{
	if(type == GL_DEBUG_TYPE_ERROR)
	{
		printf( "GL CALLBACK: %s type = 0x%x, severity = 0x%x, message = %s\n",
			( type == GL_DEBUG_TYPE_ERROR ? "** GL ERROR **" : "" ),
			type, severity, message );
	}
}

void mts_sdl_Init()
{
	SDL_Surface* screenSurface = NULL; 
	SDL_Init(SDL_INIT_EVERYTHING);
	
	//but yet we use samplers (unclear whether 3.2 or 3.3), so I don't know if it's safe
	SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
	SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 2);
	//SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE); //needed?
	
	//we have to set some kind of defaults here. hopefully we can expose options to behave differently
	//(this main window parameters stuff is opengl legacy garbage IMO)
	SDL_GL_SetAttribute(SDL_GL_DOUBLEBUFFER, 1);
	SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 24);
	
	sdl_window = SDL_CreateWindow("Mount Sharperest", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 1280, 720, SDL_WINDOW_SHOWN | SDL_WINDOW_OPENGL);
	SDL_GLContext glcontext = SDL_GL_CreateContext(sdl_window);
	SDL_GL_MakeCurrent(sdl_window, glcontext);
	
	glEnable              ( GL_DEBUG_OUTPUT );
	glDebugMessageCallback( (GLDEBUGPROC) MessageCallback, 0 );

	//COMPRESSED_RGB_S3TC_DXT1_EXT
	//COMPRESSED_RGBA_S3TC_DXT5_EXT
	//bool test = epoxy_has_gl_extension("GL_EXT_texture_compression_s3tc");
	
	////list all extensions, for reference during development
	//int num_extensions;
	//glGetIntegerv(GL_NUM_EXTENSIONS, &num_extensions);
	//for (int i = 0; i < num_extensions; i++) {
	//	const char *gl_ext = (const char *)glGetStringi(GL_EXTENSIONS, i);
	//	printf("%s\n",gl_ext);
	//}

	_engine_init();
}

void mts_sdl_Exit()
{
	SDL_DestroyWindow(sdl_window); 
	SDL_Quit(); 
}