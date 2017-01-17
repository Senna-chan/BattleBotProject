import socket

hostname = socket.gethostname()
IP = socket.gethostbyname(hostname)

UDP_IP = "0.0.0.0"
UDP_PORT = 20010

sock = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP
sock.bind((UDP_IP, UDP_PORT))
print "Awaiting Data"
print "IP = " + IP
while True:
    data, addr = sock.recvfrom(100)
    if(str(data).startswith("client") and str(data).endswith("connected")):
       sock.sendto("YouConnected", addr)
    if(str(data).startswith("client") and str(data).endswith("disconnected")):
        break;
    print(data);
    
