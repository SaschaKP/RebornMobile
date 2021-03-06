

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.IO.Resources;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO
{
    internal static class UOFileManager
    {
        public static string GetUOFilePath(string file)
        {
            var filePath = Path.Combine(Settings.GlobalSettings.UltimaOnlineDirectory, file);
            
            //NOTE: Potential fix file not found issues on iOS due to filesystem case sensitivity
            //If the file with the given name doesn't exist, check for it with alternative casing
            if (File.Exists(filePath) == false)
            {
                var firstChar = file[0];
                if (char.IsUpper(firstChar))
                {
                    file = char.ToLowerInvariant(firstChar) + file.Substring(1);
                }
                else
                {
                    file = char.ToUpperInvariant(firstChar) + file.Substring(1);
                }
                var newFilePath = Path.Combine(Settings.GlobalSettings.UltimaOnlineDirectory, file);
                if (File.Exists(newFilePath))
                {
                    return newFilePath;
                }
            }

            return filePath;
        }


        public static void Load()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // List<Task> tasks = new List<Task>
            // {
            //     AnimationsLoader.Instance.Load(),
            //     AnimDataLoader.Instance.Load(),
            //     ArtLoader.Instance.Load(),
            //     MapLoader.Instance.Load(),
            //     ClilocLoader.Instance.Load(Settings.GlobalSettings.ClilocFile),
            //     GumpsLoader.Instance.Load(),
            //     FontsLoader.Instance.Load(),
            //     HuesLoader.Instance.Load(),
            //     TileDataLoader.Instance.Load(),
            //     MultiLoader.Instance.Load(),
            //     SkillsLoader.Instance.Load().ContinueWith(t => ProfessionLoader.Instance.Load()),
            //     TexmapsLoader.Instance.Load(),
            //     SpeechesLoader.Instance.Load(),
            //     LightsLoader.Instance.Load(),
            //     SoundsLoader.Instance.Load(),
            //     MultiMapLoader.Instance.Load()
            // };
            //
            // if (!Task.WhenAll(tasks).Wait(TimeSpan.FromSeconds(10)))
            // {
            //     Log.Panic("Loading files timeout.");
            // }
            
            AnimationsLoader.Instance.Load().Wait();
            AnimDataLoader.Instance.Load().Wait();
            ArtLoader.Instance.Load().Wait();
            MapLoader.Instance.Load().Wait();
            ClilocLoader.Instance.Load(Settings.GlobalSettings.ClilocFile).Wait();
            GumpsLoader.Instance.Load().Wait();
            FontsLoader.Instance.Load().Wait();
            HuesLoader.Instance.Load().Wait();
            TileDataLoader.Instance.Load().Wait();
            MultiLoader.Instance.Load().Wait();
            SkillsLoader.Instance.Load().Wait();
            ProfessionLoader.Instance.Load().Wait();
            TexmapsLoader.Instance.Load().Wait();
            SpeechesLoader.Instance.Load().Wait();
            LightsLoader.Instance.Load().Wait();
            SoundsLoader.Instance.Load().Wait();
            MultiMapLoader.Instance.Load().Wait();


            UOFileMul verdata = Verdata.File;

            bool use_verdata = Client.Version < ClientVersion.CV_500A || (verdata != null && verdata.Length != 0 && Verdata.Patches.Length != 0);

            if (!Settings.GlobalSettings.UseVerdata && use_verdata)
            {
                Settings.GlobalSettings.UseVerdata = use_verdata;
            }

            Log.Trace($"Use verdata.mul: {(Settings.GlobalSettings.UseVerdata ? "Yes" : "No")}");

            if (Settings.GlobalSettings.UseVerdata)
            {
                if (verdata != null && Verdata.Patches.Length != 0)
                {
                    Log.Info(">> PATCHING WITH VERDATA.MUL");

                    for (int i = 0; i < Verdata.Patches.Length; i++)
                    {
                        ref UOFileIndex5D vh = ref Verdata.Patches[i];
                        //Log.Info($">>> patching  FileID: {vh.FileID}  -  BlockID: {vh.BlockID}");

                        if (vh.FileID == 0)
                        {
                            MapLoader.Instance.PatchMapBlock(vh.BlockID, vh.Position);
                        }
                        else if (vh.FileID == 4)
                        {
                            ushort id = (ushort) (vh.BlockID - Constants.MAX_LAND_DATA_INDEX_COUNT);

                            if (id < ArtLoader.Instance.Entries.Length)
                                ArtLoader.Instance.Entries[id] = new UOFileIndex(verdata.StartAddress,
                                                                                      (uint) verdata.Length,
                                                                                      vh.Position,
                                                                                      (int) vh.Length,
                                                                                      0);
                        }
                        else if (vh.FileID == 12)
                        {
                            GumpsLoader.Instance.Entries[vh.BlockID] = new UOFileIndex(verdata.StartAddress,
                                                                                       (uint) verdata.Length,
                                                                                       vh.Position,
                                                                                       (int) vh.Length,
                                                                                      0,
                                                                                      (short) (vh.GumpData >> 16),
                                                                                       (short) (vh.GumpData & 0xFFFF));
                        }
                        else if (vh.FileID == 14 && vh.BlockID < MultiLoader.Instance.Count)
                        {
                            MultiLoader.Instance.Entries[vh.BlockID] = new UOFileIndex(verdata.StartAddress,
                                                                                       (uint) verdata.Length,
                                                                                       vh.Position,
                                                                                       (int) vh.Length,
                                                                                       0);
                        }
                        else if (vh.FileID == 16 && vh.BlockID < SkillsLoader.Instance.SkillsCount)
                        {
                            SkillEntry skill = SkillsLoader.Instance.Skills[(int) vh.BlockID];

                            if (skill != null)
                            {
                                DataReader reader = new DataReader();
                                reader.SetData(verdata.StartAddress, verdata.Length);

                                skill.HasAction = reader.ReadBool();
                                skill.Name = reader.ReadASCII((int) (vh.Length - 1));

                                reader.ReleaseData();
                            }
                        }
                        else if (vh.FileID == 30)
                        {
                            verdata.Seek(0);
                            verdata.Skip((int) vh.Position);

                            if (vh.Length == 836)
                            {
                                int offset = (int) (vh.BlockID * 32);

                                if (offset + 32 > TileDataLoader.Instance.LandData.Length)
                                    continue;

                                verdata.ReadUInt();

                                for (int j = 0; j < 32; j++)
                                {
                                    ulong flags;

                                    if (Client.Version < ClientVersion.CV_7090)
                                    {
                                        flags = verdata.ReadUInt();
                                    }
                                    else
                                    {
                                        flags = verdata.ReadULong();
                                    }

                                    TileDataLoader.Instance.LandData[offset + j] = new LandTiles(flags, verdata.ReadUShort(), verdata.ReadASCII(20));
                                }
                            }
                            else if (vh.Length == 1188)
                            {
                                int offset = (int) ((vh.BlockID - 0x0200) * 32);

                                if (offset + 32 > TileDataLoader.Instance.StaticData.Length)
                                    continue;

                                verdata.ReadUInt();

                                for (int j = 0; j < 32; j++)
                                {
                                    ulong flags;

                                    if (Client.Version < ClientVersion.CV_7090)
                                    {
                                        flags = verdata.ReadUInt();
                                    }
                                    else
                                    {
                                        flags = verdata.ReadULong();
                                    }

                                    TileDataLoader.Instance.StaticData[offset + j] =
                                        new StaticTiles(flags,
                                                        verdata.ReadByte(),
                                                        verdata.ReadByte(),
                                                        verdata.ReadInt(),
                                                        verdata.ReadUShort(),
                                                        verdata.ReadUShort(),
                                                        verdata.ReadUShort(),
                                                        verdata.ReadByte(),
                                                        verdata.ReadASCII(20));
                                }
                            }
                        }
                        else if (vh.FileID == 32)
                        {
                            if (vh.BlockID < HuesLoader.Instance.HuesCount)
                            {
                                VerdataHuesGroup group = Marshal.PtrToStructure<VerdataHuesGroup>(verdata.StartAddress + (int) vh.Position);

                                HuesLoader.Instance.HuesRange[vh.BlockID].Header = group.Header;

                                for (int j = 0; j < 8; j++)
                                {
                                    Array.Copy(group.Entries[j].ColorTable,
                                               HuesLoader.Instance.HuesRange[vh.BlockID].Entries[j].ColorTable,
                                               32);
                                }
                            }
                        }
                        else if (vh.FileID != 5 && vh.FileID != 6)
                        {
                            Log.Warn($"Unused verdata block\tFileID: {vh.FileID}\tBlockID: {vh.BlockID}");
                        }
                    }

                    Log.Info("<< PATCHED.");
                }
            }
         

            Log.Trace( $"Files loaded in: {stopwatch.ElapsedMilliseconds} ms!");
            stopwatch.Stop();
        }

        internal static void MapLoaderReLoad(MapLoader newloader)
        {
            MapLoader.Instance?.Dispose();
            MapLoader.Instance = newloader;
        }
    }
}