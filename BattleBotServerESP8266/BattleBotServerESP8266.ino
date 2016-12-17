/*
This code is for the ESP8266 that is gonna handle the wifi connection for the Arduino Mega
*/

#include <SoftwareSerial.h>
#include <RemoteDebug.h>
#include <ESP8266mDNS.h>
#include "PinOuts.h"
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include "ESPCOMBYTES.h"
#define true  1
#define false 0

#define ESPRESETPIN D3
//const char* ssid = "Zycon";
//const char* password = "cdfc3d9152";
const char* ssid = "Natsuki-WiFi";
const char* password = "Golden-Darkness";
//const char* ssid = "Sitecom02A1ED";
//const char* password = "X8FZNV3Z393R";

const char* otapassword = "4453c907975672a2a27bcacd1ee850b8";
const char* apssid = "BattleBotNetwork";
const char* mdnsName = "natsuki-esp8266";
int counter = 0;
boolean clientConnected = false;

SoftwareSerial SerialArd = SoftwareSerial(D7, D8);
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
		SerialArd.write(ESPCLIENT);
		SerialArd.write(0x03);
		SerialArd.write(ESPMESSAGEEND);
	}
}

void setup()
{
	Serial.begin(115200);
	SerialArd.begin(115200);
	//Serial.setDebugOutput(true);
	Serial.println("Starting");
	// Connect to WiFi network
	Serial.println();
	Serial.println();
	Serial.print("Connecting to ");
	Serial.println(ssid);
	pinMode(ESPRESETPIN, INPUT);
	WiFi.onEvent(WiFiEvent);
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
	digitalWrite(D1, HIGH);
	SerialArd.write(ESPCLIENT);
	SerialArd.write(0x01);
	SerialArd.write(ESPMESSAGEEND);
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
		server.println("YouConnected");
		if (!server.endPacket()) Debug.println("ERROR SENDING PACKAGE");
		Debug.print("Senpai noticed me from: ");
		Debug.println(server.remoteIP());
		SerialArd.write(ESPCLIENT);
		SerialArd.write(0x02);
		SerialArd.write(ESPMESSAGEEND);
	}
	if (clientaction == "disconnected")
	{
		SerialArd.write(ESPMOTOR);
		SerialArd.write(100);
		SerialArd.write(100);
		SerialArd.write(ESPMESSAGEEND);

		SerialArd.write(ESPCLIENT);
		SerialArd.write(0x03);
		SerialArd.write(ESPMESSAGEEND);
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
	Debug.println("Recieved command is a motor command");
	int leftMotorSpeed, rightMotorSpeed, pan, tilt;
	Debug.print("Wifi Strength: ");
	Debug.println(WiFi.RSSI());
	Debug.println();
	if (sscanf(MotorSpeeds, "%d,%d", &leftMotorSpeed, &rightMotorSpeed) == 2){
		if(leftMotorSpeed > 200 || rightMotorSpeed > 200 || leftMotorSpeed < 0 || rightMotorSpeed < 0)
		{
			Debug.println("Speeds received are not valid");
			return; 
		}
		Debug.print("m1speed: ");
		Debug.print(leftMotorSpeed);
		Debug.print(" m2speed: ");
		Debug.println(rightMotorSpeed);
		SerialArd.write(ESPMOTOR);
		SerialArd.write(leftMotorSpeed);
		SerialArd.write(rightMotorSpeed);
		SerialArd.write(ESPMESSAGEEND);
	}
	else
	{
		Debug.println("Motorspeed was not valid");
		return;
	}
	if (sscanf(ServoSpeeds, "%d,%d", &pan, &tilt)==2){
		if(pan > 100 || tilt > 100 || pan < -100 || tilt < -100)
		{
			Debug.println("Servo positions received are not valid");
			return;
		}
		SerialArd.write(0xf1);
		SerialArd.write(pan + 100);
		SerialArd.write(tilt + 100);
		SerialArd.write(0xff);
	}
	else
	{
		Debug.println("The servo positions are not valid");
	}
}

void HandleUDPData(int noBytes)
{
	delay(0);
	Debug.print(millis() / 1000);
	Debug.print(":Packet of ");
	Debug.print(noBytes);
	Debug.print(" received from ");
	Debug.print(server.remoteIP());
	Debug.print(":");
	Debug.println(server.remotePort());
	delay(0);
	server.read(packetBuffer, noBytes); // read the packet into the buffer

										// following 4 lines are needed because of a possible bu.g
	commandType.remove(0);
	command.remove(0);
	arguments.remove(0);
	ReceivedString.remove(0);

	int colloncounter = 0;
	for (int i = 0; i <= noBytes - 1; i++)
	{
		char receivedChar = char(packetBuffer[i]);
		ReceivedString += receivedChar;

		//      Debug.print(char(packetBuffer[i]));
		//      Debug.print(" colloncounter: ");
		//      Debug.println(colloncounter);
		if (packetBuffer[i] == 58)
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
	Debug.print("Commandtype: ");
	Debug.println(commandType);
	Debug.print("Command: ");
	Debug.println(command);
	Debug.print(" Arguments: ");
	Debug.println(arguments);
	delay(0);
	if (commandType == "client") HandleClientCommand(command, arguments);
	if (commandType == "dc") HandleMotorCommand(command1, arguments1);
}

void HandleArduinoSerialData()
{
	byte c = SerialArd.read();
	if(c == ESPMOTORDIAG)
	{
		byte m = SerialArd.read();
		byte p = SerialArd.read();
		if (SerialArd.read() == ESPMESSAGEEND) {
			if (p == 0x01)
			{
				Debug.print("The current in motor ");
				Debug.print(m);
				Debug.println(" is to high");
			}
			if (p == 0x02)
			{
				Debug.print("A fault occured in motor ");
				Debug.println(m);
			}
		}
	}
}

void loop()
{
//	if(digitalRead(ESPRESETPIN) == HIGH)
//	{
//		Debug.println("Received Reset signal, Rebooting");
//		ESP.restart();
//	}
	delay(0);
	ArduinoOTA.handle();
	Debug.handle();
	MDNS.update();
	delay(0);
	int noBytes = server.parsePacket();
	if (noBytes)
	{
		HandleUDPData(noBytes);
	} // end if
	if(Serial.available()) // Forward everything to the client
	{
		server.beginPacket(server.remoteIP(), server.remotePort());
		while (Serial.available()) {
			server.println(Serial.readStringUntil('\r\n'));
		}
		server.endPacket();
	}
	if(SerialArd.available())
	{
		HandleArduinoSerialData();
	}
	delay(0);
}
