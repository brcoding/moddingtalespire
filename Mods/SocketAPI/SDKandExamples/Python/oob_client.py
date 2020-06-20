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


# SendMessage(json.dumps({"sessionid": "321488b0-d193-4671-ae47-6b95bad36ca1", "type": "get"}))


#SendMessage(json.dumps({"sessionid": "321488b0-d193-4671-ae47-6b95bad36ca1", "type": "put", "handoutUrl": "https://i.pinimg.com/originals/8b/f9/e6/8bf9e6bc4228d4050c0c99a8e95d51dd.png"}))
#SendMessage(json.dumps({"sessionid": "321488b0-d193-4671-ae47-6b95bad36ca1", "type": "put", "handoutUrl": "https://geekandsundry.com/wp-content/uploads/2019/04/aibook-mechbeholder.jpg"}))
#SendMessage(json.dumps({"sessionid": "321488b0-d193-4671-ae47-6b95bad36ca1", "type": "put", "handoutUrl": "https://i.pinimg.com/736x/e6/9e/19/e69e19173f655643b82f1bf381003382.jpg"}))
SendMessage(json.dumps({"sessionid": "321488b0-d193-4671-ae47-6b95bad36ca1", "type": "put", "handoutUrl": "https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/8ab86111-40c3-4c11-abf4-2c512a9b3c9d/d990ibg-746a80cf-5bca-43d6-af03-eb16d4202012.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOiIsImlzcyI6InVybjphcHA6Iiwib2JqIjpbW3sicGF0aCI6IlwvZlwvOGFiODYxMTEtNDBjMy00YzExLWFiZjQtMmM1MTJhOWIzYzlkXC9kOTkwaWJnLTc0NmE4MGNmLTViY2EtNDNkNi1hZjAzLWViMTZkNDIwMjAxMi5qcGcifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6ZmlsZS5kb3dubG9hZCJdfQ.c1YY6XSCKHXKApHgeJiu9vo1h2zBbd1gfD_o1qT1qNQ"}))
