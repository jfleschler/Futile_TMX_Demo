using UnityEngine;
using System;

public class FAnimation {
	
	protected string _name;
	protected string _elementName;
	protected int _delay; // in milliseconds
	protected bool _looping;
	protected int _width;
	protected int _height;
	protected int _row;
	protected int _startCol;
	protected int _numFrames;
	
	
	public FAnimation (string givenName, string elementName, int width, int height, int row, int startCol, int numFrames, int delayInMilliseconds, bool loop=false) 
	{
		_elementName = elementName;
		_name = givenName;
		_width = width;
		_height = height;
		_row = row;
		_startCol = startCol;
		_numFrames = numFrames;
		_delay = delayInMilliseconds;
		_looping = loop;
	}
	
	public string name 
	{
		get { return _name; }
	}
	
	public string elementName 
	{
		get { return _elementName; }
	}
	
	public int delay 
	{
		get { return _delay; }
		set { _delay = value; }
	}
	
	public bool looping 
	{
		get { return _looping; }
		set { _looping = value; }
	}
	
	public int width 
	{
		get { return _width; }
	}
	
	public int height 
	{
		get { return _height; }
	}
	
	public int row 
	{
		get { return _row; }
	}
	
	public int startCol 
	{
		get { return _startCol; }
	}
	
	public int numFrames 
	{
		get { return _numFrames; }
	}
	
}