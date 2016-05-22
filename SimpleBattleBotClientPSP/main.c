/*
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
*/

#include <pspkernel.h>
#include <pspdisplay.h>
#include <string.h>
#include <math.h>
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
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <sys/unistd.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <netinet/in.h>
#include <psputility.h>
#include "common/callback.h"

PSP_MODULE_INFO("Simple Battlebot Client PSP", 0, 1, 1);

#define printf pspDebugScreenPrintf


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
	printf("Netdialog 1st line");
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

int main(int argc, char *argv[])
{
	setupExitCallback();

	sceUtilityLoadNetModule(PSP_NET_MODULE_COMMON);

	sceUtilityLoadNetModule(PSP_NET_MODULE_INET);

	setupGu();
	netInit();
	netDialog();
	const char *ip;
	int ipToChoose;
	pspDebugScreenInit();
	pspDebugScreenSetXY(0, 0);
	printf("Select /\\ for 192.168.43.251\n");
	printf("Select o for 192.168.1.132\n");
	printf("Select x for 192.168.1.101\n");
	sceDisplayWaitVblankStart();
	SceCtrlData pad;

	sceCtrlSetSamplingCycle(0);
	sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);
	while (1) {
		sceCtrlPeekBufferPositive(&pad, 1);

		if (pad.Buttons != 0)
		{
			if (pad.Buttons & PSP_CTRL_CROSS) {
				ipToChoose = 1;
				//ip = "192.168.1.101";
				break;
			}
			if (pad.Buttons & PSP_CTRL_CIRCLE) {
				ipToChoose = 2;
				//ip = "192.168.1.132";
				break;
			}
			if (pad.Buttons & PSP_CTRL_TRIANGLE) {
				ipToChoose = 3;
				//ip = "192.168.43.251";
				break;
			}
			else {
				printf("Nope\n");
			}
		}
	}
	switch (ipToChoose)
	{
	case 1:
		ip = "192.168.1.101";
		break;
	case 2:
		ip = "192.168.1.132";
		break;
	case 3:
		ip = "192.168.43.251";
		break;
	default:
		break;
	}
	printf(ip);
	delay(1000);
	pspDebugScreenClear();
	int socket_desc;
	struct sockaddr_in server;
	char *message;
	pspDebugScreenSetXY(0, 0);
	socket_desc = sceNetInetSocket(AF_INET, SOCK_STREAM, 0);
	if (socket_desc == -1)
	{
		printf("Could not create socket\n");
		printf("socket returned $%x\n", socket_desc);
		printf("errno=$%x\n", sceNetInetGetErrno());
		sceDisplayWaitVblankStart();
		printf("Shutting down because of error.");
		delay(1000);
		netTerm();
		sceKernelExitGame();
	}else{
		printf("Socket Made\n");
		sceDisplayWaitVblankStart();
	}
	
	server.sin_addr.s_addr = inet_addr(ip);
	server.sin_family = 2;
	server.sin_port = htons(20010);

	//Connect to remote server
	if (sceNetInetConnect(socket_desc, (struct sockaddr *)&server, sizeof(server)) < 0)
	{
		printf("Error connecting to socket\n");
		printf("errno=$%x\n", sceNetInetGetErrno());
		sceDisplayWaitVblankStart();
		printf("Shutting down because of error.");
		delay(1000);
		netTerm();
		sceKernelExitGame();
	}else{
		printf("Connected\n");
		sceDisplayWaitVblankStart();
	}
	//Send some data
	message = "client:psp:connected";
	if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
	{
		printf("Send failed\n");
		sceDisplayWaitVblankStart();
		sceKernelDelayThread(1000);
	}else{
		printf("Data Send\n");
		sceDisplayWaitVblankStart();
	}
	//sceNetInetClose(socket_desc);
	sceKernelDelayThread(1000000);
	int speed = 0;
	int wheelpos1 = 0;
	int wheelpos2 = 0;
	int servoY = 0;
	int servoX = 0;
	int ButtonTriangle = 0;
	while (running)
	{
		running = isRunning();
		int cruiseControll = 0;
		int rPressed = 0;
		pspDebugScreenClear();
		pspDebugScreenSetXY(0, 0);
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

		printf("Analog X = %d ", aX);
		printf("Analog Y = %d \n", aY);

		sceCtrlPeekBufferPositive(&pad, 1);

		if (pad.Buttons != 0)
		{
			if (pad.Buttons & PSP_CTRL_START) {
				printf("Start");
				message = "client:disconnected:psp";
				//socket_desc = sceNetInetSocket(1, 2, 0);
				//sceNetInetConnect(socket_desc, (struct sockaddr *)&server, sizeof(server));
				if (sceNetInetSend(socket_desc, message, strlen(message), 0) < 0)
				{
					printf("\nSend failed");
					sceDisplayWaitVblankStart();
				}
				sceNetInetClose(socket_desc);
				running = 0;
				break;
			}
			if (pad.Buttons & PSP_CTRL_SELECT)		printf("Select");
			if (pad.Buttons & PSP_CTRL_UP)			printf("Up");
			if (pad.Buttons & PSP_CTRL_DOWN)		printf("Down");
			if (pad.Buttons & PSP_CTRL_RIGHT)		printf("Right");
			if (pad.Buttons & PSP_CTRL_LEFT)		printf("Left");

			if (pad.Buttons & PSP_CTRL_CROSS)		printf("Cross");
			if (pad.Buttons & PSP_CTRL_CIRCLE)		printf("Circle");
			if (pad.Buttons & PSP_CTRL_SQUARE)		printf("Square");
			if (pad.Buttons & PSP_CTRL_TRIANGLE) {
				if (!ButtonTriangle) {
					ButtonTriangle = 1;
					printf("Triangle");

					message = "client:psp:disconnected";
					sceNetInetSend(socket_desc, message, strlen(message), 0);
					sceNetInetClose(socket_desc);
					printf("Socket closed");
					socket_desc = sceNetInetSocket(AF_INET, SOCK_STREAM, 0);
					if (sceNetInetConnect(socket_desc, (struct sockaddr *)&server, sizeof(server)) < 0)
					{
						printf("Error connecting to socket\n");
						printf("errno=$%x\n", sceNetInetGetErrno());
						printf("Exiting client");
						delay(1000);
						running = 0;
						break;
					}
					else {
						printf("Connected\n");
						sceDisplayWaitVblankStart();
					}
					message = "client:psp:connected";
					sceNetInetSend(socket_desc, message, strlen(message), 0);
				}
			}
			else ButtonTriangle = 0;

			if (pad.Buttons & PSP_CTRL_RTRIGGER)
			{
				rPressed = 1;
			}
			if (pad.Buttons & PSP_CTRL_LTRIGGER)
			{
				cruiseControll = 1;
			}
		}
		else {
			ButtonTriangle = 0;
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
		snprintf(dcmessage, sizeof(dcmessage), "DC:%i,%i,%i:%i,%i", speed, wheelpos1, wheelpos2, servoX, servoY);
		printf(dcmessage);
		//socket_desc = sceNetInetSocket(1, 2, 0);
		//sceNetInetConnect(socket_desc, (struct sockaddr *)&server, sizeof(server));
		if (sceNetInetSend(socket_desc, dcmessage, strlen(dcmessage), 0) < 0)
		{
			printf("\nSend failed");
			sceDisplayWaitVblankStart();
		}
		delay(30);
		//sceNetInetClose(socket_desc);
		sceDisplayWaitVblank();
		sceGuSwapBuffers();
	}
	netTerm();
	sceKernelExitGame();

	return 0;
}