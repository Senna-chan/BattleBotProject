#include <pspkernel.h> 
#include <pspnet.h>
#include <pspnet_inet.h>
#include <pspnet_apctl.h>
#include <psppower.h>
#include <pspdisplay.h>
static int powerStatus = 0;
static int exitRequest = 0;
#define printf  pspDebugScreenPrintf
int GetPowerStatus()
{
	return powerStatus;
}

int isRunning()
{
	return !exitRequest;
}

int exitCallback(int arg1, int arg2, void *common)
{
	sceNetApctlTerm();

	sceNetInetTerm();

	sceNetTerm();

	sceKernelExitGame();
	exitRequest = 1;
	return 0;
}
void delayy(int milliseconds) {
	sceKernelDelayThread(1000 * milliseconds);
}
int power_callback(int unknown, int pwrflags, void *common)
{
	/* check for power switch and suspending as one is manual and the other automatic */
	if (pwrflags & PSP_POWER_CB_POWER_SWITCH || pwrflags & PSP_POWER_CB_SUSPENDING) {
		powerStatus = 1;
	}
	else if (pwrflags & PSP_POWER_CB_RESUMING) {
		printf(
			"first arg: 0x%08X, flags: 0x%08X: resuming from suspend mode\n",
			unknown, pwrflags);
		powerStatus = 2;
	}
	else if (pwrflags & PSP_POWER_CB_RESUME_COMPLETE) {
		printf(
			"first arg: 0x%08X, flags: 0x%08X: resume complete\n", unknown, pwrflags);
		powerStatus = 3;
	}
	else if (pwrflags & PSP_POWER_CB_STANDBY) {
		printf(
			"first arg: 0x%08X, flags: 0x%08X: entering standby mode\n", unknown, pwrflags);
		powerStatus = 4;
	}
	sceDisplayWaitVblankStart();

	return 0;
}

int callbackThread(SceSize args, void *argp)
{
	int callbackID;

	callbackID = sceKernelCreateCallback("Exit Callback", exitCallback, NULL);
	sceKernelRegisterExitCallback(callbackID);
	callbackID = sceKernelCreateCallback("Power Callback", power_callback, NULL);
	scePowerRegisterCallback(0, callbackID);
	sceKernelSleepThreadCB();
	return 0;
}


int setupExitCallback()
{
	int threadID = 0;

	threadID = sceKernelCreateThread("Callback Update Thread", callbackThread, 0x11, 0xFA0, THREAD_ATTR_USER, 0);

	if (threadID >= 0)
	{
		sceKernelStartThread(threadID, 0, 0);
	}

	return threadID;
}