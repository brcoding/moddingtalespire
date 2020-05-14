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
def SelectNextPlayerControlled():
    return ExecuteRemoteFunction('SelectNextPlayerControlled')

def SetCreatureHp(creatureId, currentHp, maxHp):
    return json.loads(ExecuteRemoteFunction('SetCreatureHp {0},{1},{2}'.format(creatureId, currentHp, maxHp)))

def SetCreatureStat(creatureId, statNumber, current, max):
    return json.loads(ExecuteRemoteFunction('SetCreatureStat {0},{1},{2},{3}'.format(creatureId, statNumber - 1, current, max)))

def PlayEmote(creatureId, emote):
    return json.loads(ExecuteRemoteFunction('PlayEmote {0},{1}'.format(creatureId, emote)))

def Knockdown(creatureId):
    return json.loads(ExecuteRemoteFunction('Knockdown {0}'.format(creatureId)))

def MoveCreature(creatureId, direction, steps):
    for i in range(steps):
        json.loads(ExecuteRemoteFunction('MoveCreature {0},{1}'.format(creatureId, direction)))
        sleep(1)

def GetCreatureIdByAlias(alias):
    for creature in GetCreatureList():
        if creature['Alias'].lower() == alias.lower():
            return creature['CreatureId']

# creature_id = GetCreatureIdByAlias("Barf")
# MoveCreature(creature_id, "FORWARD", 3);
# MoveCreature(creature_id, "left", 1);
# MoveCreature(creature_id, "FORWARD", 3);


SelectNextPlayerControlled()
sleep(0.5)
SelectNextPlayerControlled()
sleep(0.5)
SelectNextPlayerControlled()
#TLA_Twirl,TLA_Action_Knockdown,TLA_Wiggle,TLA_MeleeAttack
# PlayEmote(creature_id, "TLA_Action_Knockdown")
# sleep(2)
# PlayEmote(creature_id, "TLA_MeleeAttack")
# sleep(2)
# PlayEmote(creature_id, "TLA_Twirl")
# sleep(2)
# PlayEmote(creature_id, "TLA_Wiggle")

# Knockdown(creature_id)

# SetCreatureHp(creature_id, 40, 100)
# SetCreatureStat(creature_id, 1, 11, 101)
# SetCreatureStat(creature_id, 2, 22, 102)
# SetCreatureStat(creature_id, 3, 33, 103)
# SetCreatureStat(creature_id, 4, 44, 104)