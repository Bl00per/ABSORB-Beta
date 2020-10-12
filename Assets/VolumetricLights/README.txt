********************************
*       VOLUMETRIC LIGHTS      *
*      Created by Kronnect     *   
*          README FILE         *
********************************

Requirements & Setup
--------------------
1) Volumetric Lights works only with Universal Rendering Pipeline (v7.1.8 or later)
2) Make sure you have Universal RP package imported in the project before using Volumetric Lights.
3) Make sure you have a Universal Rendering Pipeline asset assigned to Project Settings / Graphics. There's a URP sample asset in Demo/URP Pipeline Settings folder.


Demo Scene
----------
There's a demo scene which lets you quickly check if Volumetric Lights is working correctly in your project.


Documentation
-------------
Please read the documentation (PDF) for additional instructions and options.


Support
-------
* Support: contact@kronnect.me
* Website-Forum: http://kronnect.me
* Twitter: @KronnectGames


Future updates
--------------
All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Volumetric Lights will be eventually available on the Asset Store.


Version history
---------------

Current version
- API: added "settings" property to allow modifications of individual lights without affecting a shared profile
- [Fix] Fixed particles not appearing immediately when disabling/reenabling light

v2.3 8-Sep-2020
- Added Shadow Auto Toggle and Shadow Visible Distance to optimize shadow rendering based on light distance to camera
- Added further optimizations for dust particles and shadows when not visible in frustum


v2.2 22-Aug-2020
- Added support for orthographic camera
- Added "Raymarching Min Step" parameter which can improve performance
- [Fix] Fixed VR issue due to URP not setting inverted VP matrices correctly
- [Fix] Fixed rare clipping issue on Android due to lack of floating point precision 

v2.1.4 28-Jun-2020
- Improved fit for rect light shadow map

v2.1.3 23-Jun-2020
- Particle system user modifications are now preserved
- Improved bluenoise sampling
- Added "Attenuation Mode" option (Simple and Quadratic modes supported with ability to specify quadratic coeficients)
- Added "Blend Mode" : PreMultiply
- Dust Particles: added "Distance Attenuation" and "Auto Toggle" options based on distance
- Improved profile editor UI
- [Fix] Fixed clipping issue on some platforms

v1.4 14-JUN-2020
- Enhanced blue noise jitter operator

v1.3 9-MAY-2020
- Added blue noise option
- Changed default render queue to 3101 for improved compatibility with Volumetric Fog & Mist 2

v1.2 April 2020
- Shadow occlusion optimizations
- [Fix] Fixed VR issues

v1.1 April 2020
- Added warning to inspector if Depth Texture option is not enabled in URP settings override in Project Settings / Quality
- API: added ScheduleShadowUpdate() method to issue a manual shadow update when shadow bake at start is enabled

v1.0 March / 2020
First release