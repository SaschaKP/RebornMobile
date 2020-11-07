

using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO.Resources
{
    internal class MultiLoader : UOFileLoader
    {
        private UOFile _file;
        private int _itemOffset;
        private DataReader _reader;

        private MultiLoader()
        {

        }

        private static MultiLoader _instance;
        public static MultiLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MultiLoader();
                }

                return _instance;
            }
        }



        public int Count { get; private set; }
        public UOFile File => _file;
        public bool IsUOP { get; private set; }
        public int Offset => _itemOffset;


        public override unsafe Task Load()
        {
            return Task.Run(() =>
            {
                string uopPath = UOFileManager.GetUOFilePath("MultiCollection.uop");

                if (Client.IsUOPInstallation && System.IO.File.Exists(uopPath))
                {
                    Count = Constants.MAX_MULTI_DATA_INDEX_COUNT;
                    _file = new UOFileUop(uopPath, "build/multicollection/{0:D6}.bin");
                    Entries = new UOFileIndex[Count];
                    _reader = new DataReader();
                    IsUOP = true;
                }
                else
                {
                    string path = UOFileManager.GetUOFilePath("multi.mul");
                    string pathidx = UOFileManager.GetUOFilePath("multi.idx");

                    if (System.IO.File.Exists(path) && System.IO.File.Exists(pathidx))
                    {
                        _file = new UOFileMul(path, pathidx, Constants.MAX_MULTI_DATA_INDEX_COUNT, 14);
                        Count = _itemOffset = Client.Version >= ClientVersion.CV_7090 ? sizeof(MultiBlockNew) + 2 : sizeof(MultiBlock);
                    }
                }

                _file.FillEntries(ref Entries);

            });
        }

        public override void ClearResources()
        {
            _file?.Dispose();
            _file = null;
            _instance = null;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    ref struct MultiBlock
    {
        public ushort ID;
        public short X;
        public short Y;
        public short Z;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    ref struct MultiBlockNew
    {
        public ushort ID;
        public short X;
        public short Y;
        public short Z;
        public ushort Flags;
        public uint Unknown;
    }
}