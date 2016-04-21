-- ptDanzeff LuaOSK v0.01 (20060522) by LilCthulhu 
-- http://www.palmtime.com/ - lilcthulhu@palmtime.com

-- Please read readme.txt and license.txt

-- danzeff public functions                danzeff function description
-- ------------------------------------ -- ------------------------------------------------------
-- danzeff_load() 			-- Initialise and load OSK graphics
-- danzeff_isinitialized()		-- Returns whether OSK is initialised or not as a boolean
-- danzeff_moveTo(x,y) 			-- Position OSK on screen
-- danzeff_render()         		-- Render OSK on screen		
-- danzeff_readInput(Controls.read())	-- Read and returns OSK input as an integer
-- danzeff_free()			-- Release OSK graphics memory

-- danzeff private functions			   danzeff function description
-- ------------------------------------ -- ------------------------------------------------------
-- danzeff_dirty()			-- Returns whether OSK is dirty or not as boolean	
-- danzeff_boolValue(bool)		-- Convert boolean value to c boolean value 
-- danzeff_compareControllers(c1,c2)	-- Returns whether two controller buttons are the same
-- surface_draw_offset(p,x,y,oX,oY,w,h)	-- Draw surface at offset
-- surface_draw(p)			-- Draw surface

-- ptDanzeff LuaOSK requires that screen variable is set to screen

DANZEFF_LEFT	= 1			-- Left button return value
DANZEFF_RIGHT	= 2			-- Right button return value
DANZEFF_SELECT	= 3			-- Select button return value
DANZEFF_START	= 4			-- Start button return value

-- ptDanzeff LuaOSK requires that screen variable is set to screen
-- screen	= screen		-- not required for lua adaptation
-- holding	= false			-- not used by lua adaptation
prevctrl	= Controls.read()	-- lua adaptation -- previous controls data 
dirty		= true        		-- whether OSK is dirty or not as boolean
shifted		= false			-- whether OSK is shifted or not as boolean
mode		= 0            		-- OSK mode as integer
initialized	= false 		-- whether OSK is initialized or not as boolean

selected_x	= 1			-- selected_x OSK section as integer
selected_y	= 1			-- selected_y OSK section as integer

PICS_BASEDIR	= ".\\danzeff\\"	-- graphics directory as string

guiStringsSize	= 12			-- guiStrings string array size
			
guiStrings	= {			-- guiStrings string array
	PICS_BASEDIR .. "keys.png", 	PICS_BASEDIR .. "keys_t.png", 	PICS_BASEDIR .. "keys_s.png",
	PICS_BASEDIR .. "keys_c.png", 	PICS_BASEDIR .. "keys_c_t.png", PICS_BASEDIR .. "keys_s_c.png",
	PICS_BASEDIR .. "nums.png", 	PICS_BASEDIR .. "nums_t.png", 	PICS_BASEDIR .. "nums_s.png",
	PICS_BASEDIR .. "nums_c.png", 	PICS_BASEDIR .. "nums_c_t.png", PICS_BASEDIR .. "nums_s_c.png"
}

MODE_COUNT	= 2			-- number of mode

modeChar	= {			-- all mode characters
	{	{ ",abc",  ".def","!ghi" },
		{ "-jkl","\010m n", "?opq" },
		{ "(rst",  ":uvw",")xyz" }
	},
	{	{ "^ABC",  "@DEF","*GHI" },
		{ "_JKL","\010M N", "\"OPQ" },
		{ "=RST",  ";UVW","/XYZ" }
	},
	{	{ "\0\0\0001","\0\0\0002","\0\0\0003" },
		{ "\0\0\0004",  "\010\0 5","\0\0\0006" },
		{ "\0\0\0007","\0\0\0008", "\0\00009" }
	},
	{	{ "'(.)",  "\"<'>","-[_]" },
		{ "!{?}","\010\0 \0", "+\\=/" },
		{ ":@;#",  "~$`%","*^|&" }
	}
}

keyBits		= {}			-- Graphics array
keyBitsSize	= 0			-- Unused
moved_x		= 0			-- OSK x position
moved_y		= 0			-- OSK y position

-- Public: Returns whether OSK is initialised or not as a boolean
function danzeff_isinitialized() 
	return initialized 
end

-- Private: Returns whether OSK is dirty or not as boolean 
function danzeff_dirty() 
	return dirty 
end

-- Private: Convert boolean value to c boolean value 
function danzeff_boolValue(bool) -- lua adaptation 
	if (bool) then
		return 1
	else
		return 0
	end 
end 

