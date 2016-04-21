# ptDanzeff PyNewkey v0.01 (20060522) by LilCthulhu
# http://www.palmtime.com/ - lilcthulhu@palmtime.com

# Please read readme.txt and license.txt

import psp2d

# ptDanzeff PyOSK: Include ptDanzeff PyOSK.
execfile(".\\danzeff\\danzeff.py")

# ptDanzeff PyOSK requires that screen variable is set to psp2d.Screen()
screen = psp2d.Screen()	# required by python adaptation

bg = psp2d.Image("bg.png")			
fnt = psp2d.Font("font.png")		
white = psp2d.Color(255, 255, 255) 	
val = ''							

def demo():
	global screen, bg, fnt, white, val

	# ptDanzeff PyOSK: Initialise and load OSK graphics
	danzeff_load()						
	
	# ptDanzeff PyOSK: Returns whether OSK is initialised or not as a boolean
	if (not danzeff_isinitialized()):	
		return

	# ptDanzeff PyOSK: Position OSK on screen
	danzeff_moveTo(220,20)
	
	while (True):
		# Prepare background
		screen.blit(bg)

		# ptDanzeff PyOSK: Render OSK on screen		
		danzeff_render() 

		# ptDanzeff PyOSK: Returns OSK input as an integer
		cha = danzeff_readInput(psp2d.Controller())

		# Now Evaluate return value and take proper action
		if (cha != 0 and cha != 1 and cha != 2):
			if (cha == 8):
				val = val[0:len(val)-1]
			elif (cha == 13):
				val = ""
			elif (cha == 4):
				return
			else:
				val = val + chr(cha)

		# Print current input string
		fnt.drawText(screen, 5, 230, val)	

		# Refresh screen	
		screen.swap()

# Launch ptDanzeff PyOSK Demo					
demo()

# ptDanzeff PyOSK: Release OSK graphics memory
danzeff_free()
