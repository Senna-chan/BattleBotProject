/*
* Licensing stuff from the original Net Dialog Sample for PSP
* PSP Software Development Kit - http://www.pspdev.org
* -----------------------------------------------------------------------
* Licensed under the BSD license, see LICENSE in PSPSDK root for details.
*
* main.c - Net dialog sample for connecting to an access point
*
* For OE firmwares, this sample must be run under the 3.xx kernel.
*
* Copyright (c) 2007 David Perry (Insert_Witty_Name)
*
*
*
*/

#include <pspkernel.h>
#include <pspdisplay.h>
#include <string.h>
#include <psputility.h>
#include <pspgu.h>
#include <pspgum.h>
#include <pspsdk.h>
#include <pspnet.h>
#include <pspnet_inet.h>
#include <pspnet_apctl.h>

#include <pspctrl.h>
#include <pspdebug.h>
#include <psputility_netmodules.h>
#include <stdarg.h>
#include <stdio.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <sys/time.h> 
#include "common/callback.h"
#include "common/graphics.h"
#include <psppower.h>
#include <stdlib.h>
#include "common/ini.h"

PSP_MODULE_INFO("Simple Battlebot Client PSP", 0, 1, 1);

#define printf pspDebugScreenPrintf
#define RGB(r, g, b) ((r)|((g)<<8)|((b)<<16))
#define true 1
#define false 1
int ColorRed = RGB(255, 0, 0); // Red
int ColorBlue = RGB(0, 0, 255); // Blue
int ColorGreen = RGB(0, 255, 0); // Green
int ColorBlack = RGB(0, 0, 0); // Black
int ColorWhite = RGB(255, 255, 255); // Black
int ColorGray = RGB(126, 137, 126); // Gray-ish
static int running = 1;

/* Graphics stuff, based on cube sample */
static unsigned int __attribute__((aligned(16))) list[262144];

#define BUF_WIDTH (512)
#define SCR_WIDTH (480)
#define SCR_HEIGHT (272)
#define PIXEL_SIZE (4)
#define FRAME_SIZE (BUF_WIDTH * SCR_HEIGHT * PIXEL_SIZE)
#define ZBUF_SIZE (BUF_WIDTH SCR_HEIGHT * 2)

static void setupGu()
{
	sceGuInit();

	sceGuStart(GU_DIRECT, list);
	sceGuDrawBuffer(GU_PSM_8888, (void*)0, BUF_WIDTH);
	sceGuDispBuffer(SCR_WIDTH, SCR_HEIGHT, (void*)0x88000, BUF_WIDTH);
	sceGuDepthBuffer((void*)0x110000, BUF_WIDTH);
	sceGuOffset(2048 - (SCR_WIDTH / 2), 2048 - (SCR_HEIGHT / 2));
	sceGuViewport(2048, 2048, SCR_WIDTH, SCR_HEIGHT);
	sceGuDepthRange(0xc350, 0x2710);
	sceGuScissor(0, 0, SCR_WIDTH, SCR_HEIGHT);
	sceGuEnable(GU_SCISSOR_TEST);
	sceGuDepthFunc(GU_GEQUAL);
	sceGuEnable(GU_DEPTH_TEST);
	sceGuFrontFace(GU_CW);
	sceGuShadeModel(GU_SMOOTH);
	sceGuEnable(GU_CULL_FACE);
	sceGuEnable(GU_CLIP_PLANES);
	sceGuFinish();
	sceGuSync(0, 0);

	sceDisplayWaitVblankStart();
	sceGuDisplay(GU_TRUE);
}

static void drawStuff(void)
{
	static int val = 0;

	sceGuStart(GU_DIRECT, list);

	//sceGuClearColor(0xff554433);
	sceGuClearColor(0x000000);
	sceGuClearDepth(0);
	sceGuClear(GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);

	sceGumMatrixMode(GU_PROJECTION);
	sceGumLoadIdentity();
	sceGumPerspective(75.0f, 16.0f / 9.0f, 0.5f, 1000.0f);

	sceGumMatrixMode(GU_VIEW);
	sceGumLoadIdentity();

	sceGumMatrixMode(GU_MODEL);
	sceGumLoadIdentity();

	sceGuFinish();
	sceGuSync(0, 0);

	val++;
}

