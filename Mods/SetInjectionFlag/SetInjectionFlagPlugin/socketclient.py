import socket
from time import sleep
import json

def ExecuteRemoteFunction(command):
    port = 999
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(('127.0.0.1', port))
    s.send(command.encode('utf-8'))
    s.settimeout(10)
    return s.recv(24576).decode('utf-8')
    s.close()

# Retrieves all creatures on the current board
def GetCreatureList():
    return json.loads(ExecuteRemoteFunction('GetCreatureList'))

# Gets all creatures controlled by the current player
def GetPlayerControlledList():
    return json.loads(ExecuteRemoteFunction('GetPlayerControlledList'))

# Selects and focuses on a player controlled by alias name
def SelectPlayerControlledByAlias(alias):
    return json.loads(ExecuteRemoteFunction('SelectPlayerControlledByAlias {0}'.format(alias)))

# Selects and focuses the next player controlled creature
def SelectNextPlayerControlled(alias):
    return json.loads(ExecuteRemoteFunction('SelectNextPlayerControlled'))

def SetCreatureHp(creatureId, currentHp, maxHp):
    return json.loads(ExecuteRemoteFunction('SetCreatureHp {0},{1},{2}'.format(creatureId, currentHp, maxHp)))

def SetCreatureStat(creatureId, statNumber, current, max):
    return json.loads(ExecuteRemoteFunction('SetCreatureStat {0},{1},{2},{3}'.format(creatureId, statNumber - 1, current, max)))

def PlayEmote(creatureId, emote):
    return json.loads(ExecuteRemoteFunction('PlayEmote {0},{1}'.format(creatureId, emote)))

creature_id = ""
for creature in GetCreatureList():
    print("Alias: {0} Position: {1} Rotation: {2}".format(creature['Alias'], creature['Position'], creature['Rotation']))
    #print("Alias: {0} Id: {1}".format(creature['Alias'], creature['CreatureId']))
    if creature['Alias'] == 'Barf':
        creature_id = creature['CreatureId']

#TLA_Twirl,TLA_Action_Knockdown,TLA_Wiggle,TLA_MeleeAttack
PlayEmote(creature_id, "TLA_Action_Knockdown")
sleep(2)
PlayEmote(creature_id, "TLA_MeleeAttack")
sleep(2)
PlayEmote(creature_id, "TLA_Twirl")
sleep(2)
PlayEmote(creature_id, "TLA_Wiggle")

SetCreatureHp(creature_id, 40, 100)
SetCreatureStat(creature_id, 1, 11, 101)
SetCreatureStat(creature_id, 2, 22, 102)
SetCreatureStat(creature_id, 3, 33, 103)
SetCreatureStat(creature_id, 4, 44, 104)