﻿using System;
using TankGame.Src.Actors;
using TankGame.Src.Actors.Shaders;
using TankGame.Src.Actors.Weathers;
using TankGame.Src.Data.Gamestate;
using TankGame.Src.Data.Sounds;
using TankGame.Src.Data.Textures;
using TankGame.Src.Events;

namespace TankGame.Src.Data.Weathers
{
    internal class WeatherController : ITickable, IDisposable
    {
        public static readonly int WeatherMinimalTime = 30;
        public static readonly int WeatherMaximalTime = 61;
        public static readonly float WeatherMinimalIntensity = 0.5F;
        public static readonly float WeatherMaximalIntensity = 3F;
        private Weather Weather { get; set; }
        public float CurrentWeatherTime { get; private set; }
        public string WeatherType => Weather is null ? "clear" : Weather.Type;
        public AnimationType AnimationType;

        public WeatherController()
        {
            RegisterTickable();
            CurrentWeatherTime = 0;

            AnimationType = WeatherShader.CanUseShader ? AnimationType.Shaded : AnimationType.Animated;

            GetNewWeather();
        }

        public void SetWeather(string type, float weatherTime)
        {
            if (Weather != null) Weather.Dispose();
            Weather = null;
            MusicManager.Instance.StopMusic();

            CurrentWeatherTime = weatherTime;
            float intensity = (float)((GamestateManager.Instance.Random.NextDouble() * (WeatherMaximalIntensity - WeatherMinimalIntensity)) + WeatherMinimalIntensity);
            Weather = type switch
            {
                "clear" => null,
                "rain" => new Weather(TextureManager.Instance.GetTexture(TextureType.Weather, "rain"), 1.15F, MusicType.Rain, intensity, AnimationType, "rain"),
                "snow" => new Weather(TextureManager.Instance.GetTexture(TextureType.Weather, "snow"), 1.3F,  MusicType.Snow, intensity, AnimationType, "snow"),
                _ => null,
            };
        }

        public float GetSpeedModifier()
        {
            return Weather != null ? Weather.SpeedModifier : 1;
        }

        public void Tick(float deltaTime)
        {
            CurrentWeatherTime -= deltaTime;
            if (CurrentWeatherTime <= 0) GetNewWeather();
        }

        private void GetNewWeather()
        {
            if (Weather != null) Weather.Dispose();
            Weather = null;
            CurrentWeatherTime = GamestateManager.Instance.Random.Next(WeatherMinimalTime, WeatherMaximalTime);
            float intensity = (float)((GamestateManager.Instance.Random.NextDouble() * (WeatherMaximalIntensity - WeatherMinimalIntensity)) + WeatherMinimalIntensity);
            Weather = GamestateManager.Instance.Random.Next(1, 4) switch
            {
                1 => null,
                2 => new Weather(TextureManager.Instance.GetTexture(TextureType.Weather, "rain"), 1.15F, MusicType.Rain, intensity, AnimationType, "rain"),
                3 => new Weather(TextureManager.Instance.GetTexture(TextureType.Weather, "snow"), 1.3F, MusicType.Snow, intensity, AnimationType, "snow"),
                _ => null,
            };

            if (Weather == null) MusicManager.Instance.StopMusic();
        }

        public void RegisterTickable()
        {
            MessageBus.Instance.PostEvent(MessageType.RegisterTickable, this, new EventArgs());
        }

        public void UnregisterTickable()
        {
            MessageBus.Instance.PostEvent(MessageType.UnregisterTickable, this, new EventArgs());
        }

        public void Dispose()
        {
            if (Weather != null) Weather.Dispose();
            UnregisterTickable();
        }
    }
}