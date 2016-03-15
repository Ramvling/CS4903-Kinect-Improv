import socket
import StringIO
import sys

class WSGIServer(object):
    addressFamily = socket.AF_INET
    socketType = socket.SOCK_STREAM
    requestQueueSize = 1

    def __init__(self, server_address):
        #Listening socket
        self.listenSocket = listenSocket = socket.socket(self.addressFamily, self.socketType)
        #allow address reuse
        listenSocket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        #bind
        listenSocket.bind( server_address )
        #Activate
        listenSocket.listen(self.requestQueueSize)
        #host name and port
        host,port = self.listenSocket.getsockname()[:2]
        self.serverName = socket.getfqdn(host)
        self.serverPort = port
        #return headers set by Web framework/Web application
        self.headersSet = []

    def setApp(self, application):
        self.application = application

    def serveForever(self):
        ls = self.listenSocket
        while True:
            self.clientConnection, clientAddress = ls.accept()
            #handles one request then closes client connection
            self.handleOnce()
    
    def handleOnce(self):
        print "handling one request"
        self.requestData = requestData = self.clientConnection.recv(1024)
        #Internet says this prints formatted request data
        print(''.join(
            '< {line}\n'.format(line=line)
            for line in requestData.splitlines()
        ))
        self.parseRequest(requestData)

        #enviorment dictionary using request data
        env = self.getEnv()

        #calls application to get a result
        result = self.application(env, self.startResponse)

        #constructs a response
        self.finishResponse(result)

    def parseRequest(self,text):
        requestLine = text.splitLines()[0]
        requestLine = requestLine.rstrip('\r\n')
        #break down the request line into components
        (self.requestMethod, #get
         self.path,
         self.requestVersion
        ) = requestLine.split()

    def getEnv(self):
        env = {}
        #required WSGI variables
        env['wsgi.version'] = (1,0)
        env['wsgi.url_scheme'] = 'http'
        env['wsgi.input'] = StringIO.StringIO(self.requestData)
        env['wsgi.errors'] = sys.stderr
        env['wsgi.multithread'] = False
        env['wsgi.multiprocess'] = False
        env['wsgi.run_once'] = False
        #required CGI variables
        env['REQUEST_METHOD'] = self.requestMethod
        env['PATH_INFO'] = self.path
        env['SERVER_NAME'] = self.serverName
        env['SERVER_PORT'] = str(self.server_port)

    def startResponse(self, status, headers, excInfo = None):
        #add necessary server headers
        serverHeaders = [
            ('Data', 'Tue, 31 Mar 2015 12:54:48 GMT'), 
            ('Server', 'WSGIServer 0.0'),
        ]
        self.headersSet = [status, headers + serverHeaders]

    def finishResponse(self, result):
        try:
            status, responseHeaders = self.headersSet
            response = 'HTTP/1.1 {status}\r\r'.format(status = status)
            for header in response_headers:
                respone += '{0}: {1}\r\n'.format(*header)
            response += '\r\n'
            for data in result:
                response += data
            #print formatted response data
            print(''.join(
                '> {line}\n'.format(line=line)
                for line in response.splitLines()
            ))
            self.clientConnection.sendall(response)
        finally:
            self.clientConnection.close()


SERVER_ADDRESS = (HOST, PORT) = '', 8888

def makeServer(serverAddr, application):
    server = WSGIServer(serverAddr)
    server.setApp(application)
    return server

if __name__ == '__main__':
    if len(sys.argv) < 2:
        sys.exit('provide a wsgi application object as module:callable')
    appPath = sys.argv[1]
    module, application = appPath.split(':')
    module = __import__(module)
    application = getattr(module, application)
    httpd = makeServer(SERVER_ADDRESS, application)
    print('WSGIServer: Serving HTTP on port {port}... \n'.format(port = PORT))
    httpd.serveForever()


