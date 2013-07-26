using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class Level : FContainer {
	
	// Sprites of all the tiles
	public List<FSprite> tiles = new List<FSprite>();
	
 	// The grids for the level's  layers.
    public int[,] Grid_Collision = new int[,] { { 0, 0 }, { 0, 0 } };

    protected int[][][] Grid_9;
    protected int[][][] Grid_8;
    protected int[][][] Grid_7;
    protected int[][][] Grid_6;
    protected int[][][] Grid_5;
    protected int[][][] Grid_4;
    protected int[][][] Grid_3;
    protected int[][][] Grid_2;
    protected int[][][] Grid_1;
		
	// Tileset broken up into Elements
	private Dictionary<string, FAtlasElement> _allElementsByName = new Dictionary<string, FAtlasElement>();
	
	public int tileWidth;
	public int tileHeight;
	
	public Vector2 mapSize;
	
	public Level(string name) : base() {
		shouldSortByZ = true;
		
		LoadTMX("TileMaps/" + name);
		
		int lengthI = Grid_Collision.GetLength(0);
        int lengthJ = Grid_Collision.GetLength(1);
		
		mapSize = new Vector2((lengthJ) * tileWidth , (lengthI) * tileHeight );
	}
	
	public void LoadTMX(string fileName) 
	{
		// load xml document
		TextAsset dataAsset = (TextAsset) Resources.Load (fileName, typeof(TextAsset));
		if(!dataAsset) {
			Debug.Log ("FTiledScene: Couldn't load the xml data from: " + fileName);
		}
		Resources.UnloadAsset(dataAsset);

		// parse xml string
		XmlTextReader mapReader = new XmlTextReader(new StringReader(dataAsset.text));
		mapReader.Read();
		
		// build tilesets
		while (mapReader.ReadToFollowing("tileset")) {
           	CreateElementsForTileMap(mapReader);
		}
		
		// reset xmlreader
		mapReader = new XmlTextReader(new StringReader(dataAsset.text));
		
		// build layer grids
		string layer = "";
        for (int u = 0; u < 10; u++) {
            mapReader.ReadToFollowing("layer");
            layer = mapReader[0];

            if (layer == "9")
                Grid_9 = loadSpriteLayer(mapReader, 9);
            else if (layer == "8")
                Grid_8 = loadSpriteLayer(mapReader, 8);
            else if (layer == "7")
                Grid_7 = loadSpriteLayer(mapReader, 7);
            else if (layer == "6")
                Grid_6 = loadSpriteLayer(mapReader, 6);
            else if (layer == "5")
                Grid_5 = loadSpriteLayer(mapReader, 5);
            else if (layer == "4")
                Grid_4 = loadSpriteLayer(mapReader, 4);
            else if (layer == "3")
                Grid_3 = loadSpriteLayer(mapReader, 3);
            else if (layer == "2")
                Grid_2 = loadSpriteLayer(mapReader, 2);
            else if (layer == "1")
                Grid_1 = loadSpriteLayer(mapReader, 1);
            else if (layer == "Collision")
                Grid_Collision = loadCollisionLayer(mapReader);
        }
		
		BuildTiles();
	}
	
	private static int[][][] loadSpriteLayer(XmlReader mapReader, int levelNum) {
        int[][][] layer;

        int width = Int32.Parse(mapReader[1]);
        int height = Int32.Parse(mapReader[2]);

        layer = new int[height][][];
        for (int i = 0; i < height; i++) {
            layer[i] = new int[width][];
            for (int j = 0; j < width; j++) {
                layer[i][j] = new int[4];
            }
        }

        for (int i = 0; i < height * width; i++) {
            mapReader.ReadToFollowing("tile");
            uint val = UInt32.Parse(mapReader[0]);
            int sprite = 0;

            int xflip = 0;
            xflip = (int)(val & (1 << 31));
            if (xflip != 0) {
                xflip = 1;

                if (sprite != 0) {
                    sprite = (int)sprite & ~(1 << 31);
                } else {
                    sprite = (int)val & ~(1 << 31);
                }
            }

            int yflip = 0;
            yflip = (int)(val & (1 << 30));
            if (yflip != 0) {
                yflip = 1;
                if (sprite != 0) {
                    sprite = (int)sprite & ~(1 << 30);
                } else {
                    sprite = (int)val & ~(1 << 30);
                }
            }

            int rotate = 0;
            rotate = (int)(val & (1 << 29));
            if (rotate != 0) {
                rotate = 1;
                if (sprite != 0) {
                    sprite = (int)sprite & ~(1 << 29);
                } else {
                    sprite = (int)val & ~(1 << 29);
                }
            }

            if (sprite == 0) {
                sprite = (int)val;
            }

            layer[i / width][i % width][0] = sprite;
            layer[i / width][i % width][1] = xflip;
            layer[i / width][i % width][2] = yflip;
            layer[i / width][i % width][3] = rotate;
        }
        return layer;
    }
	
	private static int[,] loadCollisionLayer(XmlReader mapReader) {
        int[,] layer;
        
        int width = Int32.Parse(mapReader[1]);
        int height = Int32.Parse(mapReader[2]);

        layer = new int[height, width];
        for (int i = 0; i < height * width; i++) {
            mapReader.ReadToFollowing("tile");
            uint col = UInt32.Parse(mapReader[0]);
            if (col != 0)
                layer[i / width, i % width] = (int)col;
            else
                layer[i / width, i % width] = 0;
        }
        return layer;
    }
	
	private void CreateElementsForTileMap(XmlReader tilemap) {
		
		XmlDocument doc = new XmlDocument();
        doc.LoadXml(tilemap.ReadOuterXml());
		
		XmlNode imageNode = doc.ChildNodes[0];
		
		float scaleInverse = Futile.resourceScaleInverse;
		
		//XMLNode imageNode = tilemap.children[0] as XMLNode;
		string sourceString = imageNode.Attributes["name"].Value.ToString();
		
		tileWidth = Convert.ToInt32(imageNode.Attributes["tilewidth"].Value.ToString());
		tileHeight = Convert.ToInt32(imageNode.Attributes["tileheight"].Value.ToString());
		
		int firstGID = Convert.ToInt32(imageNode.Attributes["firstgid"].Value.ToString());
		
		imageNode = imageNode.ChildNodes[0];
		int sourceWidth = Convert.ToInt32(imageNode.Attributes["width"].Value.ToString());
		int sourceHeight = Convert.ToInt32(imageNode.Attributes["height"].Value.ToString());
		
		int numRows = (sourceHeight / tileHeight);
		int numCols = (sourceWidth / tileWidth);
		int totalTiles = numRows * numCols;
		
		// load tileset image
		Futile.atlasManager.LoadImage("Atlases/" + sourceString);
		FAtlasElement origElement = Futile.atlasManager.GetElementWithName("Atlases/" + sourceString);
		
		Vector2 _textureSize = origElement.atlas.textureSize;
		
		int gid = firstGID;
		for(int i = 0; i < totalTiles; i++) {
			
			FAtlasElement element = new FAtlasElement();
			
			string name = "tile_" + gid;
			
			element.name = name;
			
			element.isTrimmed = origElement.isTrimmed;
			
			element.atlas = origElement.atlas;
			element.atlasIndex = i;
			
			//the uv coordinate rectangle within the atlas
			float frameX = (origElement.uvRect.x * _textureSize.x) - origElement.sourceRect.x + ((i % numCols) * tileWidth);
			float frameY = (-1 * ((origElement.uvRect.y * _textureSize.y) - _textureSize.y + origElement.sourceRect.height)) - origElement.sourceRect.y + ((i / numRows) * tileHeight);
			
			float frameW = tileWidth;
			float frameH = tileHeight;
			
			Rect uvRect = new Rect
			(
				frameX/_textureSize.x,
				((_textureSize.y - frameY - frameH)/_textureSize.y),
				frameW/_textureSize.x,
				frameH/_textureSize.y
			);
				
			element.uvRect = uvRect;
		
			element.uvTopLeft.Set(uvRect.xMin,uvRect.yMax);
			element.uvTopRight.Set(uvRect.xMax,uvRect.yMax);
			element.uvBottomRight.Set(uvRect.xMax,uvRect.yMin);
			element.uvBottomLeft.Set(uvRect.xMin,uvRect.yMin);

			//the source size is the untrimmed size
			element.sourcePixelSize.x = tileWidth;
			element.sourcePixelSize.y = tileHeight;

			element.sourceSize.x = element.sourcePixelSize.x * scaleInverse;	
			element.sourceSize.y = element.sourcePixelSize.y * scaleInverse;

			//this rect is the trimmed size and position relative to the untrimmed rect
			float rectX = origElement.sourceRect.x;
			float rectY = origElement.sourceRect.y;
			float rectW = tileWidth * scaleInverse;
			float rectH = tileHeight * scaleInverse;
			
			element.sourceRect = new Rect(rectX,rectY,rectW,rectH);
			
			_allElementsByName.Add(element.name, element);
			gid++;
		}
	}
	
	private FAtlasElement GetElementWithName (string elementName) {
		if (_allElementsByName.ContainsKey(elementName))
        {
            return _allElementsByName [elementName];
        } 
		
		throw new FutileException("Couldn't find element named '" + elementName + "'. \nUse Futile.atlasManager.LogAllElementNames() to see a list of all loaded elements names");
	}
		
	public void BuildTiles() {
        int spritenumber, xflip, yflip, rotation = 0;
        float rot = 0;
        int[][][] grid;
		
        for (int d = 1; d < 10; d++) {
            switch (d)
            {
                case 1: grid = Grid_1; break;
                case 2: grid = Grid_2; break;
                case 3: grid = Grid_3; break;
                case 4: grid = Grid_4; break;
                case 5: grid = Grid_5; break;
                case 6: grid = Grid_6; break;
                case 7: grid = Grid_7; break;
                case 8: grid = Grid_8; break;
                case 9: grid = Grid_9; break;
                default: grid = new int[0][][]; break;
            }
            
            for (int i = 0; i < grid.Length; i++) {
                for (int j = 0; j < grid[0].Length; j++) {
                    
                    spritenumber = grid[i][j][0];

                    if (spritenumber != 0) {						
						FSprite tile = new FSprite(GetElementWithName("tile_" + spritenumber));
						
                        rot = 0;
                        xflip = grid[i][j][1];
                        yflip = grid[i][j][2];
                        rotation = grid[i][j][3];

                        if (xflip == 1 && rotation == 1) {
                            rot = 90f;
                        } else if (yflip == 1 && rotation == 1) {
                            rot = -90f;
                        } else {
	                        if (xflip == 1) tile.scaleX = -1f;
	                        if (yflip == 1) tile.scaleY = -1f;
						}

                        tile.rotation = rot;

                       	tile.SetPosition(new Vector2(j * tileWidth + tileWidth/2, -i * tileHeight - tileHeight/2));
						tile.sortZ = d * 10;
						
						tiles.Add(tile);
						AddChild(tile);
                    }
                }
            }
        }
    }

	// Update is called once per frame
	public void Update () {
		
		int lengthI = Grid_Collision.GetLength(0); // Y
        int lengthJ = Grid_Collision.GetLength(1); // X

        int drawMin_X = Math.Abs((int)(this.x / tileWidth)) - 1;
        int drawMax_X = drawMin_X + (int)(Futile.screen.width / tileWidth) + 2;
        int drawMin_Y = (int)((this.y - Futile.screen.height) / tileHeight) - 1;
        int drawMax_Y = drawMin_Y + (int)(Futile.screen.height / tileHeight) + 2;

        if (drawMin_X < 0) drawMin_X = 0;
        if (drawMax_X > lengthJ) drawMax_X = lengthJ;
        if (drawMin_Y < 0) drawMin_Y = 0;
        if (drawMax_Y > lengthI) drawMax_Y = lengthI;
		
		if (Futile.screen.width > mapSize.x) drawMin_X = 0;
		
		// only draw visible tiles
		foreach (FSprite e in this.tiles) {
			int j = (int)(e.x / tileWidth);
			int i = (int)(-1 * (e.y / tileHeight));
			
			if (j >= drawMin_X && j <= drawMax_X && i >= drawMin_Y && i <= drawMax_Y) {
				e.isVisible = true;
			} else {
				e.isVisible = false;
			}
		}
	}
}
