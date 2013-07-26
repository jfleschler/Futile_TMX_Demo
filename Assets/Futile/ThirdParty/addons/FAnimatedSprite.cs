using UnityEngine;
using System;
using System.Collections.Generic;

public class FAnimatedSprite : FSprite {
	
	private List<FAtlasElement> _elements = new List<FAtlasElement>();
	
	private Dictionary<string, FAtlasElement> _allElementsByName = new Dictionary<string, FAtlasElement>();	

	protected bool _pause = false;
	protected float _time = 0;
	protected int _currentFrame = 0;
		
	protected List<FAnimation> _animations;
	
	protected FAnimation _currentAnim;
	
	public FAnimatedSprite (FAnimation defaultAnim) : base() 
	{	
		_animations = new List<FAnimation>();
		addAnimation(defaultAnim);
		
		ListenForUpdate(Update);
				
		// default to first frame, no animation
		Init(FFacetType.Quad, this.GetElementWithName(defaultAnim.elementName + "_" + defaultAnim.name + "_0"),1); // expects individual frames, in convention of NAME_#.EXT
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}
	
	// Update is called once per frame
	virtual public void Update()
	{
		if (_currentAnim != null && !_pause) {
			_time += Time.deltaTime;
		
			while (_time > (float)_currentAnim.delay / 1000.0f) { // looping this way will skip frames if needed
				_currentFrame++;
				if (_currentFrame >= _currentAnim.numFrames) {
					if (_currentAnim.looping) {
						_currentFrame = 0;
					} else {
						_currentFrame = _currentAnim.numFrames - 1;
					}
					
					// send Signal if it exists
					//_currentAnim.checkFinished();
				}
				
				//Redraw(true,true);
				element = this.GetElementWithName(_currentAnim.elementName + "_" + _currentAnim.name + "_" + _currentFrame);
				
				_time -= (float)_currentAnim.delay / 1000.0f;
			}
		}
	}
	
	public void addAnimation(FAnimation anim) 
	{
		_animations.Add(anim);
		
		CreateElementsForAnimation(anim);
		
		if (_currentAnim == null) {
			_currentAnim = anim;
			_currentFrame = anim.startCol;
			_pause = false;
			isVisible = true;
		}
	}
	
	public void play(string animName, bool forced=false) 
	{
		// check if we are given the same animation that is currently playing
		if (_currentAnim.name == animName) {
			if (forced) {
				// restart at first frame
				_currentFrame = 0;
				_time = 0;
				
				// redraw
				element = this.GetElementWithName(_currentAnim.elementName + "_" + _currentAnim.name + "_" + _currentFrame);
			}
			
			return;
		}
		
		// find the animation with the name given, no change if not found
		foreach (FAnimation anim in _animations) {
			if (anim.name == animName) {
				_currentAnim = anim;
				_currentFrame = 0;
				_time = 0;
				
				// force redraw to first frame
				element = this.GetElementWithName(_currentAnim.elementName + "_" + _currentAnim.name + "_" + _currentFrame);
				
				break;
			}
		}
	}
	
	public void pause(bool forced=false) 
	{
		if (forced) {
			_pause = true;
		} else {
			_pause = !_pause;
		}
	}
	
	public void start() {
		_pause = false;
	}
	
	public FAnimation currentAnim {
		get { return _currentAnim; }
	}
	
	public int currentFrame 
	{
		get {
			return _currentFrame;
		}
	}
	
	public bool isPaused 
	{
		get { return _pause; }
	}
	
	private void CreateElementsForAnimation(FAnimation anim)
	{
		float scaleInverse = Futile.resourceScaleInverse;
		FAtlasElement origElement = Futile.atlasManager.GetElementWithName(anim.elementName);
		
		Vector2 _textureSize = origElement.atlas.textureSize;
		
		for(int i = 0; i < anim.numFrames; i++)
		{
			FAtlasElement element = new FAtlasElement();
			
			string name = anim.elementName + "_" + anim.name + "_" + i;
			
			element.name = name;
			
			element.isTrimmed = origElement.isTrimmed;
			
			element.atlas = origElement.atlas;
			element.atlasIndex = i;
			
			//the uv coordinate rectangle within the atlas
			float frameX = (origElement.uvRect.x * _textureSize.x) - origElement.sourceRect.x + ((anim.startCol + i) * anim.width);
			float frameY = (-1 * ((origElement.uvRect.y * _textureSize.y) - _textureSize.y + origElement.sourceRect.height)) - origElement.sourceRect.y + (anim.row * anim.height);
			
			float frameW = anim.width;
			float frameH = anim.height;
			
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
			element.sourcePixelSize.x = anim.width;
			element.sourcePixelSize.y = anim.height;

			element.sourceSize.x = element.sourcePixelSize.x * scaleInverse;	
			element.sourceSize.y = element.sourcePixelSize.y * scaleInverse;

			//this rect is the trimmed size and position relative to the untrimmed rect
			float rectX = origElement.sourceRect.x;
			float rectY = origElement.sourceRect.y;
			float rectW = anim.width * scaleInverse;
			float rectH = anim.height * scaleInverse;
			
			element.sourceRect = new Rect(rectX,rectY,rectW,rectH);
			
			_elements.Add(element);
			_allElementsByName.Add(element.name, element);
		}
	}
	
	public FAtlasElement GetElementWithName (string elementName)
	{
		if (_allElementsByName.ContainsKey(elementName))
        {
            return _allElementsByName [elementName];
        } 
		
		throw new FutileException("Couldn't find element named '" + elementName + "'. \nUse Futile.atlasManager.LogAllElementNames() to see a list of all loaded elements names");
	}
}
