class_name Mod extends RefCounted

func _load(_tree : SceneTree) -> void:
	var ModLoader = load("res://GodotMonoModLoader/GodotMonoModLoader.cs")
	var mod_loader = ModLoader.new()
	# mod_loader.LoadDllFromPath(ProjectSettings.globalize_path("user://Mods/DirectShipInventory/0Harmony.dll"), false)
	mod_loader.LoadDllFromZip(ProjectSettings.globalize_path("user://Mods/0Harmony.zip"), "0Harmony/0Harmony.dll", false)
	mod_loader.LoadDllFromZip(ProjectSettings.globalize_path("user://Mods/0Harmony.zip"), "0Harmony/Harmony.dll", true)
	