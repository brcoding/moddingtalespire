#!/usr/bin/python           # This is server.py file                                                                                                                                                                           

# import socket
# from time import sleep
# import json


# def ExecuteRemoteFunction(command):
#     #print(command)
#     port = 999
#     s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#     s.connect(('127.0.0.1', port))
#     s.send(command.encode('utf-8'))
#     s.settimeout(10)
#     data = ""
#     while 1:
#         d = s.recv(1024).decode('utf-8')
#         data += d
#         if not d:
#             break

#     return data
#     #return s.recv(44576).decode('utf-8')
#     #s.close()


import socket               # Import socket module
import thread
import json
import sys
import uuid
from datetime import datetime, timedelta

messages = {}

def on_new_client(clientsocket,addr):
    global messages
    data = ""
    clientsocket.settimeout(2)
    while 1:
        try:
            d = clientsocket.recv(1024)
        except:
            break
        data += d.decode('utf-8')
        if not d or data.endswith("}"):
            break
    print("Got data: " + data)
    if data == "":
        return
    # read and store for pickup later
    json_data = json.loads(data)
    if "sessionid" in json_data and "type" in json_data:
        client_id = json_data["sessionid"]
        if client_id not in messages:
            messages[client_id] = []
        cleaned = []
        # Remove any client entries older than 30 seconds
        for message in messages[client_id]:
            now = datetime.now()
            if now - timedelta(seconds=8) <= message["last_updated"] <= now:
                cleaned.append(message)
        messages[client_id] = cleaned
        if json_data["type"] == "put":
            # used to expire messages after a period of time.
            json_data["last_updated"] = datetime.now()
            json_data["messageid"] = str(uuid.uuid4())
            messages[client_id].append(json_data)

            #Maybe some code to compute the last digit of PI, play game or anything else can go here and when you are done.
            clientsocket.send(json.dumps({"result": "ok"}))
        elif json_data["type"] == "get":
            outbound = []
            for item in messages[client_id]:
                tmp = item.copy()
                tmp.pop("last_updated", None)
                outbound.append(tmp)
            clientsocket.send(json.dumps(outbound))
        else:
            clientsocket.send(json.dumps({"result": "unknown request"}))
    else:
        clientsocket.send(json.dumps({"result": "failure"}))
    clientsocket.close()

def main():
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)         # Create a socket object
        host = socket.gethostname() # Get local machine name
        port = 887                # Reserve a port for your service.

        print 'Server started!'
        print 'Waiting for clients...'

        s.bind((host, port))        # Bind to the port
        s.listen(5)                 # Now wait for client connection.

        while True:
           c, addr = s.accept()     # Establish connection with client.
           thread.start_new_thread(on_new_client,(c,addr))
           #Note it's (addr,) not (addr) because second parameter is a tuple
           #Edit: (c,addr)
           #that's how you pass arguments to functions when creating new threads using thread module.
        s.close()
    except (KeyboardInterrupt, SystemExit):
        sys.exit()

if __name__ == "__main__":
    main()