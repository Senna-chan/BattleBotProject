void setup() {
	Serial.begin(115200);
	Serial.print("AT+CWMODE=1\r\n");
	Serial.print("AT+CWJAP=\"Raspberry-Led\",\"Raspberry-LED\"\r\n");
	Serial.print("AT+CIPSTART=\"TCP\",\"192.168.1.103\",20010\r\n");
}
int i = 0;
int n = 0;
int m = 0;
void loop() {
	i = random(-100, 100);
	n = random(-100, 100);
	m = random(-100, 100);
	String dcmessage = "DC:";
	dcmessage += i;
	dcmessage += ",";
	dcmessage += n;
	dcmessage += ",";
	dcmessage += m;
	dcmessage += "|";
	dcmessage += n;
	dcmessage += ",";
	dcmessage += m;
	dcmessage += "\r\n";

	String s1 = "AT+CIPSEND=";
	s1 += dcmessage.length();
	s1 += "\r\n";
	Serial.print(s1);

	String s2 = dcmessage;
	s2 += "\r\n";
	Serial.print(s2);
}