/*
This code is for the ESP8266 that is gonna handle the wifi connection for the Arduino Mega
*/

#include <Wire.h>
#include <SoftwareSerial.h>
#include <RemoteDebug.h>
#include <ESP8266mDNS.h>
#include "PinOuts.h"
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include "BattleBotComBytes.h"

#define ESPREADYPIN D3

const char* ssid = "Natsuki-WiFi";
const char* password = "Golden-Darkness";
//const char* ssid = "Sitecom02A1ED";
//const char* password = "X8FZNV3Z393R";
//const char* ssid = "hijlkema2.4";
//const char* password = "Ruudanita.1";

const char* otapassword = "4453c907975672a2a27bcacd1ee850b8";
const char* apssid = "BattleBotNetwork";
const char* mdnsName = "natsuki-esp8266";
int counter = 0;
int noBytes; // Holds the amount of bytes read by either the UDP server or the Serial UART
boolean clientConnected = false;

SoftwareSerial SerialArd = SoftwareSerial(D5,D6);
WiFiUDP server;
IPAddress clientIP;
IPAddress serverIP;
RemoteDebug Debug;
byte DC1motorSpeed = 0;
byte DC2motorSpeed = 0;

byte packetBuffer[30]; //buffer to hold incoming and outgoing packets
String commandType;
String command;
String arguments;
String ReceivedString;

void setupOTA()
{
	Serial.println("Setting up ArduinoOTA");
	// Port defaults to 8266
	// ArduinoOTA.setPort(8266);

	ArduinoOTA.setHostname(mdnsName);

	// ArduinoOTA.setPassword("admin");

	ArduinoOTA.setPasswordHash("4453c907975672a2a27bcacd1ee850b8");

	ArduinoOTA.onStart([]()
		{
			String type;
			if (ArduinoOTA.getCommand() == U_FLASH)
				type = "sketch";
			else // U_SPIFFS
				type = "filesystem";

			// NOTE: if updating SPIFFS this would be the place to unmount SPIFFS using SPIFFS.end()
			Serial.println("Start updating " + type);
		});
	ArduinoOTA.onEnd([]()
		{
			Serial.println("\nEnd");
		});
	ArduinoOTA.onProgress([](unsigned int progress, unsigned int total)
		{
			Serial.printf("Progress: %u%%\r", (progress / (total / 100)));
		});
	ArduinoOTA.onError([](ota_error_t error)
		{
			Serial.printf("Error[%u]: ", error);
			if (error == OTA_AUTH_ERROR) Serial.println("Auth Failed");
			else if (error == OTA_BEGIN_ERROR) Serial.println("Begin Failed");
			else if (error == OTA_CONNECT_ERROR) Serial.println("Connect Failed");
			else if (error == OTA_RECEIVE_ERROR) Serial.println("Receive Failed");
			else if (error == OTA_END_ERROR) Serial.println("End Failed");
		});
	Serial.println("ArduinoOTA started");
}

void WiFiEvent(WiFiEvent_t event)
{
	if(event == WIFI_EVENT_SOFTAPMODE_STADISCONNECTED)
	{
		SerialArd.write(COMCLIENT);
		SerialArd.write(CLIENTDISCONNECT);
		SerialArd.write(MESSAGEEND);
	}
}

