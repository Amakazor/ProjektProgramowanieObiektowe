﻿using SFML.System;
using TankGame.Src.Actors.Pawns.MovementControllers;
using TankGame.Src.Data;

namespace TankGame.Src.Actors.Pawns.Enemies
{
    internal class HeavyTank : Enemy
    {
        public HeavyTank(Vector2f position, Vector2f size, int health = 3) : base(position, size, TextureManager.Instance.GetTexture(TextureType.Pawn, "enemy3"), health)
        {
        }
    }
}