using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "Scriptable Objects/Map")]
public class Map : ScriptableObject
{
    public List<Vector2> frontLinePath = new List<Vector2>();
    public List<Vector2> backLinePath = new List<Vector2>();
    public List<Vector2> frontLineSentPath = new List<Vector2>();
    public List<Vector2> backLineSentPath = new List<Vector2>();
    public List<MapEndRegion> frontLineEnds = new List<MapEndRegion>();
    public List<MapEndRegion> backLineEnds = new List<MapEndRegion>();
    public List<MapEndRegion> frontLineSentEnds = new List<MapEndRegion>();
    public List<MapEndRegion> backLineSentEnds = new List<MapEndRegion>();
}

[Serializable]
public class MapEndRegion
{
    public Vector2 center;
    public Vector2 bounds;
}