-- Private: Returns whether two controller buttons are the same
function danzeff_compareControllers(controller1, controller2) -- lua adaptation
	if (controller1:square() ~= controller2:square()) then
		return false
	elseif (controller1:triangle() ~= controller2:triangle()) then
		return false
	elseif (controller1:circle() ~= controller2:circle()) then
		return false
	elseif (controller1:cross() ~= controller2:cross()) then
		return false
	elseif (controller1:up() ~= controller2:up()) then
		return false
	elseif (controller1:down() ~= controller2:down()) then
		return false
	elseif (controller1:left() ~= controller2:left()) then
		return false
	elseif (controller1:right() ~= controller2:right()) then
		return false
	elseif (controller1:start() ~= controller2:start()) then
		return false
	elseif (controller1:select() ~= controller2:select()) then
		return false
	elseif (controller1:l() ~= controller2:l()) then
		return false
	elseif (controller1:r() ~= controller2:r()) then
		return false
	end
	
	return true
end

-- Public: Read and returns OSK input as an integer
function danzeff_readInput(pspctrl)
	x = 1
	y = 1
	
	if (pspctrl:analogX() < 85 - 127) then      -- -127 lua adaptation
	 	x = x - 1
	elseif (pspctrl:analogX() > 170 - 127) then -- -127 lua adaptation
	 	x = x + 1 	
	end

	if (pspctrl:analogY() < 85 - 127) then      -- -127 lua adaptation
	 	y = y - 1
	elseif (pspctrl:analogY() > 170 - 127) then -- -127 lua adaptation
		y = y + 1
	end	

	if (selected_x ~= x or selected_y ~= y) then 
		dirty = true
		selected_x = x
		selected_y = y
	end 

	if ((not shifted and pspctrl:r()) or (shifted and not pspctrl:r())) then 		
		dirty = true
	end

	pressed = 0 

	shifted = pspctrl:r()
		
	if (not danzeff_compareControllers(pspctrl, prevctrl)) then
		if (pspctrl:cross() or pspctrl:circle() or pspctrl:triangle() or pspctrl:square()) then
			innerChoice = 0

			if (pspctrl:triangle()) then
				innerChoice = 0
			elseif (pspctrl:square()) then
				innerChoice = 1
			elseif (pspctrl:cross()) then
				innerChoice = 2
			else -- if (pspctrl:circle()) then
				innerChoice = 3	
			end

			pressed = string.byte(string.sub(modeChar[mode * 2 + danzeff_boolValue(shifted) + 1][y + 1][x + 1],innerChoice + 1,innerChoice + 1))	-- lua adaptation		
		elseif (pspctrl:l()) then 
			dirty = true
			mode = mode + 1
			mode = math.mod(mode,MODE_COUNT)
		elseif (pspctrl:down()) then
			pressed = 13
		elseif (pspctrl:up()) then 
			pressed = 8 
		elseif (pspctrl:left()) then
			pressed = DANZEFF_LEFT 
		elseif (pspctrl:right()) then
			pressed = DANZEFF_RIGHT 
		elseif (pspctrl:select()) then
			pressed = DANZEFF_SELECT 
		elseif (pspctrl:start()) then
			pressed = DANZEFF_START 
		end
	end 
		
	prevctrl = pspctrl -- lua adaptation
	
	return pressed
end

-- Private: Draw surface at offset
function surface_draw_offset(pixels, screenX, screenY, offsetX, offsetY, intWidth, intHeight)
	danzeff_screen_rect_x = moved_x + screenX
	danzeff_screen_rect_y = moved_y + screenY

	pixels_rect_x = offsetX
	pixels_rect_y = offsetY
	pixels_rect_w = intWidth
	pixels_rect_h = intHeight

	screen:blit(danzeff_screen_rect_x, danzeff_screen_rect_y, pixels, pixels_rect_x, pixels_rect_y, pixels_rect_w, pixels_rect_h, true)

	return
end

-- Private: Draw surface
function surface_draw(pixels)
	surface_draw_offset(pixels, 0, 0, 0, 0, pixels:width(), pixels:height())

	return
end

-- Public: Render OSK on screen
function danzeff_render()
	dirty = false

	if (selected_x == 1 and selected_y == 1) then
		surface_draw(keyBits[6 * mode + danzeff_boolValue(shifted) * 3 + 1]) -- lua adaptation
	else
		surface_draw(keyBits[6 * mode + danzeff_boolValue(shifted) * 3 + 1 + 1]) -- lua adaptation
	end
	 
	surface_draw_offset(keyBits[6 * mode + danzeff_boolValue(shifted) * 3 + 2 + 1], selected_x * 43, selected_y * 43, selected_x * 64,selected_y * 64, 64, 64) -- lua adaptation

	return
end

-- Public: Position OSK on screen
function danzeff_moveTo(newX, newY)
	moved_x = newX
	moved_y = newY

	return
end

-- Public: Initialise and load OSK graphics
function danzeff_load()
	if (initialized) then return end

	for a = 1, guiStringsSize do
		keyBits[a] = Image.load(guiStrings[a])

		if (keyBits[a] == nil) then
			for b = 1, a do
				keyBits[b] = nil
			end
			
			initialized = false
			return
		end
	end

	initialized = true
	
	return
end

-- Public: Release OSK graphics memory
function danzeff_free()
	if (not initialized) then return end

	for a = 1, guiStringsSize do
		keyBits[a] = nil
	end
	
	initialized = false

	return
end 