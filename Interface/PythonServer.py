import socket
import time

host, port = "127.0.0.1", 25001
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host, port))

while True:
    # send data to C#
    sock.sendall('hello C#'.encode("UTF-8"))

    # receive data from C#
    receivedData = sock.recv(1024).decode("UTF-8")
    print(receivedData)
    
    time.sleep(1)