void setup()
{
	SerialArd.begin(250000);
	Serial.begin(115200);
	Wire.begin(0x10);
	Serial.println("Serial monitors activated");
	//Serial.setDebugOutput(true);
	Serial.println("Starting");
	// Connect to WiFi network
	Serial.println();
	Serial.println();
	Serial.print("Connecting to ");
	Serial.println(ssid);
	WiFi.persistent(false);
	WiFi.mode(WIFI_OFF); // this is a temporary line, to be removed after SDK update to 1.5.4
	WiFi.mode(WIFI_STA);
	WiFi.setOutputPower(20.5);
	WiFi.hostname(mdnsName);
	WiFi.begin(ssid, password);
	while (WiFi.status() != WL_CONNECTED)
	{
		if (counter == 10)
		{
			Serial.println();
			Serial.println("Can't connect to the network");
			Serial.println("I am making my own hotspot then");
			WiFi.disconnect(true);
			WiFi.mode(WIFI_OFF);
			WiFi.mode(WIFI_AP);
			WiFi.softAP(apssid);
			delay(200);
			serverIP = WiFi.softAPIP();
			break;
		}
		delay(500);
		Serial.print(".");
		counter++;
	}
	Serial.println();
	if (!MDNS.begin(mdnsName))
	{
		Serial.println("Error setting up MDNS responder!");
	}
	else
	{
		Serial.println("No errors setting up mDNS");
	}
	if(!serverIP)
	{
		serverIP = WiFi.localIP();
	}
	Serial.println("mDNS responder started");
	MDNS.addService("telnet", "tcp", 23);
	MDNS.addService("battlebot", "udp", 20010);
	Debug.begin(mdnsName);
	Debug.setResetCmdEnabled(true); // Enable the reset command
	Debug.showTime(true); // To show time
	Debug.showProfiler(true); // To show profiler - time between messages of Debug
	Debug.setSerialEnabled(true);

	Serial.println("WiFi connected");
	setupOTA();
	server.begin(20010);
	Serial.println("Server started");
	Serial.println(serverIP);
	Serial.print("Wifi strength: ");
	Serial.println(WiFi.RSSI());
	Serial.println("Ready for communication");
	Serial.println();
	digitalWrite(ESPREADYPIN, HIGH);
	SerialArd.write((byte)0x00);
	SerialArd.write(COMCLIENT);
	SerialArd.write(CLIENTESPREADY);
	SerialArd.write(MESSAGEEND);
}

void HandleClientCommand(String clienttype, String clientaction)
{
	Debug.println("Recieved command is a client command");
	Debug.print("ClientType: ");
	Debug.println(clienttype);
	Debug.print("ClientAction: ");
	Debug.println(clientaction);
	if (clientaction == "connected")
	{
		clientConnected = true;
		server.beginPacket(server.remoteIP(), server.remotePort());
		server.write(0x70);
		if (!server.endPacket()) Debug.println("ERROR SENDING PACKAGE");
		Debug.print("Senpai noticed me from: ");
		Debug.println(server.remoteIP());
		SerialArd.write((byte)0x00);
		SerialArd.write(COMCLIENT);
		SerialArd.write(CLIENTCONNECT);
		SerialArd.write(MESSAGEEND);
	}
	if (clientaction == "disconnected")
	{
		byte hunderd = 100;
		SerialArd.write((byte)0x00);
		SerialArd.write(COMMOTOR);
		SerialArd.write(hunderd);
		SerialArd.write(hunderd);
		SerialArd.write(MESSAGEEND);

		SerialArd.write((byte)0x00);
		SerialArd.write(COMCLIENT);
		SerialArd.write(CLIENTDISCONNECT);
		SerialArd.write(MESSAGEEND);
	}
	if(clientaction == "paused")
	{
		byte hunderd = 100;
		SerialArd.write(COMMOTOR);
		SerialArd.write(hunderd);
		SerialArd.write(hunderd);
		SerialArd.write(MESSAGEEND);

		SerialArd.write(COMCLIENT);
		SerialArd.write(CLIENTPAUSED);
		SerialArd.write(MESSAGEEND);
	}
	if(clientaction == "continued")
	{
		SerialArd.write((byte)0x00);
		SerialArd.write(COMCLIENT);
		SerialArd.write(CLIENTCONTINUED);
		SerialArd.write(MESSAGEEND);
	}
	if (clienttype == "pc")
	{
		if (clientaction == "connected")
		{
			Debug.println("PC just connected");
		}
		else if (clientaction == "disconnected")
		{
			Debug.println("PC just disconnected");
			clientConnected = false;
		}
	}
	else if (clienttype == "psp")
	{
		if (clientaction == "connected")
		{
			Debug.println("PSP just connected");
		}
		else if (clientaction == "disconnected")
		{
			Debug.println("PSP just disconnected");
			clientConnected = false;
		}
	}
	else
	{
		Debug.println("Non-defined client");
	}
}

