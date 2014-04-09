coms-437-trashdroids
====================

A 3D Asteroids-style game built in XNA for ComS 437 at Iowa State University

Attributions
------------

Particle System:
* The particle system used in this software is distributed by Microsoft under the [Ms-PL license.] (http://www.microsoft.com/en-us/openness/licenses.aspx)
* The original, unmodified works for the Particle System can be found [here.] (http://xbox.create.msdn.com/downloads/?id=496&filename=Particles3DSample_4_0.zip)
* The following files fall under this agreement:
    * Trashdroids/Trashdroids/Particles/ExplosionParticleSystem.cs
    * Trashdroids/Trashdroids/Particles/MissileParticleSystem.cs
    * Trashdroids/Trashdroids/Particles/ParticleSettings.cs
    * Trashdroids/Trashdroids/Particles/ParticleSystem.cs
    * Trashdroids/Trashdroids/Particles/ParticleVertex.cs
    * Trashdroids/Trashdroids/Particles/PowerupParticleSystem.cs
    * Trashdroids/Trashdroids/TrashdroidsContent/particles/ParticleEffect.fx

BEPU Physics:
* All BEPU Physics content is licensed under the [Apache License 2.0.] (http://bepuphysics.codeplex.com/license)
* All content in the following directories fall under this agreement:
    * Trashdroids/BEPUphysics
    * Trashdroids/BEPUphysicsDrawer
    * Trashdroids/BEPUutilities
    * Trashdroids/ConversionHelper

"Droid" model:
* Source: http://www.turbosquid.com/FullPreview/Index.cfm/ID/499274
* Author: gentlemenk
* License: Turbosquid Royalty Free License (http://support.turbosquid.com/entries/31030006-Royalty-Free-License)

Missile model:
* Source: http://www.turbosquid.com/FullPreview/Index.cfm/ID/671861
* Author: max web
* License: Turbosquid Royalty Free License (http://support.turbosquid.com/entries/31030006-Royalty-Free-License)

Sound effect: crash_metal
* Source: https://www.freesound.org/people/Halleck/sounds/121657/
* Author: Halleck
* License: Attribution (http://creativecommons.org/licenses/by/3.0/)

Sound effect: ingame_ambient
* Source: http://freesound.org/people/Diboz/sounds/211683/
* Author: Diboz
* License: Creative Commons 0 (http://creativecommons.org/publicdomain/zero/1.0/)

Sound effect: explode_small
* Source: https://www.freesound.org/people/Omar%20Alvarado/sounds/199725/
* Author: Omar Alvarado
* License: Attribution Noncommercial (http://creativecommons.org/licenses/by-nc/3.0/)

Sound effect: explode_large
* Source: https://www.freesound.org/people/ryansnook/sounds/110113/
* Author: ryansnook
* License: Attribution (http://creativecommons.org/licenses/by/3.0/)

Sound effect: engine
* Source: http://www.freesound.org/people/qubodup/sounds/146770/
* Author: qubodup
* License: Creative Commons 0 (http://creativecommons.org/publicdomain/zero/1.0/)

Introduction
------------

It is the distant future, the year 2000. During the robotic uprising of the late 90's, the last remaining humans fled from their robot overlords and launched into space aboard a small space station. Now, as the colony continues to thrive in a remote corner of the solar system, they face one great challenge: garbage. If they jettison their garbage into the void of space, the robots could use their, erm, magic... garbage scanners... or something... to detect the humans. This challenge forced the humans to create unique trash receptacles, inside of which trash must somehow be destroyed. Clearly, the only logical solution is to blow it up with missiles. The receptacles were filled with tiny, trash-destroying droids and thus, Trashdroids were born.


Objective
---------

Single Player: Destroy at least 50% of the asteroids on the field. After 3 collisions with other objects, your ship is destroyed and you lose.

Multiplayer: Destroy the enemy ship! Ships are only damaged by missiles. Powerups may also be collected to enhance your ship's capabilities.

<!---
Controls
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
Controls
--------

<table><tbody><tr><th>Control</th><th>Keyboard</th><th>Gamepad</th></tr><tr><td>Ship Movement</td><td> </td><td> </td></tr><tr><td>  Forward/Backward</td><td>W/S</td><td>L Stick Up/Down</td></tr><tr><td>  Strafe Left/Right</td><td>A/D</td><td>L Stick Left/Right</td></tr><tr><td>  Yaw</td><td>Q/E</td><td>Left/Right Bumpers</td></tr><tr><td>  Pitch</td><td>Up/Down</td><td>R Stick Up/Down</td></tr><tr><td>  Roll</td><td>Left/Right</td><td>R Stick Left/Right</td></tr><tr><td>  Stabilizer</td><td>L Shift</td><td>Left Trigger</td></tr><tr><td>  Fire weapon</td><td>Space</td><td>Right Trigger</td></tr><tr><td>Menu Controls</td><td> </td><td> </td></tr><tr><td>  Menu Navigation</td><td>Up/Down</td><td>D-Pad Up/Down</td></tr><tr><td>  Menu Selection</td><td>Space/Enter</td><td>Start/A</td></tr><tr><td>Toggle Fullscreen</td><td>Backspace</td><td>N/A</td></tr></tbody></table>


