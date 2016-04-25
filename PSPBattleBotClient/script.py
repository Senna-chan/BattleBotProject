import socket, sys, os, math
from time import sleep
import psp2d, pspnet, pspos
execfile(".\\danzeff\\danzeff.py")

screen = psp2d.Screen()
font = psp2d.Font('font.png')
black = psp2d.Color(0,0,0);
white = psp2d.Color(255,255,255);
client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
val = "";
ip = "";
analogX = 0
analogY = 0
def keyBoard():
    global screen, bg, font, white, val

    danzeff_load()						
    val = ''
    # ptDanzeff PyOSK: Returns whether OSK is initialised or not as a boolean
    if (not danzeff_isinitialized()):	
        return

    # ptDanzeff PyOSK: Position OSK on screen
    danzeff_moveTo(220,20)
    screen.clear(black)
    screen.swap()
    while (True):

        # ptDanzeff PyOSK: Render OSK on screen		
        danzeff_render() 

        # ptDanzeff PyOSK: Returns OSK input as an integer
        cha = danzeff_readInput(psp2d.Controller())

        # Now Evaluate return value and take proper action
        if (cha != 0 and cha != 1 and cha != 2):
            if (cha == 8):
                val = val[0:len(val)-1]
                screen.clear(black)
                screen.swap()
            elif (cha == 13):
                val = ""
            elif (cha == 4):
                return
            else:
                val = val + chr(cha)

        # Print current input string
        font.drawText(screen, 5, 230, val)	

        # Refresh screen	
        screen.swap()

    danzeff_free()
    screen.clear(black)

def connectToSocket():
    global client_socket, val;
    #keyBoard()
    #ip = val
    ip = "192.168.0.121"
    font.drawText(screen, 0, 0, val)
    screen.swap()
    try:
        client_socket.connect((ip, 20010))
    except:
        screen.clear(black);
        font.drawText(screen, 0, 0, "Press X to try to reconnect")
        font.drawText(screen, 0, 25, "Press O to close the programm")
        screen.swap();
        while True:
            pad = psp2d.Controller()
            if pad.cross:
                screen.clear(black);
                screen.swap();
                connectToSocket()
            if pad.circle:
                pass 
    test = 0
    client_socket.sendall("client:psp", test)
    font.drawText(screen, 0, 0, str(test))
    screen.swap()
    sleep(1)
    controll()

    
def controll():
    global client_socket, ip, analogX, analogY
    cruisecontroll = False
    oldpad = psp2d.Controller()
    while True:
        pad = psp2d.Controller()
        if pad.start:
            break
        #if(pad.cross):
            #client_socket.sendall("PEWPEW");
        if(not pad.l and oldpad.l):
            cruisecontroll = not cruisecontroll


        analogX = int(round(pad.analogX / 1.27))
        analogY = int(round(pad.analogY / 1.27)) * -1
        if(analogX > 100):
            analogX = 100
        if(analogY > 100):
            analogY = 100
        if(analogX < -100):
            analogX = -100
        if(analogY < -100):
            analogY = -100
        screen.clear(black)
        font.drawText(screen, 0, 0, "X sideways:"+str(analogX));
        font.drawText(screen, 0, 25, "Y up/down:"+str(analogY));
        
        if(cruisecontroll):
            font.drawText(screen, 0, 40, "Cruise");
            client_socket.sendall("SC:"+str(analogY)+','+str(analogX))
        else:
            if(not pad.square):
                font.drawText(screen, 0, 40, "no cruise and driving")
                client_socket.sendall("MC:"+str(analogY)+','+str(analogX)+','+str(analogX)+',500');
            else:
                font.drawText(screen, 0, 40, "No cruise and servo")
                client_socket.sendall("SC:"+str(analogY)+','+str(analogX))
        screen.swap()
        oldpad = pad
        sleep(0.100)
        
    pspnet.disconnectAPCTL()

def main():
    global val;
    if(pspnet.wlanSwitchState()):
        font.drawText(screen, 0, 2, 'What is the network number')
        screen.swap()
        #keyBoard()
        def cb(s):
            #if s >= 0:
            font.drawText(screen, 0, 50, 'State: %d/4' % s)
            #else:
            #    font.drawText(screen, 0, 70, 'Connected. IP: %s' % pspnet.getIP())
            screen.swap()
    
    
        if(pspnet.getAPCTLState() != 4):
            pspnet.connectToAPCTL(3, cb)
            #pspnet.connectToAPCTL(int(val), cb)
    else:
        screen.clear(black);
        font.drawText(screen, 0, 0, "Please turn on WiFi")
        screen.swap()
        while True:
            if(pspnet.wlanSwitchState()):
                main();
        #
    
    connectToSocket()
    #controll()
# end of main 
    
if __name__ == '__main__':
    try:
        pspos.setclocks(333,166)
        main()
    except KeyboardInterrupt:
        pass
    except:
        import traceback
        traceback.print_exc(file = file('trace.txt', 'w'))