void HandleMotorCommand(char* MotorSpeeds, char* ServoSpeeds)
{
	int pan, tilt;
	int leftMotorSpeed, rightMotorSpeed;;
	if (sscanf(MotorSpeeds, "%i,%i", &leftMotorSpeed, &rightMotorSpeed) == 2){
		if(leftMotorSpeed > 200 || rightMotorSpeed > 200 || leftMotorSpeed < 0 || rightMotorSpeed < 0)
		{
			Debug.println("Speeds received are not valid");
			return; 
		}
		Debug.print("m1speed: ");
		Debug.print(leftMotorSpeed);
		Debug.print(" m2speed: ");
		Debug.println(rightMotorSpeed);
		SerialArd.write(COMMOTOR);
		SerialArd.write((byte)leftMotorSpeed);
		SerialArd.write((byte)rightMotorSpeed);
		SerialArd.write(MESSAGEEND);
		delay(3);
	}
	else
	{
		Debug.println("Motorspeed was not valid");
	}
	if (sscanf(ServoSpeeds, "%i,%i", &pan, &tilt)==2){
		if(pan > 180 || tilt > 180)
		{
			Debug.println("Servo positions received are not valid");
			return;
		}
		SerialArd.write(COMSERVO);
		SerialArd.write((byte)pan);
		SerialArd.write((byte)tilt);
		SerialArd.write(MESSAGEEND);
	}
	else
	{
		Debug.println("The servo positions are not valid");
	}
}

void HandleGenericCommand(String command, String param)
{
	if(command == "shoot")
	{
		SerialArd.write(COMGENERIC);
		SerialArd.write(GENSHOOT);
		SerialArd.write(MESSAGEEND);
	}
}

void HandleUDPData(byte Packet[])
{
	delay(0);
	// following 4 lines are needed because of a possible b.ug.
	commandType.remove(0);
	command.remove(0);
	arguments.remove(0);
	ReceivedString.remove(0);

	int colloncounter = 0;
	for (int i = 0; i <= noBytes - 1; i++)
	{
		char receivedChar = char(Packet[i]);
		ReceivedString += receivedChar;

		//      Debug.print(char(packetBuffer[i]));
		//      Debug.print(" colloncounter: ");
		//      Debug.println(colloncounter);
		if (Packet[i] == 58)
		{
			colloncounter++;
			continue;
		}
		if (colloncounter == 0)
		{
			if (receivedChar == '0') continue;
			commandType += receivedChar;
		}
		else if (colloncounter == 1)
		{
			command += receivedChar;
		}
		else
		{
			arguments += receivedChar;
		}
	} // end for
	delay(0);
	commandType.toLowerCase();
	char command1[20];
	char arguments1[20];
	command.toCharArray(command1, sizeof(command));
	arguments.toCharArray(arguments1, sizeof(arguments));
	Debug.print("ReceivedString: ");
	Debug.println(ReceivedString);
	if (Debug.ative(Debug.VERBOSE)) {
		Debug.print("Commandtype: ");
		Debug.println(commandType);
		Debug.print("Command: ");
		Debug.println(command);
		Debug.print(" Arguments: ");
		Debug.println(arguments);
	}
	delay(0);
	if (commandType == STRCLIENT)	HandleClientCommand(command, arguments);
	if (commandType == STRMOTOR)	HandleMotorCommand(command1, arguments1);
	if (commandType == STRGENERAL)	HandleGenericCommand(command1, arguments1);
}

void HandleArduinoSerialData()
{
	byte c = SerialArd.read();
	if(c == ESPMOTORDIAG)
	{
		byte m = SerialArd.read();
		byte p = SerialArd.read();
		if (SerialArd.read() == MESSAGEEND) {
			if (p == CURRENTWARNING)
			{
				Debug.print("The current in motor ");
				Debug.print(m);
				Debug.println(" is to high");
			}
			if (p == MOTORFAULT)
			{
				Debug.print("A fault occured in motor ");
				Debug.println(m);
			}
		}
	}
	else if(c == ESPGEN)
	{
		byte what = SerialArd.read();
		if(what == ESPGENREADYACK)
		{
			digitalWrite(ESPREADYPIN, LOW);
		}
	}
}

void loop()
{
	delay(0);
	ArduinoOTA.handle();
	Debug.handle();
	MDNS.update();
	delay(0);
	noBytes = server.parsePacket();
	if (noBytes)
	{
		if (Debug.ative(Debug.VERBOSE)) {
			Debug.print(millis() / 1000);
			Debug.print(":Packet of ");
			Debug.print(noBytes);
			Debug.print(" received from ");
			Debug.print(server.remoteIP());
			Debug.print(":");
			Debug.println(server.remotePort());
		}
		delay(0);
		server.read(packetBuffer, noBytes); // read the packet into the buffer

		HandleUDPData(packetBuffer);
	} // end if
	if(Serial.available())
	{
		delay(0);
		noBytes = Serial.readBytesUntil('\r\n', packetBuffer, 30);
		HandleUDPData(packetBuffer);
	}
	delay(0);
}
