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
HOST = '128.61.105.215'
CPORT = 8085
WEBPORT = 8886

#List of active websocket clients
clients = []
pID = 0
cConnect = None

def setupMessages():
    testOut = createMsgStruct(0, False)
    testOut.addString()

    testIn = createMsgStruct(0, True)
    testIn.addString()

class WebClient:
    def __init__(self, socket, pID):
        self.socket = socket
        self.pID = pID

    def handle(self):
        global cConnect
        if (self.socket.canHandleMsg() == False):
            return
        packet = self.socket.readPacket()
        #message type
        msgID = packet.msgID
        msg = packet.read()
        print(msg)
        self.socket.newPacket(0)
        self.socket.write(msg)
        self.socket.send()
        cConnect.send(msg.encode())
        
    def disconnect(self):
        print("My lost client")
        clients.remove(self)
        self.socket = None
        return
#new websocket client
def handle(socket):
    global pID, clients
    pID += 1
    client = WebClient(socket, pID)
    clients.append(client)

def serveWeb():		
    #testing PyWebPlug connections
    print("listening for websocket connection on 8886")
    try:
        setupMessages()
        server = startServer()
        while True:
            newClient = handleNetwork()
            if newClient:
                print("new client")
                handle(newClient)
            for client in clients:
                client.handle()
            sleep(0.01)
    except KeyboardInterrupt:
        print(' received, closing server.')
        server.close()

def serveCSharp():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind( (HOST, CPORT) )
    #probably need to form some sort of handshake here so I know what kind of client is connecting
    print("Listening for c# connection on localhost port 8085")
    sock.listen(1)
    connection, addr = sock.accept()
    global cConnect
    cConnect = connection
    print ('C# connection made, spinning up new thread')
    thread = threading.Thread(target = cThread, args=(connection,addr))
    thread.start()

#Runs on a seperate thread and communicates with the kinect
def cThread(connection, addr):
    while True:
        data = connection.recv(1024)
        if not data:
            break
        print (data)
        wait = input("enter a string:")
        
        connection.sendall(wait.encode())


setupMessages()
serveCSharp()
serveWeb()
