import os
import string

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
        if "!help" in message.content:
            output = "__Available Commands:__\n"
            output += "**!characters**\n"
            output += "   Gets a list of characters you can control.\n"
            output += "**!focus character**\n"
            output += "   Focus the camera on a specific character you control.\n"
            output += "   Example: ```!focus gunter```"
            output += "**!move character direction[forward,backwards,right,left] step**\n"
            output += "   Moves a character you control steps in a direction.\n"
            output += "   Example: ```!move gunter forward 1```"
            output += "**!camera zoom distance[1-10]**\n"
            output += "   Zooms the camera to a set height from 1-10. You can also provide decimals (2.5).\n"
            output += "   Example: ```!camera zoom 5```"
            output += "**!camera rotate degrees**\n"
            output += "   Rotates the camera +-degrees from it's current postion\n"
            output += "   Example: ```!camera rotate 45```"
            output += "**!camera move x z**\n"
            output += "   Moves the camera relative to the current position by the new x and z values.\n"
            output += "   Example: ```!camera move 1 1```"
            output += "```!camera move 2 -1```"
            await message.channel.send(output)
            return
        if message.content.startswith("!characters"):
            output = "Available Characters for {0}:\n".format(message.author)
            for creature in GetCreatureList():
                if str(message.author) in creature["Alias"]:
                    output += "    " + creature["Alias"].split('[')[0].strip() + '\n'
            await message.channel.send(output)
            return
        if message.content.startswith("!focus"):
            parts = message.content.split(' ')
            if len(parts) < 2:
                await message.channel.send("You must specify a character to focus on (!focus gunter)")
                return
            for creature in GetCreatureList():
                if str(message.author) in creature["Alias"]:
                    if creature["Alias"].split('[')[0].strip() == parts[1]:
                        SelectCreatureByCreatureId(creature["CreatureId"])
                        break
            return            
        if message.content.startswith("!move"):
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
        if message.content.startswith("!camera"):

            if "zoom" in message.content:
                parts = message.content.split(' ')
                if len(parts) < 3:
                    await message.channel.send("Camera zoom command must include the amount (a value 1-10 decimals are allowed) (!camera zoom 4)")
                    return
                ZoomCamera(float(parts[2]) / 10, True)
                return

            if "tilt" in message.content:
                parts = message.content.split(' ')
                if len(parts) < 3:
                    await message.channel.send("Camera tilt command must include the degrees (!camera tilt 20)")
                    return
                TiltCamera(parts[2], True)
                return
            
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
        # Parse every character every time there is a message and see if the
        # author matches a creature then say what they say in game.
        #output = "Available Characters for {0}:\n".format(message.author)
        for creature in GetCreatureList():
            if str(message.author) in creature["Alias"]:
                printable = set(string.printable)
                filtered = ''.join(filter(lambda x: x in printable, message.content))
                SayText(creature["CreatureId"], filtered)
                # Only for the first character found.
                break
        return

client = TaleBot()
client.run(token)
