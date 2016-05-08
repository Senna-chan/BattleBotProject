#include <pspsdk.h>
#include <pspuser.h>
#include <pspdisplay.h>
#include <pspctrl.h>
#include <pspgu.h>
#include <psputility.h>
#include <psputility_netmodules.h>
//#include <psputility_htmlviewer.h>
#include <pspnet.h>
#include <pspnet_inet.h>
#include <pspnet_apctl.h>
#include <pspnet_resolver.h>
#include <psphttp.h>
#include <pspssl.h>
#include <pspctrl.h>
#include <pspdebug.h>
#include "danzeff/danzeff.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <sys/unistd.h>
#include <sys/socket.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <netinet/in.h>

#include "common/callback.h"
#include "common/graphics.h"

#define printf pspDebugScreenPrintf

PSP_MODULE_INFO("HtmlViewer", PSP_MODULE_USER, 1, 1);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER);

void throwError(int milisecs, char *fmt, ...)
{
	va_list list;
	char msg[256];

	va_start(list, fmt);
	vsprintf(msg, fmt, list);
	va_end(list);

	pspDebugScreenInit();
	pspDebugScreenClear();
	pspDebugScreenPrintf(msg);

	sceKernelDelayThread(milisecs * 1000);
	sceKernelExitGame();
}

#define BUF_WIDTH (512)
#define SCR_WIDTH (480)
#define SCR_HEIGHT (272)

static char list[0x10000] __attribute__((aligned(64)));

int running = 0;

void setupGu(void)
{
	sceGuInit();

	sceGuStart(GU_DIRECT, list);
	sceGuDrawBuffer(GU_PSM_8888, 0, BUF_WIDTH);
	sceGuDispBuffer(SCR_WIDTH, SCR_HEIGHT, (void*)0x88000, BUF_WIDTH);
	sceGuDepthBuffer((void*)0x110000, BUF_WIDTH);

	sceGuOffset(0, 0);

	sceGuScissor(0, 0, SCR_WIDTH, SCR_HEIGHT);
	sceGuEnable(GU_SCISSOR_TEST);

	sceGuBlendFunc(GU_ADD, GU_SRC_ALPHA, GU_ONE_MINUS_SRC_ALPHA, 0, 0);
	sceGuEnable(GU_BLEND);

	//sceGuTexFunc(GU_TFX_MODULATE, GU_TCC_RGBA);
	//sceGuTexFilter(GU_LINEAR_MIPMAP_LINEAR, GU_NEAREST);
	//sceGuTexWrap(GU_REPEAT, GU_REPEAT);

	sceGuClearColor(0);
	sceGuClearDepth(0);
	sceGuClearStencil(0);

	sceGuFinish();
	sceGuSync(0, 0);

	sceDisplayWaitVblankStart();
	sceGuDisplay(GU_TRUE);
}

void draw()
{
	sceGuStart(GU_DIRECT, list);
	sceGuClear(GU_COLOR_BUFFER_BIT | GU_STENCIL_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);
	sceGuFinish();
	sceGuSync(0, 0);
}

void loadNetModules()
{
	sceUtilityLoadNetModule(PSP_NET_MODULE_COMMON);
	sceUtilityLoadNetModule(PSP_NET_MODULE_INET);
	sceUtilityLoadNetModule(PSP_NET_MODULE_PARSEURI);
	sceUtilityLoadNetModule(PSP_NET_MODULE_PARSEHTTP);
	sceUtilityLoadNetModule(PSP_NET_MODULE_HTTP);
	sceUtilityLoadNetModule(PSP_NET_MODULE_SSL);
}

void unloadNetModules()
{
	sceUtilityUnloadNetModule(PSP_NET_MODULE_SSL);
	sceUtilityUnloadNetModule(PSP_NET_MODULE_HTTP);
	sceUtilityUnloadNetModule(PSP_NET_MODULE_PARSEHTTP);
	sceUtilityUnloadNetModule(PSP_NET_MODULE_PARSEURI);
	sceUtilityUnloadNetModule(PSP_NET_MODULE_INET);
	sceUtilityUnloadNetModule(PSP_NET_MODULE_COMMON);
}

void netTerm()
{
	sceHttpSaveSystemCookie();
	sceHttpsEnd();
	sceHttpEnd();
	sceSslEnd();
	sceNetApctlTerm();
	sceNetInetTerm();
	sceNetTerm();

	unloadNetModules();
}

