import random
from time import sleep
import threading
# This is the SDK, we are importing functions we wish to use.
from moddingtalessdk import *

def main():
    cid = GetCreatureList()[0]["CreatureId"]
    print(cid)
    #SayText(cid, "test")
    SayText(cid, "test test test test test tseset seras asdf")
    #print(GetCreatureList())

if __name__ == "__main__":
    main()
