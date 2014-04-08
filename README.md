coms-437-trashdroids
====================

A 3D Asteroids-style game built in XNA for ComS 437 at Iowa State University

Attributions
------------

Particle System:
	The particle system used in this software is distributed by Microsoft under the Ms-PL license (http://www.microsoft.com/en-us/openness/licenses.aspx). The original, unmodified works for the Particle System can be found at: http://xbox.create.msdn.com/downloads/?id=496&filename=Particles3DSample_4_0.zip
	
	The following files fall under this agreement:
		Trashdroids/Trashdroids/Particles/ExplosionParticleSystem.cs
		Trashdroids/Trashdroids/Particles/MissileParticleSystem.cs
		Trashdroids/Trashdroids/Particles/ParticleSettings.cs
		Trashdroids/Trashdroids/Particles/ParticleSystem.cs
		Trashdroids/Trashdroids/Particles/ParticleVertex.cs
		Trashdroids/Trashdroids/Particles/PowerupParticleSystem.cs
		Trashdroids/Trashdroids/TrashdroidsContent/particles/ParticleEffect.fx
     
"Droid" model:
    Source:               http://www.turbosquid.com/FullPreview/Index.cfm/ID/499274
    Author:               gentlemenk
     
Missile model:
    Source:               http://www.turbosquid.com/FullPreview/Index.cfm/ID/671861
    Author:               max web
     
Sound effect: crash_metal
    Source:               https://www.freesound.org/people/Halleck/sounds/121657/
    Author:               Halleck
     
Sound effect: ingame_ambient 
    Source:               http://freesound.org/people/Diboz/sounds/211683/
    Author:               Diboz
 
Sound effect: explode_small
    Source:               https://www.freesound.org/people/Omar%20Alvarado/sounds/199725/
    Author:               Omar Alvarado
     
Sound effect: explode_large
    Source:               https://www.freesound.org/people/ryansnook/sounds/110113/
    Author:               ryansnook
     
Sound effect: engine
    Source:               http://www.freesound.org/people/qubodup/sounds/146770/
    Author:               qubodup
     
Camera framework:
    Source:               Taken from Greg's demo code, source unknown

Introduction
------------

It is the distant future, the year 2000. During the robotic uprising of the late 90's, the last remaining humans fled from their robot overlords and launched into space aboard a small space station. Now, as the colony continues to thrive in a remote corner of the solar system, they face one great challenge: garbage. If they jettison their garbage into the void of space, the robots could use their, erm, magic... garbage scanners... or something... to detect the humans. This challenge forced the humans to create unique trash receptacles, inside of which trash must somehow be destroyed. Clearly, the only logical solution is to blow it up with missiles. The receptacles were filled with tiny, trash-destroying droids and thus, Trashdroids were born.


Objective
---------

Single Player:
	Destroy at least 50% of the asteroids on the field. After 3 collisions
	with other objects, your ship is destroyed and you lose.

Multiplayer:
	Destroy the enemy ship! Ships are only damaged by missiles. Powerups may
	also be collected to enhance your ship's capabilities.

<!---
Controls
--------
╔═════════════════════╦═════════════╦════════════════════╗
║       Control       ║  Keyboard   ║      Gamepad       ║
╠═════════════════════╬═════════════╬════════════════════╣
║ Ship Movement       ║             ║                    ║
║   Forward/Backward  ║ W/S         ║ L Stick Up/Down    ║
║   Strafe Left/Right ║ A/D         ║ L Stick Left/Right ║
║   Yaw               ║ Q/E         ║ Left/Right Bumpers ║
║   Pitch             ║ Up/Down     ║ R Stick Up/Down    ║
║   Roll              ║ Left/Right  ║ R Stick Left/Right ║
║   Stabilizer        ║ L Shift     ║ Left Trigger       ║
║   Fire weapon       ║ Space       ║ Right Trigger      ║
║ Menu Controls       ║             ║                    ║
║   Menu Navigation   ║ Up/Down     ║ D-Pad Up/Down      ║
║   Menu Selection    ║ Space/Enter ║ Start/A            ║
║ Toggle Fullscreen   ║ Backspace   ║ N/A                ║
╚═════════════════════╩═════════════╩════════════════════╝
-->


