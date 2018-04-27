#pragma once

#if MTS_PLATFORMTYPE_Proto

#define EXPORT extern "C" __declspec(dllexport) 

#else

#define EXPORT extern "C"

#endif