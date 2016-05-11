import socket, atexit
from 
speed, wheelpos1, wheelpos2, servoX, servoY = 0;
s = socket.socket()
s.bind(('', 20010))

s.listen(5)                 # Now wait for client connection.
while True:
    c, addr = s.accept()     # Establish connection with client.
    
    c.close()                # Close the connection