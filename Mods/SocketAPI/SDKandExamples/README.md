# Socket API SDK and Examples

To make accessing the socket API simple a few examples and rudimentary SDKs are available to make life a little easier.

## Available SDKs

* CSharp GUI
* Python SDK and March Example

Each of these projects can be found in the respective subdirectores.

## API Reference

### API Format

The API consists of a couple simple rules for calling, The caller will provide the requests in the format:

```MethodName param1,param2,param3```

There should be a space after the method name and the parameter list should contain no spaces and be comma delimited.

This format was choosen to allow simple input from things like telnet and other simple socket implementations allowing commands to be written by hand if desired.

A quick telnet example:
```
> telnet 127.0.0.1 999
Trying 127.0.0.1...
Connected to 127.0.0.1.
Escape character is '^]'.
> GetCreatureList
[{"BoardAssetId":"7241df8e-f5b7-4ae6-a904-92a1662cab0a",
"CreatureId":"a00a715c-4557-407c-8652-d81bb1f97353",
"UniqueId":"a00a715c-4557-407c-8652-d81bb1f97353",
"Position":{"x":3.530932,"y":1.00308585,"z":4.464703},
"Rotation":{"w":0.522629,"x":0.0,"y":0.0,"z":0.8525603,
"eulerAngles":{"x":0.0,"y":0.0,"z":116.982468}},
...
```

No Authentication is required and this could potentially cause headaches for a GM and this may allow you to make changes for boards you are just a player on, so please do not abuse this. Only do this with either express permission of your GM or if you are in charge of the board.


### GetCreatureList

The method retrieves all creatures in an array of creature data. This is the primary interface for getting all creature data from the API.

#### Params: None
#### Returns: Array of CreatureData

Creature Data Example:
```
{
   "BoardAssetId":"7241df8e-f5b7-4ae6-a904-92a1662cab0a",
   "CreatureId":"a00a715c-4557-407c-8652-d81bb1f97353",
   "UniqueId":"a00a715c-4557-407c-8652-d81bb1f97353",
   "Position":{
      "x":3.530932,
      "y":1.00308585,
      "z":4.464703
   },
   "Rotation":{
      "w":0.522629,
      "x":0.0,
      "y":0.0,
      "z":0.8525603,
      "eulerAngles":{
         "x":0.0,
         "y":0.0,
         "z":116.982468
      }
   },
   "Alias":"knight",
   "AvatarThumbnailUrl":"",
   "Colors":[

   ],
   "Hp":{
      "Value":10.0,
      "Max":10.0
   },
   "Inventory":"{}",
   "Stat0":{
      "Value":10.0,
      "Max":10.0
   },
   "Stat1":{
      "Value":10.0,
      "Max":10.0
   },
   "Stat2":{
      "Value":10.0,
      "Max":10.0
   },
   "Stat3":{
      "Value":10.0,
      "Max":10.0
   },
   "TorchState":false,
   "ExplicitlyHidden":false
}
```

### GetPlayerControlledList

The method retrieves all creatures controlled by the current player.

#### Params: None
#### Returns: Array of CreatureData

### SelectPlayerControlledByAlias

Selects a creature by alias that the current player controls. This will select the mini and focus the camera.

#### Params: Alias
#### Returns: Error or Success Message

### SelectCreatureByCreatureId

Selects a creature by creatureId. This will select the mini and focus the camera.

#### Params:  CreatureId
#### Returns: Error or Success Message

### SelectNextPlayerControlled

Selects the next creature the player controls. If there are none nothing will happen. This will select the mini and focus the camera.

#### Params:  None
#### Returns: Error or Success Message

### SetCreatureHp

Sets the creature HP current / max.

#### Params:  CreatureId,currenthp,maxhp
#### Returns: Error or Success Message

### SetCreatureStat

Sets the creature stat current / max.

#### Params:  CreatureId,statNumber,currenthp,maxhp
#### Returns: Error or Success Message
#### Example Call
    The following example will set the creature ID stat 1 value to 20/20
    ```SetCreatureStat ABCD-12345-123,1,20,20```


### PlayEmote

Plays the provided emote on the mini.

#### Params:  CreatureId,EmoteName
    Current available values for emoteName:
    * TLA_Twirl
    * TLA_Action_Knockdown
    * TLA_Wiggle
    * TLA_MeleeAttack
#### Returns: Error or Success Message
#### Example Call
    ```PlayEmote ABCD-12345-123,TLA_Twirl```

### Knockdown

Shortcut method to perform a knockdown emote on a creatureId.

#### Params:  CreatureId
#### Returns: Error or Success Message
#### Example Call
    ```Knockdown ABCD-12345-123```

### SetCameraHeight

Sets the camera height to the provided value. If absolute is true it will set the height to exactly the value passed. If false it will move the height up or down based on the value passed from the current height.

#### Params:  height,absolute
#### Returns: Error or Success Message
#### Example Call
    ```SetCameraHeight 20,true```

### MoveCamera

Sets the camera position coordinates. If absolute is true it will set the position to exactly the value passed. If false it will move the camera relative to it's current position.

#### Params:  x,y,z,absolute
#### Returns: Error or Success Message
#### Example Call
    This will move the camera to location 10,0,10.
    ```MoveCamera 10,0,10,true```

