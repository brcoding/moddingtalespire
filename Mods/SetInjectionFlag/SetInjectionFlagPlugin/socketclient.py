import socket
import time
import json

def ExecuteRemoteFunction(command):
    port = 999
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(('127.0.0.1', port))
    s.send(command.encode('utf-8'))
    s.settimeout(10)
    return s.recv(24576).decode('utf-8')
    s.close()

# while(1):
creature_list = json.loads(ExecuteRemoteFunction('GetCreatureList'))

for creature in creature_list:
    print("Alias: {0} Position: {1} Rotation: {2}".format(creature['Alias'], creature['Position'], creature['Rotation']))
# print("Got alias list {0}".format(alias_list))
    # for alias in alias_list:
    #     print("Selecting: {0}".format(alias))
    #     ExecuteRemoteFunction('SelectPlayerControlledByName ' + alias)
    #     time.sleep(2)

