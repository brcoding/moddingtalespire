import os
import discord
from dotenv import load_dotenv
from moddingtalessdk import *

load_dotenv()

token = os.getenv('DISCORD_TOKEN')
discord_channel = int(os.getenv('DISCORD_CHANNEL'))

class TaleBot(discord.Client):
    async def on_message(self, message):
        global getting_who, s
        if message.author == client.user:
            return
        if message.channel.id != discord_channel:
            return
        if "!characters" in message.content:
            output = "Available Characters for {0}:\n".format(message.author)
            for creature in GetCreatureList():
                if str(message.author) in creature["Alias"]:
                    output += "    " + creature["Alias"].split('[')[0].strip() + '\n'
            await message.channel.send(output)
            return
        if "!move" in message.content:
            parts = message.content.split(' ')
            if len(parts) < 4:
                await message.channel.send("You must specify a character to move, direction (forward, backwards, left, right), and how many steps (!move gunter forward 2)")
                return
            if parts[2] not in ['forward', 'backwards', 'right', 'left']:
                await message.channel.send("Direction must be forward, backwards, right, left")
                return
            for creature in GetCreatureList():
                if str(message.author) in creature["Alias"]:
                    if creature["Alias"].split('[')[0].strip() == parts[1]:
                        MoveCreature(creature["CreatureId"], parts[2], parts[3], True)
            return
        if "!camera" in message.content:
            
            if "rotate" in message.content:
                parts = message.content.split(' ')
                if len(parts) < 3:
                    await message.channel.send("Camera rotate command must include the degrees (!camera rotate 10)")
                    return
                RotateCamera(parts[2], False)
                return
            if "move" in message.content:
                parts = message.content.split(' ')
                if len(parts) < 4:
                    await message.channel.send("Camera move command must include x and z (!camera move 10 3)")
                    return
                MoveCamera(parts[2], 0, parts[3], False)
                return
            return

client = TaleBot()
client.run(token)
