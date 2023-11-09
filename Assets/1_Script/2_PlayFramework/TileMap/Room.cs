using RandomDungeon.PlayFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Room
{

    public TileMap      tileMap;

    public RectInt  tileIndexRect   = new RectInt();
    public int      Index           = -1;

    public virtual void Start()
    {
    }

    public virtual void Enter()
    {

    }

    public virtual void Exit()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void PlayingUpdate()
    {

    }

}

public class EmptyRoom : Room
{
}

public class MonsterRoom : Room
{

    public override void Start()
    {
    }

}

public class PlayerStartRoom : Room
{

    public override void Start()
    {
    }

}


public class BossRoom : Room
{

    public override void Start()
    {
    }

}

