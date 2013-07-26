Futile_TMX_Demo
===============

This is a demo project showing how to render TMX files using Futile in Unity3d.


Files of Note  
-------------

**Scripts/Engine/Level.cs**  
Pass the TMX filename in the constructor and it parses the relevant information.  Takes care of splitting up the tilesheet into FSprites and stores them in an internal list for display purposes.  Each tile is assigned a name "tile_" + gid (where gid is the grid id from Tiled.)

The update method of this class sets all tiles that are not visible on the screen to not draw.


**Scripts/Player/Player.cs**  
This is the player class that makes use of the "Collision" layer defined in the TMX.


Futile Addons
-------------
Located in the "Futile\ThirdParty\addons" folder I have created two classes **FAnimation** and **FAnimatedSprite**.  You can see how they are used in the **Player** class but basically they allow me to use one image for animations instead of breaking them out into separate pngs.


Extra Goodies
-------------

I am lazy.  That being said I was getting sick of having to rename my TMX files to .txt or .xml so I created a custom importer class.  This is located at "Editor\CustomImporter."  Now whenever I drag a new file into my project it will check to see if the extension is ".tmx", if so it automatically renames it to ".txt" and will overwrite any previous file of the same name.


Disclaimer
----------
This code is provided as is.  Use at your own risk, blah, blah, blah.
Feel free to use any code in future projects, but please give credit where credit is due :)

Special thanks to Matt Rix for the awesome Futile framework!
