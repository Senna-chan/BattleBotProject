#include <ESP8266mDNS.h>
#include "PinOuts.h"
#include "MotorHelper.h"
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include <Wire.h>
#include <stdio.h>
//const char* ssid = "Zycon";
//const char* password = "cdfc3d9152";
const char* ssid = "Natsuki-WiFi";
const char* password = "Golden-Darkness";
const char* apssid = "BattleBotNetwork";
int counter = 0;

char* commandtype = "";
char* command = "";
char* arguments = "";
char* received_command = "";
boolean clientConnected = false;

MotorHelperClass DC1Helper = MotorHelperClass(D1, D2, D3);
MotorHelperClass DC2Helper = MotorHelperClass(D6, D7, D8);

byte packetBuffer[512]; //buffer to hold incoming and outgoing packets
// Create an instance of the server
// specify the port to listen on as an argument
WiFiUDP server;
IPAddress clientIP;
IPAddress serverIP;

void setupOTA()
{
  Serial.println("Setting up Android OTA");
  // Port defaults to 8266
  ArduinoOTA.setPort(8266);

  // Hostname defaults to esp8266-[ChipID]
  ArduinoOTA.setHostname("Natsuki-esp8266");

  // No authentication by default
  ArduinoOTA.setPassword((const char *)"123");

  ArduinoOTA.onStart([]() {
    Serial.println("Start");
  });
  ArduinoOTA.onEnd([]() {
    Serial.println("\nEnd");
  });
  ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
    Serial.printf("Progress: %u%%\r", (progress / (total / 100)));
  });
  ArduinoOTA.onError([](ota_error_t error) {
    Serial.printf("Error[%u]: ", error);
    if (error == OTA_AUTH_ERROR) Serial.println("Auth Failed");
    else if (error == OTA_BEGIN_ERROR) Serial.println("Begin Failed");
    else if (error == OTA_CONNECT_ERROR) Serial.println("Connect Failed");
    else if (error == OTA_RECEIVE_ERROR) Serial.println("Receive Failed");
    else if (error == OTA_END_ERROR) Serial.println("End Failed");
  });
  ArduinoOTA.begin();
  Serial.println("AndroidOTA started");
}

void StopMotors()
{
	DC1Helper.Stop();
	DC2Helper.Stop();
}

void WiFiEvent(WiFiEvent_t event) {
	Serial.printf("[WiFi-event] event: %d\n", event);
}

void setup()
{
	Serial.begin(115200);
	Serial.println("Starting");
	delay(10);
	Wire.pins(D4, D5);
	analogWriteFreq(20000);
	analogWriteRange(1024);
	// Connect to WiFi network
	Serial.println();
	Serial.println();
	Serial.print("Connecting to ");
	Serial.println(ssid);
	WiFi.onEvent(WiFiEvent);
	WiFi.mode(WIFI_STA);
	WiFi.begin(ssid, password);
	serverIP = WiFi.localIP();
	while (WiFi.status() != WL_CONNECTED)
	{
		if (counter == 10)
		{
			Serial.println();
			Serial.println("Can't connect to the network");
			Serial.println("I am making my own hotspot then");
			WiFi.disconnect(1);
			WiFi.mode(WIFI_AP);
			WiFi.softAP(apssid);
			delay(500);
			serverIP = WiFi.softAPIP();
			break;
		}
		delay(500);
		Serial.print(".");
		counter++;
	}
	if(!serverIP)
	{
		serverIP = WiFi.localIP();
	}
	Serial.println("");
	Serial.println("WiFi connected");
	setupOTA();
	// Start the server
	server.begin(20010);
	Serial.println("Server started");

	// Serial.println(the IP address
	Serial.println(serverIP);
}



