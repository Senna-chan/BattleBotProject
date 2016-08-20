import subprocess, commands, socket, fcntl, struct, atexit, threading
from Adafruit_MotorHAT import Adafruit_MotorHAT, Adafruit_DCMotor
import i2clcd

def get_ip_address(ifname):
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    return socket.inet_ntoa(fcntl.ioctl(
        s.fileno(),
        0x8915,  # SIOCGIFADDR
        struct.pack('256s', ifname[:15])
    )[20:24])

LCD_LINE_1 = 0x80 # LCD RAM address for the 1st line
LCD_LINE_2 = 0xC0 # LCD RAM address for the 2nd line
LCD_LINE_3 = 0x94 # LCD RAM address for the 3rd line
LCD_LINE_4 = 0xD4 # LCD RAM address for the 4th line
IP = get_ip_address("wlan0")

SSID = "Not Connected"


def turnOffMotors():
    mh.getMotor(1).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(2).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(3).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(4).run(Adafruit_MotorHAT.RELEASE)


def checkWiFiState():
    oldSSID = SSID
    IP = get_ip_address("wlan0")
    wifiStats = subprocess.Popen('iwconfig', stdout=subprocess.PIPE).stdout.read()
    SSID = wifiStats[wifiStats.find("ESSID:")+7:wifiStats.find("Nickname:")-3];
    if(SSID != oldSSID):
        lcd.lcd_toggle_backlight(1)
        lcd.lcd_string(IP, LCD_LINE_1)
        lcd.lcd_string("AP: "+SSID,LCD_LINE_2)
    linkQuality = wifiStats[wifiStats.find("Signal level=")+13:wifiStats.find("Noise level=")-5]
    print linkQuality
    linkQuality = linkQuality[0:3]
    print linkQuality
    if(linkQuality < 50):
        turnOffMotors()

def Main(gpio, mh, lcd):
    gpio = gpio
    mh = mh
    lcd = lcd
    check_counter = 0
    checkWiFiState()
    while True:
        if(check_counter == 500):
            checkWiFiState()
            check_counter = 0

        check_counter+=1
        sleep(0.001)
    #end while