class_name Mod extends RefCounted

func _load(_tree : SceneTree) -> void:
	var ModLoader = load("res://GodotMonoModLoader/GodotMonoModLoader.cs")
	var mod_loader = ModLoader.new()
	# mod_loader.LoadDllFromPath(ProjectSettings.globalize_path("user://Mods/DirectShipInventory/0Harmony.dll"), false) # Loaded as a separate mod
	mod_loader.LoadDllFromZip(ProjectSettings.globalize_path("user://Mods/DirectShipInventory.zip"), "DirectShipInventory/DirectShipInventory.dll", true)
	