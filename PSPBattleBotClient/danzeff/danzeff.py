# ptDanzeff PyOSK v0.01 (20060522) by LilCthulhu 
# http://www.palmtime.com/ - lilcthulhu@palmtime.com

# Please read readme.txt and license.txt

# danzeff public functions                danzeff function description
# ------------------------------------- # ------------------------------------------------------
# danzeff_load() 			# Initialise and load OSK graphics
# danzeff_isinitialized()		# Returns whether OSK is initialised or not as a boolean
# danzeff_moveTo(x,y) 			# Position OSK on screen
# danzeff_render()         		# Render OSK on screen		
# danzeff_readInput(Controls.read())	# Read and returns OSK input as an integer
# danzeff_free()			# Release OSK graphics memory

# danzeff private functions			   danzeff function description
# -------------------------------------	# ------------------------------------------------------
# danzeff_dirty()			# Returns whether OSK is dirty or not as boolean	
# danzeff_boolValue(bool)		# Convert boolean value to c boolean value 
# danzeff_compareControllers(c1,c2)	# Returns whether two controller buttons are the same
# surface_draw_offset(p,x,y,oX,oY,w,h)	# Draw surface at offset
# surface_draw(p)			# Draw surface

# ptDanzeff PyOSK requires that screen variable is set to psp2d.Screen()

import math
import psp2d

DANZEFF_LEFT	= 1			# Left button return value
DANZEFF_RIGHT	= 2			# Right button return value
DANZEFF_SELECT	= 3			# Select button return value
DANZEFF_START	= 4			# Start button return value

# ptDanzeff PyOSK requires that screen variable is set to psp2d.Screen()
# screen	= psp2d.Screen()	# required by python adaptation
# holding	= False			# not used by python adaptation
prevctrl	= psp2d.Controller()	# python adaptation # previous controls data			
dirty		= True			# whether OSK is dirty or not as boolean	
shifted		= False			# whether OSK is shifted or not as boolean 
mode		= 0			# OSK mode as integer		
initialized	= False 		# whether OSK is initialized or not as boolean

selected_x	= 1			# selected_x OSK section as integer
selected_y	= 1			# selected_y OSK section as integer

PICS_BASEDIR	= ".\\danzeff\\"	# graphics directory as string

guiStringsSize	= 12			# guiStrings string array size

guiStrings	= [			# guiStrings string array,
	PICS_BASEDIR + "nums.png",	PICS_BASEDIR + "nums_t.png",	PICS_BASEDIR + "nums_s.png",
	PICS_BASEDIR + "nums_c.png",	PICS_BASEDIR + "nums_c_t.png",	PICS_BASEDIR + "nums_s_c.png",
	PICS_BASEDIR + "keys.png",	PICS_BASEDIR + "keys_t.png",	PICS_BASEDIR + "keys_s.png",
	PICS_BASEDIR + "keys_c.png",	PICS_BASEDIR + "keys_c_t.png",	PICS_BASEDIR + "keys_s_c.png"
]

MODE_COUNT	= 2			# number of mode

modeChar	= [			# all mode characters
	[	
        [ "\0\0\0001","\0\0\0002","\0\0\0003" ],
		[ "\0\0\0004",  "\010. 5","\0\0\0006" ],
		[ "\0\0\0007","\0\0\0008", "\0\00009" ]
	],
	[	
        [ "'(.)",  "\"<'>","-[_]" ],
		[ "!{?}","\010\0 \0", "+\\=/" ],
		[ ":@;#",  "~$`%","*^|&" ]
	],
	[	
        [ ",abc",  ".def","!ghi" ],
		[ "-jkl","\010m n", "?opq" ],
		[ "(rst",  ":uvw",")xyz" ]
	],
	[	
        [ "^ABC",  "@DEF","*GHI" ],
		[ "_JKL","\010M N", "\"OPQ" ],
		[ "=RST",  ";UVW","/XYZ" ]
	]
]

keyBits		= [None, None, None, None, None, None, None, None, None, None, None, None]	# Graphics array
keyBitsSize	= 12			# Unused
moved_x		= 0			# OSK x position
moved_y		= 0			# OSK x position

# Public: Returns whether OSK is initialised or not as a boolean
def danzeff_isinitialized():	
	global initialized
	
	return initialized 

# Private: Returns whether OSK is dirty or not as boolean
def danzeff_dirty():
	global dirty
	
	return dirty

# Private: Convert boolean value to c boolean value
def danzeff_boolValue(bool): # python adaptation
	if bool:
		return 1
	else:
		return 0

# Private: Returns whether two controller buttons are the same		
def danzeff_compareController(controller1, controller2): # python adaptation
	if (controller1.square != controller2.square):
		return False
	elif (controller1.triangle != controller2.triangle):
		return False
	elif (controller1.circle != controller2.circle):
		return False
	elif (controller1.cross != controller2.cross):
		return False
	elif (controller1.up != controller2.up):
		return False
	elif (controller1.down != controller2.down):
		return False
	elif (controller1.left != controller2.left):
		return False
	elif (controller1.right != controller2.right):
		return False
	elif (controller1.start != controller2.start):
		return False
	elif (controller1.select != controller2.select):
		return False
	elif (controller1.l != controller2.l):
		return False
	elif (controller1.r != controller2.r):
		return False

	return True
	
