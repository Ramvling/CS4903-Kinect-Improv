#from pyramid.config import Configurator
#from pyramid.response import Response
import socket
import sys
import threading
import string, cgi, time

sys.path.insert(0, './PyWebPlug')
from wsserver import *
from time import sleep

#Listen for c# connection. This should probably be on a seperate thread
HOST = '127.0.0.1'
CPORT = 8000
WEBPORT = 8086

def setupMessages():
    m1 = createMsgStruct(1, False)
    m1.addString()

    i1 = createMsgStruct(1, True)
    i1.addChars(2)

#testing PyWebPlug connections
print("listening for websocket connection on 8085")
try:
        setupMessages()
        server = startServer()
        while True:
            newClient = handleNetwork()
            if newClient:
                handle(newClient)
            for client in clients:
                client.handle()
            sleep(0.01)
except KeyboardInterrupt:
    print(' received, closing server.')
    server.close()

"""
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
thread.start()"""
