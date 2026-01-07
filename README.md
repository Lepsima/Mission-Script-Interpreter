# Mission Script Interpreter
A custom language for quickly designing the behaviour of NPC's and objects in game missions.
<br>

It's currently very WIP and specific to my needs and may not be suitable for other projects.

#

The goal is to provide easy control of what the NPC does at any point, reacting to events, alternative routes, mission checkpoints, etc.
It compacts C# routines and functions into something simple, while avoiding visual languages and rebuild times.
<br>

It also allows players to change the behaviour of the NPC's with a decent balance of ease and functionality.

# Example
Place the contents inside a text script (extension ".stcr", optional)<br>
DEBUG_ScriptTester.cs (inside Unity folder) shows how to run scripts<br>
<b>!This demo scipt contains custom commands not present in the source code!</b>
```javascript
STCR v0

### START
approach @Waypoint("meetup")
formation @Squadron("leader")
target NULL
speed 600


### MISSION_CHANGE
set $Target NULL
onEvent $Target OnTargetChange

checkpoint Loop
set $Target @Scan(&TARGET, &MAX_RANGE, &ENEMY)
wait 2
jump Loop

func OnTargetChange
    approach $Target
    formation NULL
    target $Target
    
    speed $Target
    afterburner true
    
    weapon &MAX_RANGE
    targetPriority &TARGET
func END
```

# Custom commands/operators/functions Template
This repository doesn't include any of the commands i use for my project, only the essentials needed to run the script are present. <br>
Custom made Commands / Conditional operators / External functions are needed and can be added as shown here:
```csharp
namespace STCR {
public partial class Script {
	static Script() {
		AddCommands();
		AddOperators();
		AddExternal();
	}

	private static void AddCommands() {
		// This command can be used as "speed [arg0]"
		commands.Add("speed", (script, args) => {
			Debug.Log("Speed set to: " + args[0]);
		});

		// This command can be used as "moveXY [arg0] [arg1]"
		commands.Add("moveXY", (script, args) => {
			Debug.Log("Moving to: X" + args[0] + " Y:" + args[1]);
		});
	}

	private static void AddOperators() {
		// Example usage "if (100 greater 50)"
		boolOperations.Add("greater", (a, b) => {
			// A and B are "object" types, cast them to the class you're using
			float numberA = float.Parse(a, CultureInfo.InvariantCulture)
			float numberB = float.Parse(b, CultureInfo.InvariantCulture)

			// Always return bool value
			return numberA > numberB;
		});

		// Example usage "if (100 smaller 50)"
		boolOperations.Add("smaller", (a, b) => {
			// A and B are "object" types, cast them to the class you're using
			float numberA = float.Parse(a, CultureInfo.InvariantCulture)
			float numberB = float.Parse(b, CultureInfo.InvariantCulture)

			// Always return bool value
			return numberA < numberB;
		});
	}

	private static void AddExternal() {
		// Returns a string, args is of type "object[]", may contain reference values
		// All variables have been evaluated, if you typed "$VariableA" it's value will replace it 
		externalFunctions.Add("@GetFoo", (script, args) => {
			Debug.Log("GetFoo Called!");
			return "foo";
		});
	}
}
}
```

### Using the commands
This example makes no sense, it's just for sytax demonstration
```javascript
STCR v0

### CUSTOM_COMMANDS
set $PosX 1000
set $PosY 6200

speed 500
moveXY $PosX $PosY

if ($PosX greater $PosY)
  set $PosX -100
  moveXY $PosX $PosY
endif
```
