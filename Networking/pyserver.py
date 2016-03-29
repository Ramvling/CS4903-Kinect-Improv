#from pyramid.config import Configurator
#from pyramid.response import Response
import socket
import threading
import httplib

#Listen for c# connection. This should probably be on a seperate thread
HOST = '127.0.0.1'
PORT = 8000

#testing SockJS connections
"""print"listening for websocket"
ws = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
ws.bind( (HOST, 8085) )
ws.listen(1)
conn, ad = ws.accept()
while True:
        data = conn.recv(1024)
        if not data:
            break
        print (data)
        wait = input("enter a string:")
        conn.sendall(wait)"""


sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind( (HOST, PORT) )
#probably need to form some sort of handshake here so I know what kind of client is connecting
print("Listening for c# connection on localhost port 8000")
sock.listen(1)
connection, addr = sock.accept()
print ('C# connection made, spinning up new thread')

#Runs on a seperate thread and communicates with the kinect
def talkToC():
    while True:
        data = connection.recv(1024)
        if not data:
            break
        print (data)
        wait = input("enter a string:")
        connection.sendall(wait)

thread = threading.Thread(target = talkToC)
thread.start()
