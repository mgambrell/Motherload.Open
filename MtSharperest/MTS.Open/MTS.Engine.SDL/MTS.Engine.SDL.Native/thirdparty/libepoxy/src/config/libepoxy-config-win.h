#define ENABLE_EGL 0

#define ENABLE_GLX 0

#undef HAVE_KHRPLATFORM_H

#define inline __inline

#define EPOXY_PUBLIC extern

//cut down on libepoxy noise in windows
#define _X86_
#include <minwindef.h>