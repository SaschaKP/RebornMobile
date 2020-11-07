

using System;

using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.IO.Audio.MP3Sharp;
using ClassicUO.Utility.Logging;

using Microsoft.Xna.Framework.Audio;

namespace ClassicUO.IO.Audio
{
    internal class UOMusic : Sound
    {
        private const int NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK = 0x8000; // 32768 bytes, about 0.9 seconds
        private readonly bool m_Repeat;
        private readonly byte[] m_WaveBuffer = new byte[NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK];
        private bool m_Playing;
        private MP3Stream m_Stream;
        private string _path;

        public UOMusic(int index, string name, bool loop)
            : base(name, index)
        {
            m_Repeat = loop;
            m_Playing = false;
            Channels = AudioChannels.Stereo;
            Delay = 0;
            _path = System.IO.Path.Combine(Settings.GlobalSettings.UltimaOnlineDirectory, Client.Version > ClientVersion.CV_5090 ? $"Music/Digital/{Name}.mp3" : $"music/{Name}.mp3"); 
        }

        private string Path => _path;

        public void Update()
        {
            // sanity - if the buffer empties, we will lose our sound effect. Thus we must continually check if it is dead.
            // OnBufferNeeded(null, null);
        }

        protected override byte[] GetBuffer()
        {
            try
            {
                if (m_Playing && _sound_instance != null)
                {
                    int bytesReturned = m_Stream.Read(m_WaveBuffer, 0, m_WaveBuffer.Length);

                    if (bytesReturned != NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK)
                    {
                        if (m_Repeat)
                        {
                            m_Stream.Position = 0;
                            m_Stream.Read(m_WaveBuffer, bytesReturned, m_WaveBuffer.Length - bytesReturned);
                        }
                        else
                        {
                            if (bytesReturned == 0)
                                Stop();
                        }
                    }

                    return m_WaveBuffer;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            
            Stop();

            return null;
        }

        protected override void OnBufferNeeded(object sender, EventArgs e)
        {
            if (m_Playing)
            {
                if (_sound_instance == null)
                {
                    Stop();
                    return;
                }

                //while (_sound_instance.PendingBufferCount < 3)
                {
                    byte[] buffer = GetBuffer();

                    if (_sound_instance.IsDisposed || buffer == null)
                        return;

                    _sound_instance.SubmitBuffer(buffer);
                }
            }
        }

        protected override void BeforePlay()
        {
            if (m_Playing) Stop();

            try
            {
                if (m_Stream != null)
                {
                    m_Stream.Close();
                    m_Stream = null;
                }

                m_Stream = new MP3Stream(Path, NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK);
                Frequency = m_Stream.Frequency;

                m_Playing = true;
            }
            catch
            {
                // file in use or access denied.
                m_Playing = false;
            }
        }

        protected override void AfterStop()
        {
            if (m_Playing)
            {
                m_Playing = false;
                m_Stream?.Close();
                m_Stream = null;
            }
        }
    }
}