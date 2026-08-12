class_name Mod extends RefCounted

func _load(_tree : SceneTree) -> void:
	var ModLoader = load("res://GodotMonoModLoader/GodotMonoModLoader.cs")
	var mod_loader = ModLoader.new()
	mod_loader.LoadDllFromZip(ProjectSettings.globalize_path("user://Mods/MoreSimAreaOptions.zip"), "MoreSimAreaOptions/MoreSimAreaOptions.dll", true)