### RotateCamera

Sets the camera rotation euler Y angle. If absolute is true it will set the rotation to exactly the value passed. If false it will move the camera relative to it's current position.

__Note:__ Currently this method is not animated and does not transition smoothly.

#### Params:  rotation,absolute
#### Returns: Error or Success Message
#### Example Call
    Rotate 45 degrees.
    ```RotateCamera 45,true```

### MoveCreature

Moves a creature a set number of steps (blocks). Important to remember that this method CAN be interrupted by sending more than one Move command to the same creature in less than 0.6 seconds. This means the behavior will be erratic. You should time moves within 0.6 second windows. You can send multiple movement commands at the same time however for different creatures. This will enable you to move a platoon or horde.

#### Params:  creatureId,direction,steps,pickup
    Available directions:
        forward,backwards,right,left
    Steps is an integer value of the number of steps to take (blocks to move across)
    Pickup picks the character off the board before moving them. This is important if they need to climb stairs, otherwise they will simply phase through.
#### Returns: Error or Success Message
#### Example Call
    This will move the creature forward by 3 steps sliding.
    ```MoveCreature ABCD-12345-123,forward,3,false```

### SetCameraHeight

Sets the height of the camera.

#### Params:  height, absolute
#### Returns: Error or Success Message
#### Example Call
    This will move the camera up by 2 units
    ```SetCameraHeight 2,false```

### RotateCamera

Rotates the camera, does not account for animation and should be managed if you need smooth rotation.

#### Params:  rotation, absolute
    Rotates the camera by a Euler angle defined by rotation, if absolute it will set it directly to that euler value.
#### Returns: Error or Success Message
#### Example Call
    ```RotateCamera 45,false```

### ZoomCamera

Zooms the camera by the amount specified.

#### Params:  zoom, absolute
#### Returns: Error or Success Message
#### Example Call
    ```ZoomCamera 5,false```

### TiltCamera

Tilts the camera by the amount specified.

#### Params:  tilt, absolute
#### Returns: Error or Success Message
#### Example Call
    ```TiltCamera 5,false```

### SayText **DEPRECATED**

This displays a bubble of text by the creature. **This is currently disabled until TaleSpire adds support back in for this feature.**

#### Params:  creatureId, text
#### Returns: Error or Success Message
#### Example Call
    ```SayText ABCD-1234,Arrrr, I am a pirate!```

### SetCustomStatName

Changes the stat name for all creatures to the new value. This change is not replicated across the network.

#### Params:  index, new name
#### Returns: Error or Success Message
#### Example Call
    This will set "Stat 1" to "Armor Class" on the popup rotary menu.
    ```SetCustomStatName 1, Armor Class```

### CreateSlab

Creates a slab at the location requested. This is the same as pasting a slab in but at a specific location.

#### Params:  x, y, z, slabtext
#### Returns: Error or Success Message
#### Example Call
    ```CreateSlab 0,0,0,ABCD==```

### GetSlabSize

Gets a slab size to make placement of other slabs easier. It will return a float 3 with the bounds of the slab.

#### Params:  x, y, z, slabtext
#### Returns: Error or ```{"x": 10, "y": 2, "z": 5}```
#### Example Call
    ```GetSlabSize ABCD==```

### GetCreatureAssets

Gets a list of all available creature assets that can be created.

#### Params:  None
#### Returns: Error or 
```
{
  "GUID": "ABCD-123",
  "boardAssetDesc": "Description",
  "boardAssetGroup": "Group Name",
  "boardAssetName": "Asset Name",
  "boardAssetType": "Asset Type",
  "seachString": "Search String",
}
```

#### Example Call
    ```GetCreatureAssets```


### AddCreature

Instantiates a creature at the position provided with relevant parameters.

**Note:** Should only be done with at least 300 ms between each call or things start to break

#### Params:  
  guid, 
  x, y, z,
  scale,
  alias,
  hpcurr, hpmax,
  stat1curr, stat1max,
  stat2curr, stat2max,
  stat3curr, stat3max,
  stat4curr, stat4max, 
  torch,
  hidden
#### Returns: Error or Success Message
#### Example Call
    ```AddCreature ABCDEF-1234,10,0,10,0,"TestRat",10,10,10,10,10,10,10,10,10,10,False,False


### KillCreature

Kills the specified creature (Delete instance)

#### Params:  guid, 

#### Returns: Error or Success Message
#### Example Call
    ```KillCreature ABCDEF-1234```


### GetCreatureStats

Gets all creature stats as an array HP, Stat 1, Stat 2, Stat 3, Stat 4 with Value and Max.

#### Params: CreatureID

#### Returns: Array of CreatureStats
```
{"ErrorMessage":null,"Message":"[{\"Value\":20.0,\"Max\":20.0},{\"Value\":10.0,\"Max\":10.0},{\"Value\":10.0,\"Max\":10.0},{\"Value\":10.0,\"Max\":10.0},{\"Value\":10.0,\"Max\":10.0},{\"Value\":0.0,\"Max\":0.0}]"}
```

#### Example Call
    ```GetCreatureStats ABCDEF-1234```
