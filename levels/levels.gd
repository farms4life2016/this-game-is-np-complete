class_name Levels extends RefCounted

static var lvl1json := {
	"name": "Hello World",
	"version": "ASCII schema v1.0",
	"base": 4,
	"height": 3,
	"drift": "right",
	"ascii": [
		"X   O-E",
		"|   | |",
		"O-O-O-B",
		"| | |  ",
        "P-O-L  "
	],
	"definitions": {
		"O": {
			"item": "none",
			"enemies": [],
			"player": false,
			"exit": false
		},
		"P": {
			"player": true
		},
		"E": {
			"exit": true
		},
		"B": {
			"enemies": [
				{
					"type": "stationary",
					"facing": "left"
				},
				{
					"type": "sniper",
					"facing": "up"
				}
			]
		},
		"L": {
			"item": "left"
		},
		"X": {
			"item": "zero"
		}
	}
}

static var lvl1 := Parser.parse_json(lvl1json)

static var LEVELS := [lvl1]
