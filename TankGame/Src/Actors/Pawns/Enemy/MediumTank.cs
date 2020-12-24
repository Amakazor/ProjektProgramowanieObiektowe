﻿using SFML.System;
using TankGame.Src.Actors.Pawns.MovementControllers;
using TankGame.Src.Data;

namespace TankGame.Src.Actors.Pawns.Enemies
{
    internal class MediumTank : Enemy
    {
        public MediumTank(Vector2f position, Vector2f size) : base(position, size, TextureManager.Instance.GetTexture(TextureType.Pawn, "enemy2"))
        {
            SetHealth(2);
        }
    }
}