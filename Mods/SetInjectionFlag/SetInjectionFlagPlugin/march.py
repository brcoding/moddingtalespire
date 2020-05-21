import socket
import random
from time import sleep
import json
import threading

def ExecuteRemoteFunction(command):
    # print(command)
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

def SetCameraHeight(height, absolute):
    return json.loads(ExecuteRemoteFunction('SetCameraHeight {0},{1}'.format(height, absolute)))

def MoveCamera(rotation, x, y, z, absolute):
    return json.loads(ExecuteRemoteFunction('MoveCamera {0},{1},{2},{3},{4}'.format(rotation, x, y, z, absolute)))

def MoveCreature(creatureId, direction, steps):
    return json.loads(ExecuteRemoteFunction('MoveCreature {0},{1},{2}'.format(creatureId, direction, steps)))

def GetCreatureIdByAlias(alias):
    for creature in GetCreatureList():
        if creature['Alias'].lower() == alias.lower():
            return creature['CreatureId']

def MoveAudience():
    threading.Timer(0.6, MoveAudience).start()

    minX = -6.39716673
    minZ = -12.7042475
    maxX = 9.526673
    maxZ = -1.40465093
    random.seed()
    for creature in GetCreatureList():
        if creature['Alias'] in ["Drunk, Toasting", "Noble, Fancy", "Merchant, Snake-Oil", "Commoner, Welcoming"]:
            direction = random.choice(["forward", "backwards", "left", "right"])
            if creature['Position']['x'] - 1 < minX and direction == "left":
                direction = "right"
            if creature['Position']['z'] - 1 < minZ and direction == "backwards":
                direction = "forward"
            if creature['Position']['x'] > maxX and direction == "right":
                direction = "left"
            if creature['Position']['z'] + 0.5 > maxZ and direction == "forward":
                direction = "backwards"

            MoveCreature(creature['CreatureId'], direction, 1);


def MoveStage():
    damsel_id = GetCreatureIdByAlias("damsel")
    knight_id = GetCreatureIdByAlias("knight")
    wolf_id = GetCreatureIdByAlias("wolf")
    bard_id = GetCreatureIdByAlias("bard")

    MoveCreature(bard_id, "right", 1);

    MoveCreature(damsel_id, "backwards", 2);
    MoveCreature(wolf_id, "left", 3);
    MoveCreature(knight_id, "left", 3);
    sleep(1.5)
    MoveCreature(damsel_id, "right", 3);
    MoveCreature(wolf_id, "backwards", 2);
    MoveCreature(knight_id, "left", 2);
    MoveCreature(bard_id, "left", 1);
    sleep(1.5)
    MoveCreature(damsel_id, "right", 2);
    MoveCreature(wolf_id, "right", 3);
    MoveCreature(knight_id, "backwards", 2);
    sleep(1.5)
    MoveCreature(damsel_id, "forward", 2);
    MoveCreature(wolf_id, "right", 2);
    MoveCreature(knight_id, "right", 3);
    sleep(1.5)
    MoveCreature(bard_id, "right", 1);
    MoveCreature(damsel_id, "left", 5);
    MoveCreature(knight_id, "right", 1);
    sleep(0.6)
    PlayEmote(knight_id, "TLA_MeleeAttack")
    sleep(0.6)
    PlayEmote(wolf_id, "TLA_Action_Knockdown")


# MoveCreature(GetCreatureIdByAlias("damsel"), "left", 3)

MoveAudience()
# sleep(5)
#MoveStage()