void delay(int milliseconds) {
	sceKernelDelayThread(1000 * milliseconds);
}



void netInit(void)
{
	sceNetInit(128 * 1024, 42, 4 * 1024, 42, 4 * 1024);
	sceNetInetInit();
	sceNetApctlInit(0x8000, 48);
}

void netTerm(void)
{
	sceNetApctlTerm();
	sceNetInetTerm();
	sceNetTerm();
}

int netDialog()
{
	int done = 0;
	running = isRunning();
	pspUtilityNetconfData data;

	memset(&data, 0, sizeof(data));
	data.base.size = sizeof(data);
	data.base.language = PSP_SYSTEMPARAM_LANGUAGE_ENGLISH;
	data.base.buttonSwap = PSP_UTILITY_ACCEPT_CROSS;
	data.base.graphicsThread = 17;
	data.base.accessThread = 19;
	data.base.fontThread = 18;
	data.base.soundThread = 16;
	data.action = PSP_NETCONF_ACTION_CONNECTAP;

	struct pspUtilityNetconfAdhoc adhocparam;
	memset(&adhocparam, 0, sizeof(adhocparam));
	data.adhocparam = &adhocparam;

	sceUtilityNetconfInitStart(&data);

	while (running)
	{
		drawStuff();

		switch (sceUtilityNetconfGetStatus())
		{
		case PSP_UTILITY_DIALOG_NONE:
			break;

		case PSP_UTILITY_DIALOG_VISIBLE:
			sceUtilityNetconfUpdate(1);
			break;

		case PSP_UTILITY_DIALOG_QUIT:
			sceUtilityNetconfShutdownStart();
			break;

		case PSP_UTILITY_DIALOG_FINISHED:
			done = 1;
			break;

		default:
			break;
		}

		sceDisplayWaitVblankStart();
		sceGuSwapBuffers();
		if (done)
			break;
	}

	return 1;
}

int map(int x, int in_min, int in_max, int out_min, int out_max)
{
	return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

void PrintText(int x, int y, int color, const char* format, ...) {
	va_list fmtargs;
	char buffer[1024];

	va_start(fmtargs, format);
	vsnprintf(buffer, sizeof(buffer) - 1, format, fmtargs);
	va_end(fmtargs);
	printTextScreen(x, y, buffer, color);
}

void PrintError(const char* format, ...) {
	va_list fmtargs;
	char buffer[1024];
	
	va_start(fmtargs, format);
	vsnprintf(buffer, sizeof(buffer) - 1, format, fmtargs);
	va_end(fmtargs);
	printTextScreen(10, 230, "ERROR: ", ColorRed);
	printTextScreen(50, 230, buffer, ColorRed);
}

int QuitScreen()
{
	SceCtrlData pad;
	sceCtrlSetSamplingCycle(0);
	sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);
	clearScreen(ColorBlack);
	PrintText(150, 120, ColorWhite, "Do you want to quit?");
	PrintText(150, 130, ColorRed, "X = Yes");
	PrintText(210, 130, ColorWhite, "/");
	PrintText(225, 130, ColorGreen, "O = No");
	flipScreen();
	while (true)
	{
		sceCtrlPeekBufferPositive(&pad, 1);

		if (pad.Buttons != 0)
		{
			if(pad.Buttons & PSP_CTRL_CROSS)
			{
				clearScreen(ColorBlack);
				PrintText(170, 125, ColorWhite, "Bye Bye");
				flipScreen();
				delay(250);
				scePowerUnlock(0);
				return 1;
			}
			if(pad.Buttons & PSP_CTRL_CIRCLE)
			{
				return 0;
			}
		}
	}
}
void PrintToScreen(Color color)
{
	flipScreen();
	clearScreen(color);
}