void netInit()
{
	int res;

	loadNetModules();

	res = sceNetInit(0x20000, 0x2A, 0, 0x2A, 0);

	if (res < 0)
	{
		throwError(6000, "Error 0x%08X in sceNetInit\n", res);
	}

	res = sceNetInetInit();

	if (res < 0)
	{
		netTerm();
		throwError(6000, "Error 0x%08X in sceNetInetInit\n", res);
	}

	res = sceNetResolverInit();

	if (res < 0)
	{
		netTerm();
		throwError(6000, "Error 0x%08X in sceNetResolverInit\n", res);
	}

	res = sceNetApctlInit(0x1800, 0x30);

	if (res < 0)
	{
		netTerm();
		throwError(6000, "Error 0x%08X in sceNetApctlInit\n", res);
	}

	res = sceSslInit(0x28000);

	if (res < 0)
	{
		netTerm();
		throwError(6000, "Error 0x%08X in sceSslInit\n", res);
	}

	res = sceHttpInit(0x25800);

	if (res < 0)
	{
		netTerm();
		throwError(6000, "Error 0x%08X in sceHttpInit\n", res);
	}

	res = sceHttpsInit(0, 0, 0, 0);
	if (res < 0)
	{
		netTerm();
		throwError(6000, "Error 0x%08X in sceHttpsInit\n", res);
	}

	res = sceHttpsLoadDefaultCert(0, 0);

	if (res < 0)
	{
		netTerm();
		throwError(6000, "Error 0x%08X in sceHttpsLoadDefaultCert\n", res);
	}

	res = sceHttpLoadSystemCookie();

	if (res < 0)
	{
		netTerm();
		throwError(6000, "Error 0x%08X in sceHttpsLoadDefaultCert\n", res);
	}
}

#define BROWSER_MEMORY (10*1024*1024) 

SceUID vpl;
pspUtilityHtmlViewerParam params;

void htmlViewerInit(char *url)
{
	int res;

	vpl = sceKernelCreateVpl("BrowserVpl", PSP_MEMORY_PARTITION_USER, 0, BROWSER_MEMORY + 256, NULL);

	if (vpl < 0)
		throwError(6000, "Error 0x%08X creating vpl.\n", vpl);

	memset(&params, 0, sizeof(pspUtilityHtmlViewerParam));

	params.base.size = sizeof(pspUtilityHtmlViewerParam);

	sceUtilityGetSystemParamInt(PSP_SYSTEMPARAM_ID_INT_LANGUAGE, &params.base.language);
	sceUtilityGetSystemParamInt(PSP_SYSTEMPARAM_ID_INT_UNKNOWN, &params.base.buttonSwap);

	params.base.graphicsThread = 17;
	params.base.accessThread = 19;
	params.base.fontThread = 18;
	params.base.soundThread = 16;
	params.memsize = BROWSER_MEMORY;
	params.initialurl = url;
	params.numtabs = 1;
	params.cookiemode = PSP_UTILITY_HTMLVIEWER_COOKIEMODE_DEFAULT;
	params.homeurl = url;
	params.textsize = PSP_UTILITY_HTMLVIEWER_TEXTSIZE_NORMAL;
	params.displaymode = PSP_UTILITY_HTMLVIEWER_DISPLAYMODE_FIT;
	params.options = PSP_UTILITY_HTMLVIEWER_DISABLE_STARTUP_LIMITS;
	params.interfacemode = PSP_UTILITY_HTMLVIEWER_INTERFACEMODE_NONE;
	params.connectmode = PSP_UTILITY_HTMLVIEWER_CONNECTMODE_MANUAL_ALL;

	// Note the lack of 'ms0:' on the paths	
	params.dldirname = "/PSP/PHOTO";

	res = sceKernelAllocateVpl(vpl, params.memsize, &params.memaddr, NULL);

	if (res < 0)
		throwError(6000, "Error 0x%08X allocating browser memory.\n", res);

	res = sceUtilityHtmlViewerInitStart(&params);

	if (res < 0)
		throwError(6000, "Error 0x%08X initing browser.\n", res);
}

int updateHtmlViewer()
{
	draw();

	switch (sceUtilityHtmlViewerGetStatus())
	{
	case PSP_UTILITY_DIALOG_VISIBLE:
		sceUtilityHtmlViewerUpdate(1);
		break;

	case PSP_UTILITY_DIALOG_QUIT:
		sceUtilityHtmlViewerShutdownStart();
		running = 0;
		break;

	case PSP_UTILITY_DIALOG_NONE:
		running = 0;
		return 0;
		break;

	default:
		break;
	}

	return 1;
}


