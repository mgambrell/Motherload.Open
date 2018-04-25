#ifdef MTS_PLATFORMTYPE_Proto

#define EXPORT(rtype,name) extern "C" __declspec(dllexport) rtype name

#else

namespace MTS { namespace Engine {

#define EXPORT(rtype, name) rtype Native$_$S_##name##_$PInvokeWrapper

#endif

EXPORT(int, GetPlatformType)()
{
	#if MTS_PLATFORMTYPE_Proto
	return 0;
	#elif MTS_PLATFORMTYPE_Windows
	return 1;
	#elif MTS_PLATFORMTYPE_Switch
	return 2;
	#else
	return -1;
	#endif
}

EXPORT(int, DllTest)(int num)
{
	return num*2;
}

#if !MTS_PLATFORMTYPE_Proto

} } //end namespaces

#endif 