int StringStartsWith(const char *pre, const char *str)
{
	return strncmp(pre, str, strlen(pre)) == 0;
}
void remove_all_chars(char* str, char c) {
	char *pr = str, *pw = str;
	while (*pr) {
		*pw = *pr++;
		pw += (*pw != c);
	}
	*pw = '\0';
}
// Config file stuff
typedef struct
{
	char *ip1;
	char *ip2;
	char *ip3;
	int ipp1;
	int ipp2;
	int ipp3;
	int ipp4;
	int waittime;
} configuration;
static int handler(void* user, const char* section, const char* name,
	const char* value)
{
	configuration* pconfig = (configuration*)user;

#define MATCH(s, n) strcmp(section, s) == 0 && strcmp(name, n) == 0
	if (MATCH("IP", "ip1")) {
		pconfig->ip1 = strdup(value);
	}
	else if (MATCH("IP", "ip2")) {
		pconfig->ip2 = strdup(value);
	}
	else if (MATCH("IP", "ip3")) {
		pconfig->ip3 = strdup(value);
	}
//	else if(MATCH("CIP", "ipp1"))
//	{
//		pconfig->ipp1 = atoi(value);
//	}
//	else if(MATCH("CIP", "ipp2"))
//	{
//		pconfig->ipp2 = atoi(value);
//	}
//	else if(MATCH("CIP", "ipp3"))
//	{
//		pconfig->ipp3 = atoi(value);
//	}
//	else if(MATCH("CIP", "ipp4"))
//	{
//		pconfig->ipp4 = atoi(value);
//	}
//	else if(MATCH("MISC", "waittime"))
//	{
//		pconfig->waittime = atoi(value);
//	}
	else {
		return 0;  /* unknown section/name, error */
	}
	return 1;
}


