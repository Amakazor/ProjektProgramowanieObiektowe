﻿using System;
using System.Security.Cryptography.X509Certificates;
using TankGame.Src.Actors.Projectiles;
using TankGame.Src.Data;

namespace TankGame.Src.Actors.Pawns.MovementControllers
{
    internal abstract class AIMovementController : MovementController
    {
        public AIMovementController(float delay, Pawn owner) : base(delay, owner)
        {
        }

        public override Direction DoAction(Direction currentDirection)
        {
            if (NextAction == null || CanSeePlayerInLine())
            {
                DecideOnNextAction();
            }

            if (CanSeePlayerInLine() && currentDirection != GetLineDirectionToPlayer(currentDirection))
            {
                if (CanDoAction())
                {
                    Direction newDirection = GetLineDirectionToPlayer(currentDirection);

                    if (Cooldown == 0)
                    {
                        Cooldown = Delay / 4;
                    }

                    return newDirection;
                }
            }

            return base.DoAction(currentDirection);
        }

        public bool CanSeePlayerInLine()
        {
            return !(GamestateManager.Instance.Player is null) && 
                ((GamestateManager.Instance.Player.Coords.X == Owner.Coords.X && Math.Abs(GamestateManager.Instance.Player.Coords.Y - Owner.Coords.Y) < 7) || 
                (GamestateManager.Instance.Player.Coords.Y == Owner.Coords.Y && Math.Abs(GamestateManager.Instance.Player.Coords.X - Owner.Coords.X) < 7));
        }

        public Direction GetLineDirectionToPlayer(Direction currentDirection)
        {
            if (CanSeePlayerInLine() && !(GamestateManager.Instance.Player is null))
            {
                if      (GamestateManager.Instance.Player.Coords.X > Owner.Coords.X) return Direction.Right;
                else if (GamestateManager.Instance.Player.Coords.X < Owner.Coords.X) return Direction.Left;
                else if (GamestateManager.Instance.Player.Coords.Y > Owner.Coords.Y) return Direction.Down;
                else if (GamestateManager.Instance.Player.Coords.Y < Owner.Coords.Y) return Direction.Up;
            }
            return currentDirection;
        }

        protected abstract void DecideOnNextAction();

        public override void Shoot(Direction direction)
        {
            base.Shoot(direction);
            new EnemyProjectile(Owner.Position, direction);
        }
    }
}