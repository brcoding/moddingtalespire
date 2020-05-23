import os
from dotenv import load_dotenv
from moddingtalessdk import *

from twitchio.ext import commands

load_dotenv()

bot = commands.Bot(
    # set up the bot
    irc_token=os.getenv('TMI_TOKEN'),
    client_id=os.getenv('CLIENT_ID'),
    nick=os.getenv('BOT_NICK'),
    prefix=os.getenv('BOT_PREFIX'),
    initial_channels=[os.getenv('CHANNEL')]
)

move_cam_allowed = True
move_allowed = True

@bot.event
async def event_ready():
    print(f"{os.getenv('BOT_NICK')} is online!")
    ws = bot._ws
    await ws.send_privmsg(os.getenv('CHANNEL'), f"Session Bot is online!")

@bot.event
async def event_message(ctx):
    await bot.handle_commands(ctx)
    print(f'{ctx.channel} - {ctx.author.name}: {ctx.content}')

@bot.command(name='toggle')
async def toggle(ctx):
    global move_allowed, move_cam_allowed
    if ctx.author.name.lower() == os.getenv('BOT_NICK').lower():
        if 'camera' in ctx.content:
            move_cam_allowed = not move_cam_allowed
            if move_cam_allowed:
                await ctx.send("Camera movement enabled.")
            else:
                await ctx.send("Camera movement disabled.")
        if 'move' in ctx.content:
            move_allowed = not move_allowed
            if move_allowed:
                await ctx.send("Character movement enabled.")
            else:
                await ctx.send("Character movement disabled.")

@bot.command(name='characters')
async def characters(ctx):
    output = "Available Characters for {0}:\r\n".format(ctx.author.name)
    for creature in GetCreatureList():
        if str(ctx.author.name) in creature["Alias"]:
            output += "    " + creature["Alias"].split('[')[0].strip() + '\n'
    await ctx.send(output)
    return

@bot.command(name='move')
async def move(ctx):
    global move_allowed
    if not move_allowed:
        await ctx.send("Movement currently disabled.")
        return
    parts = ctx.content.split(' ')
    if len(parts) < 4:
        await ctx.send("You must specify a character to move, direction (forward, backwards, left, right), and how many steps (!move gunter forward 2)")
        return
    if parts[2] not in ['forward', 'backwards', 'right', 'left']:
        await ctx.send("Direction must be forward, backwards, right, left")
        return
    for creature in GetCreatureList():
        controlParts = creature["Alias"].split('[')
        controlName = ""
        if len(controlParts) > 1:
            controlName = controlParts[1].split(']')[0]
        else:
            continue
        if controlName in creature["Alias"]:
            if creature["Alias"].split('[')[0].strip() == parts[1]:
                MoveCreature(creature["CreatureId"], parts[2], parts[3], True)
    return

@bot.command(name='camera')
async def camera(ctx):
    global move_cam_allowed
    if not move_cam_allowed:
        await ctx.send("Camera movement currently disabled.")
        return
    if "rotate" in ctx.content:
        parts = ctx.content.split(' ')
        if len(parts) < 3:
            await ctx.send("Camera rotate command must include the degrees (!camera rotate 10)")
            return
        RotateCamera(parts[2], False)
        return
    if "zoom" in ctx.content:
        parts = ctx.content.split(' ')
        if len(parts) < 3:
            await ctx.send("Camera zoom command must include the amount (a value 1-10 decimals are allowed) (!camera zoom 4)")
            return
        ZoomCamera(float(parts[2]) / 10, True)
        return
    if "move" in ctx.content:
        parts = ctx.content.split(' ')
        if len(parts) < 4:
            await ctx.send("Camera move command must include x and z (!camera move 10 3)")
            return
        MoveCamera(parts[2], 0, parts[3], False)
        return
    return
bot.run()