#include <MTS/MTS_Native.h>

//"PADL" - a library for accessing SDL input devices

//yeah, these correspond to xinput buttons..
#define PADL_EASYPAD_BUTTON_UP (1<<0)
#define PADL_EASYPAD_BUTTON_DOWN (1<<1)
#define PADL_EASYPAD_BUTTON_LEFT (1<<2)
#define PADL_EASYPAD_BUTTON_RIGHT (1<<3)
#define PADL_EASYPAD_BUTTON_START (1<<4)
#define PADL_EASYPAD_BUTTON_BACK (1<<5)
#define PADL_EASYPAD_BUTTON_LEFT_THUMB (1<<6)
#define PADL_EASYPAD_BUTTON_RIGHT_THUMB (1<<7)
#define PADL_EASYPAD_BUTTON_LEFT_SHOULDER (1<<8)
#define PADL_EASYPAD_BUTTON_RIGHT_SHOULDER (1<<9)
#define PADL_EASYPAD_BUTTON_A (1<<10)
#define PADL_EASYPAD_BUTTON_B (1<<11)
#define PADL_EASYPAD_BUTTON_X (1<<12)
#define PADL_EASYPAD_BUTTON_Y (1<<13)
//hmmm lets put this in a pad utilities module
////OK, new virtual buttons
////first, virtualized (stick->dpad mappings)
//#define PADL_EASYPAD_DIGITALIZED_LSTICK_UP (1<<14)
//#define PADL_EASYPAD_DIGITALIZED_LSTICK_DOWN (1<<15)
//#define PADL_EASYPAD_DIGITALIZED_LSTICK_LEFT (1<<16)
//#define PADL_EASYPAD_DIGITALIZED_LSTICK_RIGHT (1<<17)
//#define PADL_EASYPAD_DIGITALIZED_RSTICK_UP (1<<18)
//#define PADL_EASYPAD_DIGITALIZED_RSTICK_DOWN (1<<19)
//#define PADL_EASYPAD_DIGITALIZED_RSTICK_LEFT (1<<20)
//#define PADL_EASYPAD_DIGITALIZED_RSTICK_RIGHT (1<<21)
////next, composite all the directions

extern "C"
{
	//Output of PADL_EasyPadPoll
	struct PADL_EasyPadState
	{
		u32 buttons;
		bool attached;
	};

	//Dead simple mechanism for polling a pad, assumed to be xinput
	//It's sloppy, a real game should use something more sophisticated. But it's good for prototyping.
	EXPORT void PADL_EasyPadPoll(int index, PADL_EasyPadState* outState);
}