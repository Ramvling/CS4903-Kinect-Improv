#from pyramid.config import Configurator
#from pyramid.response import Response
import socket
import threading

#def helloWorld(request):
#    return Response(
#        'Hello world from Pyramid! \n',
#        content_type = 'text/plain',
#    )

#config = Configurator()
#config.add_route('hello', '/hello')
#config.add_view(helloWorld, route_name='hello')
#app = config.make_wsgi_app()

#Listen for c# connection. This should probably be on a seperate thread
HOST = '127.0.0.1'
PORT = 8000
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind( (HOST, PORT) )
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
        connection.sendall(data)

thread = threading.Thread(target = talkToC)
thread.start()
