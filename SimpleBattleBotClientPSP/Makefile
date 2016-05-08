TARGET = SimpleBattleBotClientPSP
OBJS = main.o

BUILD_PRX = 1


CFLAGS = -O2 -G0 -Wall
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti
ASFLAGS = $(CFLAGS)


LIBDIR =
LDFLAGS =
LIBS = -lpsputility -lpspgum -lpspgu -lpspnet -lm

EXTRA_TARGETS = EBOOT.PBP
PSP_EBOOT_TITLE = Simple Battle Bot Client

PSPSDK=$(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
