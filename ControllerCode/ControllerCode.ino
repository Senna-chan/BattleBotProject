int speed, wheelpos = 0, pan, tilt;
int TRIGGER_PIN = 8;
int LASER_PIN = 9;
int buttonState = 0, lastButtonState = 0;
int on_off_pin = 2;
int on_off;
int loopcount = 0;
#define SSID "Natsuki-WiFi"
#define PASS "Golden-Darkness"

void setup() {
	pinMode(on_off_pin, INPUT_PULLUP);
	pinMode(TRIGGER_PIN, INPUT);
	pinMode(LASER_PIN, OUTPUT);
	Serial.begin(9600);
}

void loop() {
	//on_off = digitalRead(on_off_pin);
	//Serial.println(on_off);
	//if (on_off == HIGH) { // Ima simulating mah own on off switch with code
		speed = analogRead(A5);
		speed = map(speed, 0, 1023, -100, 100);
		wheelpos = analogRead(A0);
		wheelpos = map(wheelpos, 0, 1023, -100, 100);
		pan = analogRead(A1);
		pan = map(pan, 0, 1023, -100, 100);
		tilt = analogRead(A2);
		tilt = map(tilt, 0, 1023, -100, 100);

		loopcount++;
		if (loopcount == 50) {
			String str = "MC:";
			str += speed;
			str += ",";
			str += wheelpos;
			str += ",500=";
			str += "SC:";
			str += pan;
			str += ",";
			str += tilt;
			str += "!";
			Serial.println(str);
			loopcount = 0;
		}
		buttonState = digitalRead(TRIGGER_PIN);
		if (buttonState != lastButtonState) {
			if (buttonState == LOW) {
				Serial.println("BOOM! HEADSHOT");
			}
		}
		lastButtonState = buttonState;
	//}
	
}

//boolean connectWiFi()
//{
//	Serial.println("AT+CWMODE=1");
//	String cmd = "AT+CWJAP=\"";
//	cmd += SSID;
//	cmd += "\",\"";
//	cmd += PASS;
//	cmd += "\"";
//	dbgSerial.println(cmd);
//	Serial.println(cmd);
//	delay(2000);
//	if (Serial.find("OK"))
//	{
//		dbgSerial.println("OK, Connected to WiFi.");
//		return true;
//	}
//	else
//	{
//		dbgSerial.println("Can not connect to the WiFi.");
//		return false;
//	}
//}