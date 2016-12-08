TARGET = SimpleBattleBotClientPSP
OBJS = main.o common/graphics.o common/framebuffer.o common/callback.o common/ini.o
#OBJS = main.o common/callback.o




CFLAGS = -O2 -G0 -Wall -g
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti
ASFLAGS = $(CFLAGS)
BUILD_PRX = 1

LIBDIR =
LDFLAGS =
LIBS =  -lpsputility -lpsphttp -lpspssl -lpsppower -lpspgum -lpspgu -lpng -lz -lm

EXTRA_TARGETS = EBOOT.PBP
PSP_EBOOT_TITLE = Simple Battle Bot Client

PSPSDK=$(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
