﻿using SFML.Graphics;
using SFML.System;
using TankGame.Src.Actors;

namespace TankGame.Src.Gui.RenderComponents
{
    internal class SpriteComponent : IRenderComponent
    {
        protected IRenderable Actor { get; set; }
        private Sprite Sprite { get; }
        private Vector2f Size { get; set; }

        public SpriteComponent(Vector2f position, Vector2f size, IRenderable actor, Texture texture, Color color)
        {
            Actor = actor;

            Sprite = new Sprite(texture)
            {
                Position = position,
                Color = color
            };

            Size = size;

            SetScaleFromSize(size);
        }

        private void SetScaleFromSize(Vector2f size)
        {
            Sprite.Scale = size.X != 0 && size.Y != 0 ? new Vector2f(size.X / 64, size.Y / 64) : new Vector2f(1, 1);
        }

        public bool IsPointInside(Vector2f point)
        {
            Vector2f position = Sprite.Position;
            Vector2f size = Size;

            return (position.X <= point.X) && (position.X + size.X >= point.X) && (position.Y <= point.Y) && (position.Y + size.Y >= point.Y);
        }

        public void SetSize(Vector2f size)
        {
            Size = size;
            SetScaleFromSize(size);
        }

        public void SetPosition(Vector2f position)
        {
            Sprite.Position = position;
        }

        public IRenderable GetActor()
        {
            return Actor;
        }

        Drawable IRenderComponent.GetShape()
        {
            return Sprite;
        }
    }
}