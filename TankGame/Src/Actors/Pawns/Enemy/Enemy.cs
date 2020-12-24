﻿using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankGame.Src.Actors.Pawns.Enemies
{
    abstract internal class Enemy : Pawn
    {
        public Enemy(Vector2f position, Vector2f size, Texture texture) : base(position, size, texture)
        {
        }
    }
}