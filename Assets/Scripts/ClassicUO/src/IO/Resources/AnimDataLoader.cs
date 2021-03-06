

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using ClassicUO.Utility;

namespace ClassicUO.IO.Resources
{
    internal class AnimDataLoader : UOFileLoader
    {
        private UOFileMul _file;

        private AnimDataLoader()
        {

        }

        private static AnimDataLoader _instance;
        public static AnimDataLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AnimDataLoader();
                }

                return _instance;
            }
        }


        public override Task Load()
        {
            return Task.Run(() => 
            {
                string path = UOFileManager.GetUOFilePath("animdata.mul");

                if (File.Exists(path))
                {
                    _file = new UOFileMul(path);
                }
            });
        }

        public UOFile AnimDataFile => _file;
        
        public override void ClearResources()
        {
            _file?.Dispose();
            _file = null;
            _instance = null;
        }


      

        public AnimDataFrame2 CalculateCurrentGraphic(ushort graphic)
        {
            IntPtr address = _file.StartAddress;

            if (address != IntPtr.Zero)
            {
                IntPtr addr = address + (graphic * 68 + 4 * ((graphic >> 3) + 1));

                //Stopwatch sw = Stopwatch.StartNew();
                //for (int i = 0; i < 2000000; i++)
                //{
                //    AnimDataFrame pad = Marshal.PtrToStructure<AnimDataFrame>(addr);
                //}

                //Console.WriteLine("Marshal: {0} ms", sw.ElapsedMilliseconds);

                //sw.Restart();
                //for (int i = 0; i < 2000000; i++)
                //{
                //    
                //}

                //Console.WriteLine("Custom: {0} ms", sw.ElapsedMilliseconds);

                //if (pad.FrameCount == 0)
                //{
                //    pad.FrameCount = 1;
                //    pad.FrameData[0] = 0;
                //}

                //if (pad.FrameInterval == 0)
                //    pad.FrameInterval = 1;
                AnimDataFrame2 a = UnsafeMemoryManager.ToStruct<AnimDataFrame2>(addr);

                return a;
            }

            return default;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct AnimDataFrame2
    {
        public fixed sbyte FrameData[64];
        public byte Unknown;
        public byte FrameCount;
        public byte FrameInterval;
        public byte FrameStart;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct AnimDataFrame
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly sbyte[] FrameData;
        public readonly byte Unknown;
        public readonly byte FrameCount;
        public readonly byte FrameInterval;
        public readonly byte FrameStart;
    }
}