import socket
from time import sleep
import json

def ExecuteRemoteFunction(command):
    #print(command)
    port = 999
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(('127.0.0.1', port))
    s.send(command.encode('utf-8'))
    s.settimeout(10)
    data = ""
    while 1:
        d = s.recv(1024).decode('utf-8')
        data += d
        if not d:
            break

    return data

# Retrieves all creatures on the current board
def GetCreatureList():
    #print(ExecuteRemoteFunction('GetCreatureList'))
    return json.loads(ExecuteRemoteFunction('GetCreatureList'))

# Gets all creatures controlled by the current player
def GetPlayerControlledList():
    return json.loads(ExecuteRemoteFunction('GetPlayerControlledList'))

# Selects and focuses on a player controlled by alias name
def SelectPlayerControlledByAlias(alias):
    return json.loads(ExecuteRemoteFunction('SelectPlayerControlledByAlias {0}'.format(alias)))

# Selects a creature by creature id
def SelectCreatureByCreatureId(creatureId):
    return json.loads(ExecuteRemoteFunction('SelectCreatureByCreatureId {0}'.format(creatureId)))

# Selects and focuses the next player controlled creature
def SelectNextPlayerControlled():
    return ExecuteRemoteFunction('SelectNextPlayerControlled')

def SetCreatureHp(creatureId, currentHp, maxHp):
    return json.loads(ExecuteRemoteFunction('SetCreatureHp {0},{1},{2}'.format(creatureId, currentHp, maxHp)))

def SetCreatureStat(creatureId, statNumber, current, max):
    return json.loads(ExecuteRemoteFunction('SetCreatureStat {0},{1},{2},{3}'.format(creatureId, statNumber, current, max)))

def GetCreatureStats(creatureId):
    return json.loads(ExecuteRemoteFunction('GetCreatureStats {0}'.format(creatureId)))

def SetCustomStatName(statNumber, newName):
    return json.loads(ExecuteRemoteFunction('SetCustomStatName {0},{1}'.format(statNumber, newName)))

def PlayEmote(creatureId, emote):
    return json.loads(ExecuteRemoteFunction('PlayEmote {0},{1}'.format(creatureId, emote)))

def SayText(creatureId, text):
    return json.loads(ExecuteRemoteFunction('SayText {0},{1}'.format(creatureId, text)))

def Knockdown(creatureId):
    return json.loads(ExecuteRemoteFunction('Knockdown {0}'.format(creatureId)))

def GetCameraLocation():
    return json.loads(ExecuteRemoteFunction('GetCameraLocation'))

def SetCameraHeight(height, absolute):
    return json.loads(ExecuteRemoteFunction('SetCameraHeight {0},{1}'.format(height, absolute)))

def MoveCamera(x, y, z, absolute):
    return json.loads(ExecuteRemoteFunction('MoveCamera {0},{1},{2},{3}'.format(x, y, z, absolute)))

def ZoomCamera(zoom, absolute):
    return json.loads(ExecuteRemoteFunction('ZoomCamera {0},{1}'.format(zoom, absolute)))

def RotateCamera(rotation, absolute):
    return json.loads(ExecuteRemoteFunction('RotateCamera {0},{1}'.format(rotation, absolute)))

def TiltCamera(tilt, absolute):
    return json.loads(ExecuteRemoteFunction('TiltCamera {0},{1}'.format(tilt, absolute)))

def MoveCreature(creatureId, direction, steps=1, pickUp=False):
    return json.loads(ExecuteRemoteFunction('MoveCreature {0},{1},{2},{3}'.format(creatureId, direction, steps, pickUp)))

def CreateSlab(x, y, z, slabText):
    return json.loads(ExecuteRemoteFunction('CreateSlab {0},{1},{2},{3}'.format(x, y, z, slabText)))

def GetSlabSize(slabText):
    return json.loads(ExecuteRemoteFunction('GetSlabSize {0}'.format(slabText)))

def GetCreatureAssets():
    return json.loads(ExecuteRemoteFunction('GetCreatureAssets'))

def AddCreature(nguid, x, y, z, scale, alias, hpcurr, hpmax, stat1curr, stat1max,
    stat2curr, stat2max, stat3curr, stat3max, stat4curr, stat4max, torch, hidden):
    return json.loads(ExecuteRemoteFunction('AddCreature {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}'.format(nguid, x, y, z, scale, alias, hpcurr, hpmax, stat1curr, stat1max, stat2curr, stat2max, stat3curr, stat3max, stat4curr, stat4max, torch, hidden)))

def KillCreature(creatureId):
    return json.loads(ExecuteRemoteFunction('KillCreature {0}'.format(creatureId)))

def GetBoards():
    return json.loads(ExecuteRemoteFunction('GetBoards'))

def GetCurrentBoard():
    return json.loads(ExecuteRemoteFunction('GetCurrentBoard'))

def LoadBoard(boardId):
    return json.loads(ExecuteRemoteFunction('LoadBoard {0}'.format(boardId)))

def GetCreatureIdByAlias(alias):
    for creature in GetCreatureList():
        if creature['Alias'].lower() == alias.lower():
            return creature['CreatureId']
