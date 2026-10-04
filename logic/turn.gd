class_name Turn extends RefCounted

var puzzle: PuzzleState

var phase1_events: Array[Dictionary]
var phase2_events: Array[Dictionary]
var phase3_events: Array[Dictionary]

func _init(p: PuzzleState, evs1: Array[Dictionary] = [], evs2: Array[Dictionary] = [], evs3: Array[Dictionary] = []):
	puzzle = p
	phase1_events = evs1
	phase2_events = evs2
	phase3_events = evs3
