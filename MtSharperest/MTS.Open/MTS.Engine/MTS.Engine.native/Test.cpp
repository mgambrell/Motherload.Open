#ifdef BRUTE

#define EXPORT(rtype, name) rtype Native$_$S_##name##_$PInvokeWrapper

namespace MTS { namespace Engine {

#else

#define EXPORT(rtype,name) extern "C" __declspec(dllexport) rtype name

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

#ifdef BRUTE

} } //end namespaces

#endif 
