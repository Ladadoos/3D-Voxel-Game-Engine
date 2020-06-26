## 3D Voxel Game

![](Images/demo3.gif)

A 3D voxel game (very much inspired by Minecraft) using OpenGL, C# and GLSL. 
The focus of this project is to figure out and solve the different challenges that come with making a voxel engine/game like Minecraft: how to represent the world and the different blocks, how to generate the world, how to interact with it, how to update the world (after an interaction), how (smooth) lighting works etc.

It features:
- Procedurally generated world with biomes using perlin noise.
- Multiplayer with a client-server architecture.
- Multithreading for networking, terrain generation and tesselation.
- Different biomes each with different terrain generation and decoration properties.
- Lighting system (with the option of smooth or flat shading) supporting different colored blocks.
- Baked ambient occlusion.
- Customizable skydome shader for simulating day/night cycles.
- Interactions with the world (breaking blocks, placing blocks, interacting with blocks).
- Different types of blocks like normal blocks (dirt, stone for example) and more complex blocks like plants (wheat, sugar cane for example).
- Basic UI rendering system built with canvasses holding different UI components. 
- Rendering system with flexibility to define how a block should be rendered to avoid rendering unnecessary parts.
- Support for different texture packs.
- Collision detection and response.

Features that could be added/looked at:
- Save and load worlds to/from file.
- Make the game a lot more mod friendly by making it more data-driven.
- Add support to generate large(r) structures like villages.
- Add basic mob spawning and AI.
- Add caves.
- Add items and a survival mode.

![](Images/demo1.gif)
![](Images/demo2.gif)
![](Images/demo4.gif)