# Public: Read and returns OSK input as an integer
def danzeff_readInput(pspctrl):
	global selected_x, selected_y, shifted, holding, prevctrl, modeChar, mode, MODE_COUNT, DANZEFF_LEFT, DANZEFF_RIGHT, DANZEFF_LEFT, DANZEFF_START, DANZEFF_SELECT
		
	x = 1
	y = 1
	
	if (pspctrl.analogX < 85 - 127):		# -127 python adaptation
	 	x = x - 1
	elif (pspctrl.analogX > 170 - 127):		# -127 python adaptation
	 	x = x + 1 	

	if (pspctrl.analogY < 85 - 127):		# -127 python adaptation
	 	y = y - 1
	elif (pspctrl.analogY > 170 - 127):		# -127 python adaptation
		y = y + 1

	if (selected_x != x or selected_y != y): 
		dirty = True
		selected_x = x
		selected_y = y

	if ((not shifted and pspctrl.r) or (shifted and not pspctrl.r)): 		
		dirty = True

	pressed = 0 

	shifted = pspctrl.r
		
	if (not danzeff_compareController(prevctrl, pspctrl)):
		if (pspctrl.cross or pspctrl.circle or pspctrl.triangle or pspctrl.square):
			innerChoice = 0

			if (pspctrl.triangle):
				innerChoice = 0
			elif (pspctrl.square):
				innerChoice = 1
			elif (pspctrl.cross):
				innerChoice = 2
			else: # if (pspctrl.circle):
				innerChoice = 3	
	
			pressed = ord(modeChar[mode * 2 + danzeff_boolValue(shifted)][y][x][innerChoice]) # python adaptation		
		elif (pspctrl.l): 
			dirty = True
			mode = mode + 1
			mode = mode % MODE_COUNT
		elif (pspctrl.down):
			pressed = 13
		elif (pspctrl.up): 
			pressed = 8 
		elif (pspctrl.left): 
			pressed = DANZEFF_LEFT 
		elif (pspctrl.right): 
			pressed = DANZEFF_RIGHT 
		elif (pspctrl.select): 
			pressed = DANZEFF_SELECT 
		elif (pspctrl.start): 
			pressed = DANZEFF_START 

	prevctrl = pspctrl # python adaptation
	
	return pressed

# Private: Draw surface at offset
def surface_draw_offset(pixels, screenX, screenY, offsetX, offsetY, intWidth, intHeight):
	global screen

	danzeff_screen_rect_x = moved_x + screenX
	danzeff_screen_rect_y = moved_y + screenY

	pixels_rect_x = offsetX
	pixels_rect_y = offsetY
	pixels_rect_w = intWidth
	pixels_rect_h = intHeight

	screen.blit(pixels, pixels_rect_x, pixels_rect_y, pixels_rect_w, pixels_rect_h, danzeff_screen_rect_x, danzeff_screen_rect_y, True, intWidth, intHeight)

	return

# Private: Draw surface	
def surface_draw(pixels):
	surface_draw_offset(pixels, 0, 0, 0, 0, pixels.width, pixels.height)

	return

# Public: Render OSK on screen
def danzeff_render():
	global dirty, selected_x, selected_y, keyBits, mode, shifted
		
	dirty = False

	if (selected_x == 1 and selected_y == 1):
		surface_draw(keyBits[6 * mode + danzeff_boolValue(shifted) * 3]) # python adaptation
	else:
		surface_draw(keyBits[6 * mode + danzeff_boolValue(shifted) * 3 + 1]) # python adaptation
	
	surface_draw_offset(keyBits[6 * mode + danzeff_boolValue(shifted) * 3 + 1 + 1], selected_x * 43, selected_y * 43, selected_x * 64, selected_y * 64, 64, 64) # python adaptation
	
	return

# Public: Position OSK on screen
def danzeff_moveTo(newX, newY):
	global moved_x, moved_y

	moved_x = newX
	moved_y = newY

	return

# Public: Initialise and load OSK graphics	
def danzeff_load():
	global initialized, guiStringsSize, keyBits
		
	if (initialized):
		return

	for a in range(guiStringsSize):
		keyBits[a] = psp2d.Image(guiStrings[a])

		if (keyBits[a] == None):
			for b in range (a):
				keyBits[b] = None
			
			initialized = False
			return

	initialized = True
	
	return

# Public: Release OSK graphics memory	
def danzeff_free():
	global initialized, guiStringsSize, keyBits
    
	if (not initialized):
		return
		
	for a in range(guiStringsSize):
		keyBits[a] = None
	
	initialized = False

	return
