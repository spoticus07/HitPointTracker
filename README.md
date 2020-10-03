# HitPoint Tracker API Tool

## Tech Stack:
- netcoreapp 3.1
- Microsoft EntityFrameworkCore 3.1.8
- Docker 1.6.0
- Remote - WSL 0.44.5
- C# 1.23.2

## Default HTTP Connection (Docker):
http://localhost:5000

## APIs Endpoints
### Post:
#### End Point: **/api/HitPointTracker**
Loaded Character into App Memory for use in App.
- Header: N/A
- Body: (JSON) See APIBodyExamples/CharacterSheet.JSON
- Response: json repesenting Character Data in Memory (In memory database using EF)

Example: http://localhost:5000/api/HitPointTracker

Body: {
    "name": "Defaulty",
    "level": 3,
	"health":{
		"hitPointsMax":35,
		"hitPointsCurrent":15,
		"hitPointsMaxMod":0,
		"temporaryHitPoints":0
	},...
}
### Delete:
#### End Point: **/api/HitPointTracker/**
Removes Character from App Memory.
- Header: Character's Name (PrimaryKey)
- Body: N/A
- Response: None on success, Error code on failure.

Example: http://localhost:5000/api/HitPointTracker/Defaulty
### Get:
#### End Point: **/api/HitPointTracker/FindCharacter/**
Returns full character data in JSON by character's name
- Header: Character's Name (PrimaryKey)
- Body: N/A
- Response: json repesenting Character Data in Memory (In memory database using EF)

Example: http://localhost:5000/api/HitPointTracker/FindCharacter/Defaulty
#### End Point: **/api/HitPointTracker/CharacterList**
Returns a list of character names representing each character in the app.
- Header: N/A
- Body: N/A
- Response: An array of Character names.

Example: http://localhost:5000/api/HitPointTracker/CharacterList
#### End Point: **/api/HitPointTracker/CharacterHealth/**
Returns the current hit points of the indicated character.
- Header: Character's Name (PrimaryKey)
- Body: N/A
- Response: integer value of current hit points.

Example: http://localhost:5000/api/HitPointTracker/CharacterHealth/Defaulty
### Put:
#### End Point: **/api/HitPointTracker/RollMethod/**
Sets the method used to roll hit points for a character (defaults to "round").
- Header: Character's Name (PrimaryKey)
- Body: (string) RollMethod ("roll", "round")
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/RollMethod/Defaulty

Body: "roll"
#### End Point: **/api/HitPointTracker/Heal/**
Recovers a characters current hit points.
- Header: Character's Name (PrimaryKey)
- Body: (int) amount of healing
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/Heal/Defaulty

Body: 10
#### End Point: **/api/HitPointTracker/Damage/**
Applies damage to a character.
- Header: Character's Name (PrimaryKey)
- Body: (json) See APIBodyExamples/Damage.JSON
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/Damage/Defaulty

Body: {
    "type":"slashing",
    "value": 5
    }
#### End Point: **/api/HitPointTracker/MaxHitPointMod/**
Adjust characters max hit points with temporary modifiers.
- Header: Character's Name (PrimaryKey)
- Body: (int) amount to adjust Max HP (negative to reduce, positive to increase)
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/MaxHitPointMod/Defaulty

Body: 6
#### End Point: **/api/HitPointTracker/ResetMaxHitPoints/**
Resets character's max hit point modifieds to base max hit points.
- Header: Character's Name (PrimaryKey)
- Body: N/A
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/ResetMaxHitPoints/Defaulty
#### End Point: **/api/HitPointTracker/ResetHitPoints/**
Resets character's current hit points back to full.
- Header: Character's Name (PrimaryKey)
- Body: N/A
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/ResetHitPoints/Defaulty
#### End Point: **/api/HitPointTracker/UpdateTempHitPoints/**
Adds temporary hit points to a character if applicable.
- Header: Character's Name (PrimaryKey)
- Body: (int) amount of temporary hit points
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/UpdateTempHitPoints/Defaulty

Body: 8
#### End Point: **/api/HitPointTracker/ResetTempHitPoints/**
Resets a characters temporary hit points to zero.
- Header: Character's Name (PrimaryKey)
- Body: N/A
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/ResetTempHitPoints/Defaulty
#### End Point: **/api/HitPointTracker/RerollHitPoints/**
Rerolls a character's max hit points based on current class level and roll method.
- Header: Character's Name (PrimaryKey)
- Body: N/A
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/RerollHitPoints/Defaulty
#### End Point: **/api/HitPointTracker/LevelUp/**
Adds an additional level of an existing indicated class and rolls hit points.
- Header: Character's Name (PrimaryKey)
- Body: (string) class name ("wizard","fighter","druid", exc..)
- Response: N/A

Example: http://localhost:5000/api/HitPointTracker/LevelUp/Defaulty

Body: "wizard"