int main(int argc, char *argv[])
{
	setupExitCallback();
	/*initGraphics();
	danzeff_load();
	pspDebugScreenInit();
	
	if(!danzeff_isinitialized())
	{
		while (1)
		{
			pspDebugScreenSetXY(0, 0);
			printf("HIIIII!");
		}
		sceKernelExitGame();
		return 0;
	}

	danzeff_moveTo(220, 100);
	char val[]= "";
	SceCtrlData buttonInput;*/

	
//	while(1)
//	{
//		sceCtrlPeekBufferPositive(&buttonInput, 1);
//		danzeff_render();
//		int cha = danzeff_readInput(buttonInput);
//		if (cha != 0 && cha != 1 && cha != 2) {
//			strcpy(val, (char)cha);
//		}
//		printf(val);
//		danzeff_dirty();
//	}



	char url[] = "http://192.168.43.251/rcam/index_simple.php";
	setupGu();
	netInit();
	htmlViewerInit(url);
	pspDebugScreenInit();

	int socket_desc;
	struct sockaddr_in server;
	char *message;

	socket_desc = sceNetInetSocket(PF_INET, SOCK_STREAM, 0);
	if (socket_desc == -1)
	{
		printf("Could not create socket\n");
		printf("socket returned $%x\n", socket_desc);
		printf("errno=$%x\n", sceNetInetGetErrno());
		sceDisplayWaitVblankStart();
	}

	server.sin_addr.s_addr = inet_addr("192.168.043.251");
	server.sin_family = AF_INET;
	server.sin_port = htons(20010);

	//Connect to remote server
	if (sceNetInetConnect(socket_desc, (struct sockaddr *)&server, sizeof(server)) < 0)
	{
		printf("Error connecting to socket\n");
		printf("errno=$%x\n", sceNetInetGetErrno());
		sceDisplayWaitVblankStart();
	}

	printf("Connected\n");
	sceDisplayWaitVblankStart();
	//Send some data
	message = "client:connected:psp";
	if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
	{
		printf("Send failed");
		sceDisplayWaitVblankStart();
	}
	printf("Data Send\n");
	sceDisplayWaitVblankStart();

	int running = isRunning();
	int speed = 0;
	int wheelpos = 0;
	int wheelpos1 = 0;
	int wheelpos2 = 0;
	int freq = 50;
	int servoY = 0;
	int servoX = 0;
	SceCtrlData buttonInput;

	sceCtrlSetSamplingCycle(0);
	sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);
	sceKernelDelayThread(1000000);

	while (updateHtmlViewer() && running)
	{
		running = isRunning();
		int cruiseControll = 0;
		int rPressed = 0;
		pspDebugScreenSetXY(0, 0);
		int aX = buttonInput.Lx - 127;
		int aY = (buttonInput.Ly - 127) * -1 + 1;
		if (aX > 27)		aX -= 28;
		else if (aX < -27)	aX += 27;
		else				aX = 0;

		if (aY > 27)		aY -= 28;
		else if (aY < -27) 	aY += 27;
		else				aY = 0;

		printf("Analog X = %d ", aX);
		printf("Analog Y = %d \n", aY);

		sceCtrlPeekBufferPositive(&buttonInput, 1);

		if (buttonInput.Buttons != 0)
		{
			if (buttonInput.Buttons & PSP_CTRL_START) {
				printf("Start");
				running = 0;
			}
			if (buttonInput.Buttons & PSP_CTRL_SELECT)		printf("Select");
			if (buttonInput.Buttons & PSP_CTRL_UP)			printf("Up");
			if (buttonInput.Buttons & PSP_CTRL_DOWN)		printf("Down");
			if (buttonInput.Buttons & PSP_CTRL_RIGHT)		printf("Right");
			if (buttonInput.Buttons & PSP_CTRL_LEFT)		printf("Left");

			if (buttonInput.Buttons & PSP_CTRL_CROSS)		printf("Cross");
			if (buttonInput.Buttons & PSP_CTRL_CIRCLE)		printf("Circle");
			if (buttonInput.Buttons & PSP_CTRL_SQUARE)		printf("Square");
			if (buttonInput.Buttons & PSP_CTRL_TRIANGLE)	printf("Triangle");

			if (buttonInput.Buttons & PSP_CTRL_RTRIGGER)
			{
				rPressed = 1;
			}
			if (buttonInput.Buttons & PSP_CTRL_LTRIGGER)
			{
				cruiseControll = 1;
			}
		}

		if (cruiseControll)
		{
			speed = speed;
			if (aX > 0) {
				wheelpos1 = aX;
				wheelpos2 = 0;
			}
			else if (aX < 0) {
				wheelpos1 = 0;
				wheelpos2 = aX * -1;
			}
			printf("Cruise\n");
		}
		else {
			if (rPressed)// Servo
			{
				servoX = aX;
				servoY = aY;
				printf("No cruise and servo\n");
			}
			else if (!rPressed)// Motor
			{
				speed = aY;
				if (aX > 0) {
					wheelpos1 = aX;
				}
				else if (aX < 0) {
					wheelpos2 = aX * -1;
				}
				printf("No cruise and driving\n");
			}
		}
		char dcmessage[30];
		snprintf(dcmessage, sizeof(dcmessage), "DC:%i,%i,%i,%i:%i,%i", speed, wheelpos1, wheelpos2, freq, servoX, servoY);
		printf(dcmessage);
		socket_desc = sceNetInetSocket(PF_INET, SOCK_STREAM, 0);
		sceNetInetConnect(socket_desc, (struct sockaddr *)&server, sizeof(server));
		if (sceNetInetSend(socket_desc, dcmessage, strlen(dcmessage), 0) < 0)
		{
			printf("Send failed");
			sceDisplayWaitVblankStart();
		}
		sceNetInetClose(socket_desc);
		sceDisplayWaitVblankStart();
		sceGuSwapBuffers();
	}
	netTerm();
	sceKernelFreeVpl(vpl, params.memaddr);
	sceKernelDeleteVpl(vpl);
	sceKernelExitGame();

	return 0;
}