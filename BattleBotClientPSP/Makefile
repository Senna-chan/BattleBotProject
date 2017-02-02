TARGET = BattleBotClientPSP
OBJS = main.o common/graphics.o common/framebuffer.o common/callback.o common/jWrite.o common/jsmn.o common/helpers.o

BUILD_PRX = 1


CFLAGS = -O2 -G0 -Wall -g
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti
ASFLAGS = $(CFLAGS)


LIBDIR =
LDFLAGS =
LIBS =  -lpsputility -lpsphttp -lpspssl -lpsppower -lpspgum -lpspgu -lpng -lz -lm

EXTRA_TARGETS = EBOOT.PBP
PSP_EBOOT_TITLE = Battle Bot Client

PSPSDK=$(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak