﻿using SFML.System;
using System;
using System.Collections.Generic;
using System.Text;
using TankGame.Src.Actors.Pawns.Enemies;
using TankGame.Src.Actors.Pawns.MovementControllers;

namespace TankGame.Src.Actors.Pawns
{
    internal static class EnemyFactory
    {
        private static readonly Dictionary<string, Func<double, Enemy, List<Vector2i>, AIMovementController>> AIMCTypes = new Dictionary<string, Func<double, Enemy, List<Vector2i>, AIMovementController>>
        {
            {"random", (delay, owner, patrolRoute) =>  new RandomAIMovementController     (delay, owner)},
            {"chase",  (delay, owner, patrolRoute) =>  new ChaseAIMovementController      (delay, owner)},
            {"stand",  (delay, owner, patrolRoute) =>  new StandGroundAIMovementController(delay, owner)},
            {"patrol", (delay, owner, patrolRoute) =>  new PatrolAIMovementController     (delay, owner, patrolRoute)}
        };

        private static readonly Dictionary<string, Func<Vector2f, Enemy>> EnemyTypes = new Dictionary<string, Func<Vector2f, Enemy>>
        {
            { "light",  (Coords) => new LightTank (Coords*64, new Vector2f(64, 64))},
            { "medium", (Coords) => new MediumTank(Coords*64, new Vector2f(64, 64))},
            { "heavy",  (Coords) => new HeavyTank (Coords*64, new Vector2f(64, 64))}
        };

        public static Enemy CreateEnemy(Vector2i coords, string enemyType, string AIMCType, List<Vector2i> patrolRoute, int health)
        {
            if (EnemyTypes.ContainsKey(enemyType) && AIMCTypes.ContainsKey(AIMCType))
            {
                Enemy newEnemy = EnemyTypes[enemyType](new Vector2f(coords.X, coords.Y));
                if (health != 0) newEnemy.Health = health;
                
                newEnemy.MovementController = AIMCTypes[AIMCType](newEnemy switch{LightTank _ => 0.75, MediumTank _ => 1.5, HeavyTank _ => 2.25, _ => 1}, newEnemy, patrolRoute);

                return newEnemy;
            }
            else throw new Exception();
        }

        public static Enemy CreateEnemy(EnemySpawnData enemySpawnData, int health) => CreateEnemy(enemySpawnData.Coords, enemySpawnData.Type, enemySpawnData.AimcType, enemySpawnData.PatrolRoute, health);
    }
}