int main(int argc, char *argv[])
{
	setupExitCallback();
	scePowerSetClockFrequency(333, 333, 166);
	int powerlock = 1;

	sceUtilityLoadNetModule(PSP_NET_MODULE_COMMON);

	sceUtilityLoadNetModule(PSP_NET_MODULE_INET);

	setupGu();
	netInit();
	netDialog();
	char *ip = "";
	initGraphics();
	pspDebugScreenInit();
	pspDebugScreenSetXY(0, 0);

	clearScreen(ColorBlack);
	flipScreen();
	int ipToChoose;


	int ip1 = 192;
	int ip2 = 168;
	int ip3 = 000;
	int ip4 = 001;

	configuration config;

	if (ini_parse("config.ini", handler, &config) < 0) {
		PrintError("Can't load config.ini");
		PrintText(50, 100, ColorRed, "Please make sure that the config.ini file is present");
		PrintToScreen(ColorBlack);
		delay(1000);
		return 0;
	}
	

	while (isRunning()) {
		PrintText(10, 10, ColorWhite, "Select /\\ for %s", config.ip1);
		PrintText(10, 20, ColorWhite, "Select o for  %s", config.ip2);
		PrintText(10, 30, ColorWhite, "Select x for  %s", config.ip3);
		PrintText(10, 40, ColorWhite, "Select [] for manual entering IP W.I.P.");
		flipScreen();
		sceDisplayWaitVblankStart();
		SceCtrlData pad;
		sceCtrlSetSamplingCycle(0);
		sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);
		while (1) {
			sceCtrlPeekBufferPositive(&pad, 1);

			if (pad.Buttons != 0)
			{

				if (pad.Buttons & PSP_CTRL_TRIANGLE) {
					ipToChoose = 1;
					break;
				}
				if (pad.Buttons & PSP_CTRL_CIRCLE) {
					ipToChoose = 2;
					break;
				}
				if (pad.Buttons & PSP_CTRL_CROSS) {
					ipToChoose = 3;
					break;
				}
				if (pad.Buttons & PSP_CTRL_START) {
					if (QuitScreen())
					{
						netTerm();
						sceKernelExitGame();
						return 0;
					}
				}
				if (pad.Buttons & PSP_CTRL_SQUARE) {
					ipToChoose = 4;
					break;
				}
			}
		}
		switch (ipToChoose)
		{
		case 1:
			ip = config.ip1;
			break;
		case 2:
			ip = config.ip2;
			break;
		case 3:
			ip = config.ip3;
			break;
		case 4:;
			int oldButton = 0;
			int index = 1;
			flipScreen();
			while (1) {
				sceCtrlPeekBufferPositive(&pad, 1);
				if (pad.Buttons != 0)
				{
					if (pad.Buttons & PSP_CTRL_UP && pad.Buttons != oldButton) {
						switch (index) {
						case 1:
							if (ip1 < 255) ip1++;
							break;
						case 2:
							if (ip2 < 255) ip2++;
							break;
						case 3:
							if (ip3 < 255) ip3++;
							break;
						case 4:
							if (ip4 < 254) ip4++;
							break;
						}
					}
					if (pad.Buttons & PSP_CTRL_RTRIGGER && pad.Buttons != oldButton)
					{
						switch (index) {
						case 1:
							if (ip1 < 245) ip1 += 10;
							break;
						case 2:
							if (ip2 < 245) ip2 += 10;
							break;
						case 3:
							if (ip3 < 245) ip3 += 10;
							break;
						case 4:
							if (ip4 < 244) ip4 += 10;
							break;
						}
					}

					if (pad.Buttons & PSP_CTRL_DOWN && pad.Buttons != oldButton) {
						switch (index) {
						case 1:
							if (ip1 > 0) ip1--;
							break;
						case 2:
							if (ip2 > 0) ip2--;
							break;
						case 3:
							if (ip3 > 0) ip3--;
							break;
						case 4:
							if (ip4 > 1) ip4--;
							break;
						}
					}
					if (pad.Buttons & PSP_CTRL_LTRIGGER && pad.Buttons != oldButton)
					{
						switch (index) {
						case 1:
							if (ip1 >= 10) ip1 -= 10;
							break;
						case 2:
							if (ip2 >= 10) ip2 -= 10;
							break;
						case 3:
							if (ip3 >= 10) ip3 -= 10;
							break;
						case 4:
							if (ip4 >= 11) ip4 -= 10;
							break;
						}
					}


					if (pad.Buttons & PSP_CTRL_RIGHT && pad.Buttons != oldButton)
					{
						if (index != 4) index++;
					}
					if (pad.Buttons & PSP_CTRL_LEFT && pad.Buttons != oldButton)
					{
						if (index != 1) index--;
					}

					if (pad.Buttons & PSP_CTRL_CROSS)
					{
						break;
					}
					if (pad.Buttons & PSP_CTRL_START)
					{
						if (QuitScreen())
						{
							netTerm();
							sceKernelExitGame();
							return 0;
						}
					}
					PrintText(10, 10, ColorWhite, "Enter a ip address here:");
					if (index == 1) PrintText(50, 50, ColorBlue, "%i", ip1);
					else			PrintText(50, 50, ColorWhite, "%i", ip1);
					PrintText(74, 50, ColorWhite, ".");
					if (index == 2) PrintText(80, 50, ColorBlue, "%i", ip2);
					else			PrintText(80, 50, ColorWhite, "%i", ip2);
					PrintText(104, 50, ColorWhite, ".");
					if (index == 3) PrintText(110, 50, ColorBlue, "%i", ip3);
					else			PrintText(110, 50, ColorWhite, "%i", ip3);
					PrintText(132, 50, ColorWhite, ".");
					if (index == 4) PrintText(138, 50, ColorBlue, "%i", ip4);
					else			PrintText(138, 50, ColorWhite, "%i", ip4);

					PrintText(10, 230, ColorWhite, "Controls: Left=goto left, Right=goto right, Up=+1, Down=-1");
					PrintText(10, 240, ColorWhite, "R=+10, L=-10, Start=quit, X=confirm");
					flipScreen();
					clearScreen(ColorBlack);
				}
				oldButton = pad.Buttons;
			}
			snprintf(ip, 20, "%i.%i.%i.%i", ip1, ip2, ip3, ip4);
			break;
		default:
			break;
		}
		PrintText(100, 80, ColorRed, "IP: %s", ip);
		flipScreen();
		delay(1000);
		pspDebugScreenClear();
		int socket_desc;
		struct sockaddr_in server;
		char *message;
		pspDebugScreenSetXY(0, 0);
		socket_desc = sceNetInetSocket(AF_INET, SOCK_DGRAM, 0);
		if (socket_desc == -1)
		{
			PrintError("Could not create socket. errno=$%x", sceNetInetGetErrno());
			flipScreen();
			sceDisplayWaitVblankStart();
			delay(1000);
			netTerm();
			sceKernelExitGame();
			return 0;
		}

		server.sin_addr.s_addr = inet_addr(ip);
		server.sin_family = AF_INET;
		server.sin_port = htons(20010);

		//Connect to remote server
		if (sceNetInetConnect(socket_desc, (struct sockaddr *)&server, sizeof(server)) < 0)
		{
			PrintError("Error connecting to socket. errno=$%x", sceNetInetGetErrno());
			PrintToScreen(ColorBlack);
			delay(1000);
			netTerm();
			sceKernelExitGame();
		}
		int timeout = 20000; // in microseconds
		int err = sceNetInetSetsockopt(socket_desc, SOL_SOCKET, SO_RCVTIMEO, (char*)&timeout, sizeof(timeout));
		if (err != 0) {
			PrintError("set SO_RCVTIMEO failed");
			PrintToScreen(ColorBlack);
			delay(1000);
		}
		//Send some data
		message = "client:psp:connected";
		if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
		{
			PrintError("Send failed");
			PrintToScreen(ColorBlack);
			delay(5000);
			PrintError("Trying to send handshake again");
			PrintToScreen(ColorBlack);
			delay(500);
			if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
			{
				PrintError("Send failed. Shutting down");
				PrintToScreen(ColorBlack);
				delay(500);
				netTerm();
				sceKernelExitGame();
			}
		}
		int speed = 0;
		int wheelpos1 = 0;
		int wheelpos2 = 0;
		int servoY = 0;
		int servoX = 0;

		// Turn speed vars
		int turngear = 4;
		int turnmapMax = 100;
		// Normal speed vars
		int mapMax = 100;
		int mapMin = -100;
		int gear = 4;

		char buf[255];
		buf[0] = '\0';
		char temp[10];
		temp[0] = '\0';
		char *dofbuf[255];
		dofbuf[0] = '\0';
		char *gpsbuf[255];
		gpsbuf[0] = '\0';
		char *parsedbufA[9];
		char *parsedbufB[9];
		int oldButton = 0;
		socklen_t server_addr_len = 0;
		int waittime = 40;

		buf[0] = '\0';
		sceNetInetRecvfrom(socket_desc, buf, 255, 0, (struct sockaddr *)&server, &server_addr_len);
		delay(30);
		if (buf[0] != '\0')
		{
			clearScreen(ColorBlack);
			flipScreen();
			clearScreen(ColorBlack);
			PrintText(150, 100, ColorGreen, "Connection made");
			flipScreen();
			delay(1000);
		}
		else
		{
			PrintText(150, 100, ColorRed, "No connection could be made");
			PrintText(150, 110, ColorWhite, "Restarting in 2 seconds");
			flipScreen();
			delay(1000);
			clearScreen(ColorBlack);
			continue;
		}

		scePowerLock(0); // Forbids user to turn of the device
		// Making sure both buffers of the screen are clear
		clearScreen(ColorBlack);
		flipScreen();
		clearScreen(ColorBlack);
		flipScreen();
		// Beginning loop
		while (running)
		{
			if(GetPowerStatus() > 1)
			{

			}
			scePowerTick(0); // Forbids the screen to turn blank when no BUTTONS are pressed. It still would go blank if you only used the analog stick
			sceCtrlPeekBufferPositive(&pad, 1);
			int cruiseControll = 0;
			int rPressed = 0;
			int onHold = 0;
			running = isRunning();
			pspDebugScreenSetXY(0, 0);
			clearScreen(ColorBlack);
			if (scePowerGetBatteryLifePercent() < 10)
			{
				PrintError("BATTERY TO LOW");
				PrintText(150, 150, ColorRed, "Battery is to low. Shutting down to prevent unexpected shutdown");
				message = "DC:0,0,0:0,0";
				for (;;) {
					if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
					{
						PrintError("Motor stop message failed sending");
						flipScreen();
						delay(1000);
						continue;
					}
					break;
				}
				message = "client:psp:disconnected";
				for (;;) {
					if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
					{
						PrintError("Disconenct message failed sending");
						flipScreen();
						sceDisplayWaitVblankStart();
						delay(50);
						continue;
					}
					break;
				}
				delay(2000);
				sceNetInetClose(socket_desc);
				running = 0;
				break;
			}
			PrintText(10, 262, ColorWhite, "Battery percentage: %d%%   PowerLock: %s", scePowerGetBatteryLifePercent(), powerlock == 0 ? "unlocked" : "locked");
			if (scePowerIsLowBattery()) {
				PrintError("BATTERY IS LOW");
			}
			int aX = pad.Lx - 127;
			int aY = (pad.Ly - 127) * -1 + 1;
			if (aX > 27)		aX -= 28;
			else if (aX < -27)	aX += 27;
			else {
				aX = 0;
				wheelpos1 = 0;
				wheelpos2 = 0;
			}

			if (aY > 27)		aY -= 28;
			else if (aY < -27) 	aY += 27;
			else {
				aY = 0;
			}
			sceCtrlPeekBufferPositive(&pad, 1);
			if (pad.Buttons != 0)
			{
				if (pad.Buttons & PSP_CTRL_START) {
					message = "DC:0,0,0:0,0";
					for (;;) {
						if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
						{
							PrintError("Send failed");
							flipScreen();
							sceDisplayWaitVblankStart();
							delay(50);
							continue;
						}
						break;
					}
					if (QuitScreen()) {
						message = "client:psp:disconnected";
						for (;;) {
							if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
							{
								PrintError("Send failed");
								flipScreen();
								sceDisplayWaitVblankStart();
								delay(50);
								continue;
							}
							break;
						}
						sceNetInetClose(socket_desc);
						running = 0;
						break;
					}
				}
				if (pad.Buttons & PSP_CTRL_SELECT && pad.Buttons != oldButton)
				{
					PrintText(10, 10, ColorBlue, "Help page");
					PrintText(10, 40, ColorWhite, "Press O to reset the 10 DOF chip");
					PrintText(10, 50, ColorWhite, "Press /\\ to reset the socket");
					PrintText(10, 60, ColorWhite, "Press [] to change the powerlocking. If its changed from locked to unlocked and");
					PrintText(10, 70, ColorWhite,"the powerswitch was used it shutdowns immediately. WATCH OUT FOR THAT.");
					PrintText(10, 80, ColorWhite, "Press UP to shift the gear up");
					PrintText(10, 90, ColorWhite, "Press DOWN to shift the gear down");
					PrintText(10, 100, ColorWhite, "Press LEFT to change the steering sensitivity down");
					PrintText(10, 110, ColorWhite, "Press RIGHT to change the steering sensitivity up");
					PrintText(10, 130, ColorWhite, "Press L to enable cruise control. This locks speed but doesn't lock steering");
					PrintText(10, 140, ColorWhite, "Press R to controll the camera. This can be combined with the L function. ");
					PrintText(10, 150, ColorWhite, "Moving continues in this mode. WATCH OUT FOR THAT");
					PrintText(10, 160, ColorWhite, "Press SELECT to display help");
					PrintText(10, 170, ColorWhite, "Press START to quit the client");
					PrintText(10, 200, ColorRed, "Press O to close this help screen");
					PrintText(10, 220, ColorWhite, "Press R now to go to Settings page and L to go back to this page");
					flipScreen();
					sceDisplayWaitVblankStart();
					while (1)
					{
						sceCtrlPeekBufferPositive(&pad, 1);
						if (pad.Buttons != 0)
						{
							if (pad.Buttons & PSP_CTRL_CIRCLE) {
								break;
							}
							if(pad.Buttons & PSP_CTRL_RTRIGGER)
							{
								int index = 0;
								while (true)
								{
									sceCtrlPeekBufferPositive(&pad, 1);
									if (pad.Buttons != 0) {
										switch (index) {
											case 0:
												PrintText(10, 10, ColorBlue, "==>");

												if (pad.Buttons & PSP_CTRL_LEFT && !oldButton)
												{
													if (waittime > 40)
														waittime--;
												}
												if (pad.Buttons & PSP_CTRL_RIGHT && !oldButton)
												{
													if (waittime < 100)
														waittime++;
												}
												break;
											}
										if(pad.Buttons & PSP_CTRL_LTRIGGER || pad.Buttons & PSP_CTRL_CIRCLE)
										{
											break;
										}
										if(pad.Buttons & PSP_CTRL_START)
										{
											if (QuitScreen()) {
												message = "client:psp:disconnected";
												for (;;) {
													if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
													{
														PrintError("\nSend failed");
														flipScreen();
														sceDisplayWaitVblankStart();
														delay(50);
														continue;
													}
													break;
												}
												sceNetInetClose(socket_desc);
												netTerm();
												sceKernelExitGame();
												return 0;
											}
										}
										PrintText(30, 10, ColorWhite, "Time between transmitting data     (%i)", waittime);
										PrintText(30, 20, ColorWhite, "You might want to change this if the RC does not respond well");
										PrintToScreen(ColorBlack);
									}
									oldButton = pad.Buttons;
								}
							}
						}
					}
				}
				if (pad.Buttons & PSP_CTRL_UP && pad.Buttons != oldButton) {
					if (gear != 4) {
						gear++;
						mapMax += 25;
						mapMin -= 25;
					}
				}
				if (pad.Buttons & PSP_CTRL_DOWN && pad.Buttons != oldButton) {
					if (gear != 1) {
						gear--;
						mapMax -= 25;
						mapMin += 25;
					}
				}
				if (pad.Buttons & PSP_CTRL_RIGHT && pad.Buttons != oldButton) {
					if (turngear != 4) {
						turngear++;
						turnmapMax += 25;
					}
				}
				if (pad.Buttons & PSP_CTRL_LEFT && pad.Buttons != oldButton) {
					if (turngear != 1) {
						turngear--;
						turnmapMax -= 25;
					}
				}


				if (pad.Buttons & PSP_CTRL_CROSS && pad.Buttons != oldButton) {
					message = "c:shoot";
					sceNetInetSend(socket_desc, message, strlen(message), 0);
					PrintText(60, 230, ColorRed, "PEW PEW PEW");
				}
				if (pad.Buttons & PSP_CTRL_CIRCLE && pad.Buttons != oldButton)
				{
					message = "c:reset10dof";
					sceNetInetSend(socket_desc, message, strlen(message), 0);
				}
				if (pad.Buttons & PSP_CTRL_SQUARE && pad.Buttons != oldButton)
				{
					if (powerlock == 0) {
						if (scePowerLock(0) == 0)
							powerlock = 1;
					}
					else {
						if (scePowerUnlock(0) == 0)
							powerlock = 0;
					}
				}
				if (pad.Buttons & PSP_CTRL_TRIANGLE && pad.Buttons != oldButton) {

				}
				if (pad.Buttons & PSP_CTRL_RTRIGGER)
				{
					rPressed = 1;
				}
				if (pad.Buttons & PSP_CTRL_LTRIGGER)
				{
					cruiseControll = 1;
				}


				if (pad.Buttons & PSP_CTRL_HOLD)
				{
					onHold = 1;
				}
			}
			oldButton = pad.Buttons;


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
				PrintText(10, 20, ColorWhite, "Drive mode:     %s" , "Cruise controlled");
			}
			else {
				if (rPressed)// Servo
				{
					servoX = aX * -1;
					servoY = aY * -1;
					PrintText(10, 20, ColorWhite, "Drive mode:     %s", "Servo moving");
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
					PrintText(10, 20, ColorWhite, "Drive mode:     %s", "Normal");
				}
			}
			if (onHold)
			{
				speed = 0;
				wheelpos1 = 0;
				wheelpos2 = 0;
			}
			PrintText(10, 10, ColorGray, "PSP Data");
			//PrintText(10, 20, ColorBlack, ""); I already printed something here
			PrintText(10, 30, ColorWhite, "Speed gear:     %i", gear);
			PrintText(10, 40, ColorWhite, "Steering gear:  %i", turngear);
			PrintText(10, 50, ColorWhite, "Analog Y:       %d", aY);
			PrintText(10, 60, ColorWhite, "Analog X:       %d", aX);
			PrintText(10, 70, ColorWhite, "PowerStatus:    %i", GetPowerStatus());

			wheelpos1 = map(wheelpos1, 0, 100, 0, turnmapMax);
			wheelpos2 = map(wheelpos2, 0, 100, 0, turnmapMax);
			speed = map(speed, -100, 100, mapMin, mapMax);

			char dcmessage[30];
			snprintf(dcmessage, sizeof(dcmessage), "DC:%i,%i,%i:%i,%i", speed, wheelpos1, wheelpos2, servoX, servoY);
			if (sceNetInetSend(socket_desc, dcmessage, strlen(dcmessage), 0) < 0)
			{
				PrintError("Send failed");
				continue;
			}
			buf[0] = '\0';
			sceNetInetRecvfrom(socket_desc, buf, 255, 0, (struct sockaddr *)&server, &server_addr_len);
			if (buf[0] != '\0') {
				char *p = strtok(buf, ",:");
				int i = 0;
				while (p != NULL)
				{
					parsedbufA[i] = p;
					p = strtok(NULL, ",:");
					i++;
				}
				if(parsedbufA[0] == "AHRS")
				{
					strcpy(temp,"AHRS");
				}
				else if(parsedbufA[0] == "GPS")
				{
					strcpy(temp, "GPS");
				}
				PrintText(10, 80, ColorGray, "New %s Data", temp);
			}
			else
			{
				PrintText(10, 80, ColorGray, "Old %s Data", temp);
			}
			if (temp == "AHRS") {
				PrintText(10, 90, ColorBlue, "Temp:            %s", parsedbufA[1]);
				PrintText(10, 100, ColorBlue, "Altitude:        %s", parsedbufA[2]);
				PrintText(10, 110, ColorBlue, "Roll:            %s", parsedbufA[3]);
				PrintText(10, 120, ColorBlue, "Pitch:           %s", parsedbufA[4]);
				PrintText(10, 130, ColorBlue, "Heading:         %s", parsedbufA[5]);
			}
			delay(10);
			buf[0] = '\0';
			sceNetInetRecvfrom(socket_desc, buf, 255, 0, (struct sockaddr *)&server, &server_addr_len);
			if (buf[0] != '\0') {
				char *p = strtok(buf, ",:");
				int i = 0;
				while (p != NULL)
				{
					parsedbufB[i] = p;
					p = strtok(NULL, ",:");
					i++;
				}
				PrintText(10, 150, ColorGray, "New GPS Data");
			}
			else
			{
				PrintText(10, 150, ColorGray, "Old GPS Data");
			}
			PrintText(10, 160, ColorBlue, "Longitude:       %s", parsedbufB[1]);
			PrintText(10, 170, ColorBlue, "Latitude:        %s", parsedbufB[2]);
			PrintText(10, 180, ColorBlue, "Altitude:        %s", parsedbufB[3]);
			PrintText(10, 190, ColorBlue, "Heading:         %s", parsedbufB[4]);
			flipScreen();
			delay(waittime - 10);
		}
		netTerm();
		sceKernelExitGame();

		return 0;
	}

	netTerm();
	sceKernelExitGame();

	return 0;
}
