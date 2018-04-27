#include <MTS/MTS_Native.h>

EXPORT int DllTest(int num)
{
	return num*2;
}
EXPORT int GetPlatformType()
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

