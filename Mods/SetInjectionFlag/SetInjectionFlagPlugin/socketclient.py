import socket
import time

def ExecuteRemoteFunction(command):
    port = 999
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(('127.0.0.1', port))
    s.send(command.encode('utf-8'))
    s.settimeout(10)
    return s.recv(2048).decode('utf-8')
    s.close()


alias_list = ExecuteRemoteFunction('GetPlayerControlledList').split('|')

print("Got alias list {0}".format(alias_list))

while(1):
    alias_list = ExecuteRemoteFunction('GetPlayerControlledList').split('|')

    print("Got alias list {0}".format(alias_list))
    for alias in alias_list:
        print("Selecting: {0}".format(alias))
        ExecuteRemoteFunction('SelectPlayerControlledByName ' + alias)
        time.sleep(2)

