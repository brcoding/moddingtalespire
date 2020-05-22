# Setup:
#
# Paste the below slab into TaleSpire to create the stage.
#
# ```H4sIAAAAAAAAC32YTYgcRRTHK5NZM+AeBg8OIpLGiASJOGjQ4MJUtYYNRNAFwYBGMrAGgwc/QBBzkMYPcgjqmOwhhw2ZEAU/wB3YYRVD6GoCEQ/uapBF8WBOGkEh8WLUbMau6nlT1dXvvYZJeuq379X7v/equqdW/139fpO4Wey4rN48eOHZJ858cP7Keuuly3UhxG2tU0emTt6u3n7shU9W5gcHNudjZ+e7ay/u3xIv/744/fTX+w6187HZ53f8uraazh6NrrV2Xb0YPZePrW/cc/WV03ftXf75+P6p3z79x/i7d2nj2jMzP+55744try88ue/Epnzs8b/r/x3eePSRY1+1v/3s9Duv1vKx42cuXP9jpTf7bv3g8ra3vvjBjP2URHfOvX9sz9Iv30ztfXj9lkY+tvmv7L7aQ3fHR147sHDT4emT2/KxxsW1++fP79z9ZevoR6c+/7N1qzBXlAnRl0JcUkIksrhPZL3EXs58ZsiNkYwxNm3tVIz5bBRMmX+EGIyZ9ScbJoHFfZUV9ynOzNXLx1T+/Tvj27CO+b9eYoOAGXtgPYZ1GZ8Rw7Ss+uwycXaJOO2V4naLmWOYPmCYPmCYPmCYPmChPjHMx4g4zaWJOA/FjoVx+j6DOFci7RgWpybiNNqNLRanz7A6AKvE6bFKPhPHgjhXzHdglTjT4juaa+FYqD2RjqG1lbQGYGgPSjpnmolTE3H6DItTE3H682FxaiJOWyNFxwkMyycwLE5gaE8oppcUnU/YC7A4gWH5BIbFCQzdC0Txva/Ge2vH22PHTIUsdQz+Fu6LQuA+fRb69Fngc7TVPGcIn/ZD+PRZJc6d+XRMnJqKc+gYqj3DfQ5jxzCfwAKfC7tnvFynZbtdmVcjjfiE2gZsu2d3KbQTjjUzuifaDJsrs3Mz273apsh8GtVw7uwDXi+V2WhJOxZqsH41o0HjGkxPAJsLmfB6KdQw9HoQyWeCaxBXlGOVOng+Aw32+ZdQGrz5Qg12/yQ0+AzToAkNPqtoSByraFCOVXopdQzTMFkrWC9ltIbJ2sTWSkZo8OzCXjJ1AFapg+cz1GDXraL7DBi6xhQei2+HalB0/YCFOfPnw3KdMBqAof1JaLAfRkPCaEgIDT5De57RoBkNmtDgM2xf0owGzWjQhAZ7xbgGy2NaAzC0l2Kml2Jcgz8fuv5iWkNEaPAZth4iRkPEaIgIDT7D1kPEaJisFUkz7B1lsjZV1S5hfCaEz9JaQebTjE/N+NRMnJPayup8k55g7DCfEeGzVCNEX8T4NM/GvqyeeViOnnlM53v5jdFW5szD5AXzCRfFTqii9h/79ejUH3yqmfe6Rpm1M78pBrJm3+XNR3fE+DykJrqaYsX7f99jice6DItwZs9ttMTtEnl9NOqgrDka1eyzE7crfr/jsdTsGQQZ57h+UPe+dHUHpkI2dAz6x7cTRL+Yd2hNMHj2Y+xDzw7rJeiXgI1Gyy6W0M7GzbCJvpTJiw5Y4uWzzIqzC9AX2Jk5gDWzgHmxtDM6ljmEwblimRX9AqwdMN+uybCyhjLra3o+xdiVc90Yi6DrnhDMrgfGTjD7EsyH7T0JwWy+0TPc8dkFwybaJZ2X8vor57OvqnZmr1MyPCsp7IANGNYLmO+zy7CI8aklzYSs+uzh7I1EurOZ0C6R7pwIYyYGaj7BMDhzxFiXYOb3CrCwDmavAxbWYdGzw+oALKyDzYsm6pA6FtbBXHD2i+aFYZBTs4+afrT5jf8H79yXUQgaAAA=```
#
# After pasting create the following characters on the stage:
#
# damsel
# knight
# wolf
# bard
#
# For the audience just add a mix of:
# "Drunk, Toasting", "Noble, Fancy", "Merchant, Snake-Oil", "Commoner, Welcoming"
#
# It should look something like: https://puu.sh/FNmGv/75862c5fb6.jpg
#
# After the setup is complete you can run this example script and your characters will dance like marionettes 
#
import random
from time import sleep
import threading
# This is the SDK, we are importing functions we wish to use.
from moddingtalessdk import GetCreatureList, MoveCreature, GetCreatureIdByAlias, PlayEmote

audienceTimer = None

def MoveAudience():
    """ Loop a random audience move """
    global audienceTimer
    # Restart the timer every 0.6 seconds (the time it takes for a single step approximately)
    audienceTimer = threading.Timer(0.6, MoveAudience)
    audienceTimer.start()

    # These are the X, Z bounds that the audience should stay within. You may need to adjust as needed.
    minX = -6.39716673
    minZ = -12.7042475
    maxX = 9.526673
    maxZ = -1.40465093
    random.seed()
    # Loop through each creature on the board and find any with the matching aliases.
    for creature in GetCreatureList():
        if creature['Alias'] in ["Drunk, Toasting", "Noble, Fancy", "Merchant, Snake-Oil", "Commoner, Welcoming"]:
            direction = random.choice(["forward", "backwards", "left", "right"])
            # After getting a random direction, make sure the new direction won't move them out
            # of the bounding box, if it does, reverse the direction.
            if creature['Position']['x'] - 1 < minX and direction == "left":
                direction = "right"
            if creature['Position']['z'] - 1 < minZ and direction == "backwards":
                direction = "forward"
            if creature['Position']['x'] > maxX and direction == "right":
                direction = "left"
            if creature['Position']['z'] + 0.5 > maxZ and direction == "forward":
                direction = "backwards"

            # Move the creature, you can also pass in a True as the last parameter to have them
            # Hop around like they are being dragged.
            MoveCreature(creature['CreatureId'], direction, 1);


def MoveStage():
    """Moves the characters around the stage to perform a skit"""

    # Get each of the actors in our play
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
    # When the knight finally catches the wolf have him attack then the wolf gets knocked down.
    PlayEmote(knight_id, "TLA_MeleeAttack")
    sleep(0.6)
    PlayEmote(wolf_id, "TLA_Action_Knockdown")

def main():
    global audienceTimer
    try:
        MoveAudience()
        sleep(1)
        MoveStage()
        while 1:
            sleep(1)
    except KeyboardInterrupt:
        audienceTimer.cancel()

if __name__ == "__main__":
    main()
