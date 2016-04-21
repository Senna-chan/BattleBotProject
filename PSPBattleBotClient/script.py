import socket, sys, os
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
def keyBoard():
    global screen, bg, font, white, val

    # ptDanzeff PyOSK: Initialise and load OSK graphics
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
    keyBoard()
    ip = val
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
                
    controll()

    
def controll():
    global client_socket, ip
    while True:
        pad = psp2d.Controller()
        if pad.circle:
            break
        analogX = pad.analogX
        analogY = pad.analogY
        
        screen.clear(black)
        font.drawText(screen, 0, 0, "X sideways:"+str(analogX));
        font.drawText(screen, 0, 25, "Y up/down:"+str(analogY));
        screen.swap()
        #client_socket.close();
        #client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        #client_socket.connect((ip, 20010))
        if(not pad.r):
            client_socket.send("MC:"+str(analogY)+','+str(analogX)+','+str(analogX)+',500');
            
        else:
            client_socket.send("SC:"+str(analogY)+','+str(analogX))
        
        sleep(0.050)
        
    pspnet.disconnectAPCTL()

def main():
    global val;
    font.drawText(screen, 0, 2, 'What is the network number')
    screen.swap()
    keyBoard()
    def cb(s):
        #if s >= 0:
        font.drawText(screen, 0, 50, 'State: %d/4' % s)
        #else:
        #    font.drawText(screen, 0, 70, 'Connected. IP: %s' % pspnet.getIP())
        screen.swap()
    
    if(pspnet.wlanSwitchState()):
        if(pspnet.getAPCTLState() != 4):
            pspnet.connectToAPCTL(int(val), cb)
    else:
        screen.clear(black);
        font.drawText(screen, 0, 0, "Please turn on WiFi")
        screen.swap()
        while True:
            if(pspnet.wlanSwitchState()):
                main();
        #
    
    connectToSocket()
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
