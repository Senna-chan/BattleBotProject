#include <ESP8266WiFi.h>


const char* ssid = "BattleBot-WiFi";
const char* password = "Golden-Darkness";


void WiFiEvent(WiFiEvent_t event) {
	Serial.printf("[WiFi-event] event: %d\n", event);

	switch (event) {
	case WIFI_EVENT_STAMODE_GOT_IP:
		Serial.println("WiFi connected");
		Serial.println("IP address: ");
		Serial.println(WiFi.localIP());
		break;
	case WIFI_EVENT_STAMODE_DISCONNECTED:
		Serial.println("WiFi lost connection");
		break;
	}
}
void setup() {
	Serial.begin(115200);

	// delete old config
	WiFi.disconnect(true);

	delay(1000);

	WiFi.onEvent(WiFiEvent);

	WiFi.begin(ssid, password);

	Serial.println();
	Serial.println();
	Serial.println("Wait for WiFi... ");
	analogWriteFreq(25000);
}

void loop()
{

  /* add main program code here */

}
