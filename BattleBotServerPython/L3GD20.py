import Adafruit_GPIO.I2C as I2C

class L3GD20(I2C.Device):

    L3GD20_ADDRESS = 0x6b
                                             # Default    Type
    L3GD20_REGISTER_CTRL_REG1 = 0x20         # 00000111   rw
    L3GD20_REGISTER_CTRL_REG4 = 0x23         # 00000000   rw

    L3GD20_REGISTER_OUT_X_L = 0x28

    L3GD20_ID  = 0xD4
    L3GD20H_ID = 0xD7

    L3GD20_SENSITIVITY_250DPS  = 0.00875 # Roughly 22/256 for fixed point match
    L3GD20_SENSITIVITY_500DPS  = 0.0175  # Roughly 45/256
    L3GD20_SENSITIVITY_2000DPS = 0.07    # Roughly 18/256

    L3DS20_RANGE_250DPS  = 0x00
    L3DS20_RANGE_500DPS  = 0x10
    L3DS20_RANGE_2000DPS = 0x20
    L3DS20_RANGE_2000DPS_2 = 0x30

    GAIN = 0.07

    def __init__(self, sensitivity=0x20, busnum=1, debug=False):

        self.gyro = I2C.Device(self.L3GD20_ADDRESS, busnum)

        if sensitivity == self.L3DS20_RANGE_250DPS:
            self.GAIN = self.L3GD20_SENSITIVITY_250DPS
        elif sensitivity == self.L3DS20_RANGE_500DPS:
            self.GAIN = self.L3GD20_SENSITIVITY_500DPS
        elif sensitivity == self.L3DS20_RANGE_2000DPS or sensitivity == self.L3DS20_RANGE_2000DPS_2:
            self.GAIN = self.L3GD20_SENSITIVITY_2000DPS

        ''' Set CTRL_REG1 (0x20)
           ====================================================================
           BIT  Symbol    Description                                   Default
           ---  ------    --------------------------------------------- -------
           7-6  DR1/0     Output data rate                                   00
           5-4  BW1/0     Bandwidth selection                                00
             3  PD        0 = Power-down mode, 1 = normal/sleep mode          0
             2  ZEN       Z-axis enable (0 = disabled, 1 = enabled)           1
             1  YEN       Y-axis enable (0 = disabled, 1 = enabled)           1
             0  XEN       X-axis enable (0 = disabled, 1 = enabled)           1 '''

        ''' Switch to normal mode and enable all three channels '''
        ''' ------------------------------------------------------------------ '''

        self.gyro.write8(self.L3GD20_REGISTER_CTRL_REG1, 0x0F) # 00 00 11 11

        ''' Set CTRL_REG2 (0x21)
        ====================================================================
        BIT  Symbol    Description                                   Default
        ---  ------    --------------------------------------------- -------
        5-4  HPM1/0    High-pass filter mode selection                    00
        3-0  HPCF3..0  High-pass filter cutoff frequency selection      0000 '''

        ''' Nothing to do ... keep default values '''
        ''' ------------------------------------------------------------------ '''

        ''' Set CTRL_REG3 (0x22)
        ====================================================================
        BIT  Symbol    Description                                   Default
        ---  ------    --------------------------------------------- -------
         7  I1_Int1   Interrupt enable on INT1 (0=disable,1=enable)       0
         6  I1_Boot   Boot status on INT1 (0=disable,1=enable)            0
         5  H-Lactive Interrupt active config on INT1 (0=high,1=low)      0
         4  PP_OD     Push-Pull/Open-Drain (0=PP, 1=OD)                   0
         3  I2_DRDY   Data ready on DRDY/INT2 (0=disable,1=enable)        0
         2  I2_WTM    FIFO wtrmrk int on DRDY/INT2 (0=dsbl,1=enbl)        0
         1  I2_ORun   FIFO overrun int on DRDY/INT2 (0=dsbl,1=enbl)       0
         0  I2_Empty  FIFI empty int on DRDY/INT2 (0=dsbl,1=enbl)         0 '''

        ''' Nothing to do ... keep default values '''
        ''' ------------------------------------------------------------------ '''

        ''' Set CTRL_REG4 (0x23)
        ====================================================================
        BIT  Symbol    Description                                   Default
        ---  ------    --------------------------------------------- -------
         7  BDU       Block Data Update (0=continuous, 1=LSB/MSB)         0
         6  BLE       Big/Little-Endian (0=Data LSB, 1=Data MSB)          0
        5-4  FS1/0     Full scale selection                               00
                                      00 = 250 dps
                                      01 = 500 dps
                                      10 = 2000 dps
                                      11 = 2000 dps
         0  SIM       SPI Mode (0=4-wire, 1=3-wire)                       0 '''

        ''' Adjust resolution if requested '''
        self.gyro.write8(self.L3GD20_REGISTER_CTRL_REG4, sensitivity) # 00 11 00 00

    def gyro16(self, list, idx):
        n = (list[idx+1] << 8) | list[idx]
        return n if n < 32768 else n - 65536


    def read(self):
        list = self.gyro.readList(
          self.L3GD20_REGISTER_OUT_X_L | 0x80, 6)
        res = ( self.gyro16(list, 0),
                 self.gyro16(list, 2),
                 self.gyro16(list, 4) )

        return res
