#include <MTS/MTS_Native.h>

#include "thirdparty/zlib/zlib.h"

#include "codec.h"

//applies zlib uncompress
EXPORT bool zlib_uncompress(void *dest, void* src, int destLen, int srcLen)
{
	uLongf zDestLen = destLen;
	int ret = uncompress((Bytef*)dest, &zDestLen, (const Bytef*)src, srcLen);
	return ret == Z_OK;
}

//applies zlib compress
//returns resulting size; -1 if dest was too small
EXPORT int zlib_compress(void *dest, void* src, int destLen, int srcLen, int level)
{
	uLongf zDestLen;
	int ret = compress2((Bytef*)dest, &zDestLen, (const Bytef*)src, srcLen, level);
	if(ret == Z_OK) return (int)zDestLen;
	else return -1;
}