# SLAB FOR RECREATION
# ```H4sIAAAAAAAAC32YTYgcRRTHK5NZM+AeBg8OIpLGiASJOGjQ4MJUtYYNRNAFwYBGMrAGgwc/QBBzkMYPcgjqmOwhhw2ZEAU/wB3YYRVD6GoCEQ/uapBF8WBOGkEh8WLUbMau6nlT1dXvvYZJeuq379X7v/equqdW/139fpO4Wey4rN48eOHZJ858cP7Keuuly3UhxG2tU0emTt6u3n7shU9W5gcHNudjZ+e7ay/u3xIv/744/fTX+w6187HZ53f8uraazh6NrrV2Xb0YPZePrW/cc/WV03ftXf75+P6p3z79x/i7d2nj2jMzP+55744try88ue/Epnzs8b/r/x3eePSRY1+1v/3s9Duv1vKx42cuXP9jpTf7bv3g8ra3vvjBjP2URHfOvX9sz9Iv30ztfXj9lkY+tvmv7L7aQ3fHR147sHDT4emT2/KxxsW1++fP79z9ZevoR6c+/7N1qzBXlAnRl0JcUkIksrhPZL3EXs58ZsiNkYwxNm3tVIz5bBRMmX+EGIyZ9ScbJoHFfZUV9ynOzNXLx1T+/Tvj27CO+b9eYoOAGXtgPYZ1GZ8Rw7Ss+uwycXaJOO2V4naLmWOYPmCYPmCYPmCYPmChPjHMx4g4zaWJOA/FjoVx+j6DOFci7RgWpybiNNqNLRanz7A6AKvE6bFKPhPHgjhXzHdglTjT4juaa+FYqD2RjqG1lbQGYGgPSjpnmolTE3H6DItTE3H682FxaiJOWyNFxwkMyycwLE5gaE8oppcUnU/YC7A4gWH5BIbFCQzdC0Txva/Ge2vH22PHTIUsdQz+Fu6LQuA+fRb69Fngc7TVPGcIn/ZD+PRZJc6d+XRMnJqKc+gYqj3DfQ5jxzCfwAKfC7tnvFynZbtdmVcjjfiE2gZsu2d3KbQTjjUzuifaDJsrs3Mz273apsh8GtVw7uwDXi+V2WhJOxZqsH41o0HjGkxPAJsLmfB6KdQw9HoQyWeCaxBXlGOVOng+Aw32+ZdQGrz5Qg12/yQ0+AzToAkNPqtoSByraFCOVXopdQzTMFkrWC9ltIbJ2sTWSkZo8OzCXjJ1AFapg+cz1GDXraL7DBi6xhQei2+HalB0/YCFOfPnw3KdMBqAof1JaLAfRkPCaEgIDT5De57RoBkNmtDgM2xf0owGzWjQhAZ7xbgGy2NaAzC0l2Kml2Jcgz8fuv5iWkNEaPAZth4iRkPEaIgIDT7D1kPEaJisFUkz7B1lsjZV1S5hfCaEz9JaQebTjE/N+NRMnJPayup8k55g7DCfEeGzVCNEX8T4NM/GvqyeeViOnnlM53v5jdFW5szD5AXzCRfFTqii9h/79ejUH3yqmfe6Rpm1M78pBrJm3+XNR3fE+DykJrqaYsX7f99jice6DItwZs9ttMTtEnl9NOqgrDka1eyzE7crfr/jsdTsGQQZ57h+UPe+dHUHpkI2dAz6x7cTRL+Yd2hNMHj2Y+xDzw7rJeiXgI1Gyy6W0M7GzbCJvpTJiw5Y4uWzzIqzC9AX2Jk5gDWzgHmxtDM6ljmEwblimRX9AqwdMN+uybCyhjLra3o+xdiVc90Yi6DrnhDMrgfGTjD7EsyH7T0JwWy+0TPc8dkFwybaJZ2X8vor57OvqnZmr1MyPCsp7IANGNYLmO+zy7CI8aklzYSs+uzh7I1EurOZ0C6R7pwIYyYGaj7BMDhzxFiXYOb3CrCwDmavAxbWYdGzw+oALKyDzYsm6pA6FtbBXHD2i+aFYZBTs4+afrT5jf8H79yXUQgaAAA=```
# Just add the named characters:
# damsel
# knight
# wolf
# bard

# For the audience just add a mix of:
# "Drunk, Toasting", "Noble, Fancy", "Merchant, Snake-Oil", "Commoner, Welcoming"