

using System;

using ClassicUO.Configuration;
using ClassicUO.Game;
using Microsoft.Xna.Framework.Audio;

namespace ClassicUO.IO.Audio
{
    internal class UOSound : Sound
    {
        protected override uint DistortionFix => 500;
        private readonly byte[] _waveBuffer;

        public UOSound(string name, int index, byte[] buffer)
            : base(name, index)
        {
            _waveBuffer = buffer;
            Delay = (uint) ((buffer.Length - 32) / 88.2f);
        }

        public int X, Y;
        public bool CalculateByDistance { get; set; }

        protected override void OnBufferNeeded(object sender, EventArgs e)
        {
            // not needed.
            //if (World.InGame && X >= 0 && Y >= 0 && CalculateByDistance)
            //{
            //    int distX = Math.Abs(X - World.Player.X);
            //    int distY = Math.Abs(Y - World.Player.Y);
            //    int distance = Math.Max(distX, distY);

            //    float volume = ProfileManager.Current.SoundVolume / Constants.SOUND_DELTA;
            //    float distanceFactor = 0.0f;

            //    if (distance >= 1)
            //    {
            //        float volumeByDist = volume / (World.ClientViewRange + 1);
            //        distanceFactor = volumeByDist * distance;
            //    }

            //    if (distance > World.ClientViewRange)
            //    {
            //        Stop();
            //        Dispose();
            //        return;
            //    }

            //    if (ProfileManager.Current == null || !ProfileManager.Current.EnableSound || !Client.Game.IsActive && !ProfileManager.Current.ReproduceSoundsInBackground)
            //        volume = 0;

            //    if (Client.Game.IsActive)
            //    {
            //        if (!ProfileManager.Current.ReproduceSoundsInBackground)
            //            volume = ProfileManager.Current.SoundVolume / Constants.SOUND_DELTA;
            //    }
            //    else if (!ProfileManager.Current.ReproduceSoundsInBackground)
            //        volume = 0;

            //    VolumeFactor = distanceFactor;
            //    Volume = volume;
            //}
        }

        protected override byte[] GetBuffer()
        {
            return _waveBuffer;
        }
    }
}