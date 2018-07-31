#include <string.h>

#include "SDL.h"

#include "PADL.h"

void PADL_EasyPadPoll(int index, PADL_EasyPadState* outState)
{
	memset(outState,0,sizeof(*outState));

	auto sdlpad = SDL_JoystickOpen(index);

	if(!sdlpad)
		return;

	outState->attached = true;
	
	//for investigation
	//for(int i=0;i<32;i++) outState->buttons |= ((!!SDL_JoystickGetButton(sdlpad, i))?1:0)<<i;

	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 0) ? PADL_EASYPAD_BUTTON_A : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 1) ? PADL_EASYPAD_BUTTON_B : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 2) ? PADL_EASYPAD_BUTTON_X : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 3) ? PADL_EASYPAD_BUTTON_Y : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 4) ? PADL_EASYPAD_BUTTON_LEFT_SHOULDER : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 5) ? PADL_EASYPAD_BUTTON_RIGHT_SHOULDER : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 6) ? PADL_EASYPAD_BUTTON_BACK : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 7) ? PADL_EASYPAD_BUTTON_START : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 8) ? PADL_EASYPAD_BUTTON_LEFT_THUMB : 0;
	outState->buttons |= !!SDL_JoystickGetButton(sdlpad, 9) ? PADL_EASYPAD_BUTTON_RIGHT_THUMB : 0;
	
	static const u8 hatLut[] = {
		0, 
		PADL_EASYPAD_BUTTON_UP,
		PADL_EASYPAD_BUTTON_RIGHT,
		PADL_EASYPAD_BUTTON_UP | PADL_EASYPAD_BUTTON_RIGHT,
		PADL_EASYPAD_BUTTON_DOWN,
		0, //down and up
		PADL_EASYPAD_BUTTON_DOWN | PADL_EASYPAD_BUTTON_RIGHT,
		0, //down, right, and up
		PADL_EASYPAD_BUTTON_LEFT,
		PADL_EASYPAD_BUTTON_LEFT | PADL_EASYPAD_BUTTON_UP,
		0, //left and right
		0, //left, right, and up
		PADL_EASYPAD_BUTTON_LEFT | PADL_EASYPAD_BUTTON_DOWN,
		0, //left, down, and up
		0, //left, down, and right
		0, //left, right, down, and up
	};

	auto hat = SDL_JoystickGetHat(sdlpad, 0);
	outState->buttons |= hatLut[hat];
}