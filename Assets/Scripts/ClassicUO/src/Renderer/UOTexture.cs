

using System.Collections.Generic;

using ClassicUO.IO.Resources;
using ClassicUO.Utility.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer
{
    internal class UOTexture32 : Texture2D
    {
        private uint[] _data;

        public UOTexture32(int width, int height) : base(Client.Game.GraphicsDevice, width, height, false, SurfaceFormat.Color)
        {
            Ticks = Time.Ticks + 3000;
        }

        public long Ticks { get; set; }
        public uint[] Data => _data;

        public void PushData(uint[] data, bool keepData = false)
        {
            if (keepData)
            {
                _data = data;
            }

            SetData(data);
        }

        public bool Contains(int x, int y, bool pixelCheck = true)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                if (!pixelCheck)
                    return true;

                if (UnityTexture == null)
                    return false;
                
                int pos = y * Width + x;
                return GetDataAtPos(pos) != 0;
            }

            return false;
        }
        
        //Used for Contains checks in texture using Unity's own texture data, instead of keeping a copy of the data in _data field
        private uint GetDataAtPos(int pos)
        {
            //The index calculation here is the same as in Texture2D.SetData
            var width = Width;
            int x = pos % width;
            int y = pos / width;
            y *= width;
            var index = y + (width - x - 1);
            
            var data = (UnityTexture as UnityEngine.Texture2D).GetRawTextureData<uint>();
            //We reverse the index because we had already reversed it in Texture2D.SetData
            var reversedIndex = data.Length - index - 1;
            if (reversedIndex < data.Length && reversedIndex >= 0)
            {
                return data[reversedIndex];
            }

            return 0;
        }
    }

    internal class FontTexture : UOTexture32
    {
        public FontTexture(int width, int height, int linescount, RawList<WebLinkRect> links) : base(width, height)
        {
            LinesCount = linescount;
            Links = links;
        }

        public int LinesCount { get; set; }

        public RawList<WebLinkRect> Links { get; }
    }

    internal class AnimationFrameTexture : UOTexture32
    {
        public AnimationFrameTexture(int width, int height) : base(width, height)
        {
        }

        public short CenterX { get; set; }

        public short CenterY { get; set; }
    }

    internal class ArtTexture : UOTexture32
    {
        public ArtTexture(int offsetX, int offsetY, int offsetW, int offsetH, int width, int height) : base(width, height)
        {
            ImageRectangle = new Rectangle(offsetX, offsetY, offsetW, offsetH);
        }

        public ArtTexture(Rectangle rect, int width, int height) : base(width, height)
        {
            ImageRectangle = rect;
        }

        public Rectangle ImageRectangle;
    }
}