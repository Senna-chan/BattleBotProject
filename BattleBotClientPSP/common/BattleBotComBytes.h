#ifndef COMBYTES_H
#define COMBYTES_H

// ESP > Arduino message data
#define COMMOTOR			0xE0
#define COMSERVO			0xE1
#define COMCLIENT			0xE2
#define COMGENERIC			0xE3

// Generic message types
#define GENSHOOT			0x01
#define GENRESETDOF			0x02

// ClientTypes
#define CLIENTPC			0x01
#define CLIENTPSP			0x02
#define CLIENTAPP			0x03

// ESP Client message types
#define CLIENTESPREADY		0x01
#define CLIENTCONNECT 		0x02
#define CLIENTDISCONNECT	0x03
#define CLIENTPAUSED		0x04
#define CLIENTCONTINUED		0x05

// Arduino > ESP message data
#define ESPMOTORSTAT		0xF0
#define ESPMOTORDIAG		0xF1
#define ESPDOFSTAT			0xF2
#define ESPGPSSTAT			0xF3
#define ESPGEN				0xF4

// Arduino > ESP gen types
#define ESPGENREADYACK		0x01

// Motor diagagnostic data 
#define MOTOR1				0x01
#define MOTOR2				0x02
#define CURRENTWARNING		0x01
#define MOTORFAULT			0x02

#define MESSAGEEND			0xFF

// String Data
const char *STRGENERAL	=	"gen";
const char *STRMOTOR	=	"dc";
const char *STRLIPO		=	"lipo";
const char *STRCLIENT	=	"client";
const char *STRERROR	=	"error";
const char *STRAHRS		=	"ahrs";
const char *STRDEBUG	=	"debug";

#endif