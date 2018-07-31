#pragma once

//when prototyping, we have dlls; thus, when functions should be exported appropriately
//otherwise we're bruting, and static linking everything together
#if MTS_PLATFORMTYPE_Proto
#define EXPORT extern "C" __declspec(dllexport) 
#else
#define EXPORT extern "C"
#endif

//common types. I know, we should be using stdint. Not gonna.

#ifdef _MSC_VER
typedef unsigned __int64 u64;
typedef signed __int64 s64;
typedef unsigned __int32 u32;
typedef signed __int32 s32;
typedef unsigned __int16 u16;
typedef signed __int16 s16;
typedef unsigned __int8 u8;
typedef signed __int8 s8;
typedef float f32;
typedef double f64;
#ifdef _M_AMD64
typedef u64 uptr;
typedef s64 sptr;
#else
typedef u32 uptr;
typedef s32 sptr;
#endif
#endif
