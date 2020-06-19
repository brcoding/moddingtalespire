import socket
from time import sleep
import json

def SendMessage(command):
    #print(command)
    port = 887
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(('d20armyknife.com', port))
    s.send(command.encode('utf-8'))
    s.settimeout(3)
    data = ""
    while 1:
        d = s.recv(1024).decode('utf-8')
        data += d
        if not d:
            break

    print(data)
    s.close()

SendMessage(json.dumps({"sessionid": "abc", "type": "put", "handoutUrl": "https://i.pinimg.com/originals/8b/f9/e6/8bf9e6bc4228d4050c0c99a8e95d51dd.png"}))

SendMessage(json.dumps({"sessionid": "abc", "type": "get"}))