void HandleClientCommand(char* clienttype, char* clientaction)
{
	if (clientaction == "connected") {
		clientConnected = true;
		server.beginPacket(server.remoteIP(), server.remotePort());
		server.println("YouConnected");
		server.endPacket();
		Serial.print("Senpai noticed me from: ");
		Serial.println(server.remoteIP());
	}
	if (clientaction == "disconnected") {
		StopMotors();
	}
	if (clienttype == "pc") {
		if (clientaction == "connected") {
			Serial.println("PC just connected");
		}
		else if(clientaction == "disconnected") {
			Serial.println("PC just disconnected");
			clientConnected = false;
		}
	}
	if (clienttype == "psp") {
		if (clientaction == "connected") {
			Serial.println("PSP just connected");
		}
		else if (clientaction == "disconnected") {
			Serial.println("PSP just disconnected");
			clientConnected = false;
		}
	}
}
void HandleMotorCommand(char* MotorSpeeds, char* ServoSpeeds)
{
	int speed, wheelpos1, wheelpos2;
	int pan, tilt;
	if (sscanf(MotorSpeeds, "%i,%i,%i", &speed, &wheelpos1, &wheelpos2) == 3) {
		int DC1motorSpeed = 0;
		int DC2motorSpeed = 0;
		if (speed > 0) {
			DC1motorSpeed = speed - wheelpos1;
			DC2motorSpeed = speed - wheelpos2;
		}
		else if (speed < 0) {
			DC1motorSpeed = speed + wheelpos1;
			DC2motorSpeed = speed + wheelpos2;
		}
		if (speed == 0) {
			if (wheelpos1 > 0) {
				DC1motorSpeed = wheelpos1 * -1;
				DC2motorSpeed = wheelpos1;
			}
			if (wheelpos2 > 0) {
				DC1motorSpeed = wheelpos2;
				DC2motorSpeed = wheelpos2 * -1;
			}
		}
	

		if (DC1motorSpeed < 0)  DC1Helper.Backward(DC1motorSpeed);
		if (DC1motorSpeed > 0)  DC1Helper.Forward(DC1motorSpeed);
		if (DC1motorSpeed == 0) DC1Helper.Stop();
		if (DC2motorSpeed < 0)  DC2Helper.Backward(DC2motorSpeed);
		if (DC2motorSpeed > 0)  DC2Helper.Forward(DC2motorSpeed);
		if (DC2motorSpeed == 0) DC2Helper.Stop();
	}
	else
	{
		Serial.println("Motorspeed was not valid");
		return;
	}
	if (sscanf(ServoSpeeds, "%i,%i", &pan, &tilt) == 2) {

	}
	else
	{
		Serial.println("The servo positions are not valid");
		return;
	}
}


void loop()
{
	ArduinoOTA.handle(); // OTA. Cuz its awesome?
	int noBytes = server.parsePacket();

	if (noBytes)
	{
		Serial.print(millis() / 1000);
		Serial.print(":Packet of ");
		Serial.print(noBytes);
		Serial.print(" received from ");
		Serial.print(server.remoteIP());
		Serial.print(":");
		Serial.println(server.remotePort());
		server.read(packetBuffer, noBytes); // read the packet into the buffer

		int colloncounter = 0;
		for (int i = 0; i <= noBytes; i++)
		{
			char recieved_char = char(packetBuffer[i]);
			received_command = received_command + recieved_char;
			Serial.print("Char: ");
			Serial.print(recieved_char);
			Serial.print(" colloncounter: ");
			Serial.println(colloncounter);
			if(packetBuffer[i] == 58)
			{
				colloncounter++;
				continue;
			}
			if (colloncounter == 0)
			{
				commandtype = commandtype + recieved_char;
			}
			else if (colloncounter == 1)
			{
				command = command + recieved_char;
			}
			else {
				arguments = arguments + recieved_char;
			}
		} // end for

		Serial.print("Recieved packet: ");
		Serial.println(received_command);
		Serial.print("Commandtype: ");
		Serial.print(commandtype);
		Serial.print(", command: ");
		Serial.print(command);
		Serial.print(", Arguments: ");
		Serial.println(arguments);
		if (commandtype == "client") HandleClientCommand(command, arguments);
		if (commandtype == "dc") HandleMotorCommand(command, arguments);
		commandtype = "";
		command = "";
		arguments = "";
		received_command = "";
	} // end if
}
