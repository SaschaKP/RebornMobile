

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ClassicUO.Configuration;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;

using CUO_API;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SDL2;

namespace ClassicUO.Network
{
    internal unsafe class Plugin
    {
        private static readonly List<Plugin> _plugins = new List<Plugin>();
        public static List<Plugin> Plugins => _plugins;
        private readonly string _path;
        public string PluginPath => _path;

        private const string hardcodedInternalAssistantPath = "internal_assistant";

        private OnCastSpell _castSpell;
        private OnGetPacketLength _getPacketLength;
        private OnGetPlayerPosition _getPlayerPosition;
        private OnGetStaticImage _getStaticImage;
        private OnGetUOFilePath _getUoFilePath;
        private OnClientClose _onClientClose;
        private OnConnected _onConnected;
        private OnDisconnected _onDisconnected;
        private OnFocusGained _onFocusGained;
        private OnFocusLost _onFocusLost;

        private OnHotkey _onHotkeyPressed;
        private OnInitialize _onInitialize;
        private OnMouse _onMouse;
        private OnUpdatePlayerPosition _onUpdatePlayerPosition;
        private OnPacketSendRecv _recv, _send, _onRecv, _onSend;
        private RequestMove _requestMove;
        private OnSetTitle _setTitle;
        private OnTick _tick;
        private OnPacketSendRecv_new  _onRecv_new, _onSend_new;
        private OnPacketSendRecv_new_intptr _recv_new, _send_new;
        private OnDrawCmdList _draw_cmd_list;
        private OnWndProc _on_wnd_proc;
        private OnGetStaticData _get_static_data;
        private OnGetTileData _get_tile_data;
        private OnGetCliloc _get_cliloc;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
        private delegate void OnInstall(void* header);

        [return: MarshalAs(UnmanagedType.I1)] 
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool OnPacketSendRecv_new(byte[] data, ref int length);

        [return: MarshalAs(UnmanagedType.I1)] 
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
        private delegate bool OnPacketSendRecv_new_intptr(IntPtr data, ref int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
        private delegate int OnDrawCmdList([Out] out IntPtr cmdlist, ref int size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
        private delegate int OnWndProc(SDL.SDL_Event* ev);

        [return: MarshalAs(UnmanagedType.I1)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool OnGetStaticData(int index, ref ulong flags,
                                              ref byte weight,
                                              ref byte layer,
                                              ref int count,
                                              ref ushort animid,
                                              ref ushort lightidx,
                                              ref byte height,
                                              ref string name);

        [return: MarshalAs(UnmanagedType.I1)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool OnGetTileData(int index, ref ulong flags,
                                            ref ushort textid,
                                            ref string name);

        [return: MarshalAs(UnmanagedType.I1)] 
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
        private delegate bool OnGetCliloc(int cliloc, [MarshalAs(UnmanagedType.LPStr)] string args, bool capitalize, [Out] [MarshalAs(UnmanagedType.LPStr)] out string buffer);


        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string name);



        struct PluginHeader
        {
            public int ClientVersion;
            public IntPtr HWND;
            public IntPtr OnRecv;
            public IntPtr OnSend;
            public IntPtr OnHotkeyPressed;
            public IntPtr OnMouse;
            public IntPtr OnPlayerPositionChanged;
            public IntPtr OnClientClosing;
            public IntPtr OnInitialize;
            public IntPtr OnConnected;
            public IntPtr OnDisconnected;
            public IntPtr OnFocusGained;
            public IntPtr OnFocusLost;
            public IntPtr GetUOFilePath;
            public IntPtr Recv;
            public IntPtr Send;
            public IntPtr GetPacketLength;
            public IntPtr GetPlayerPosition;
            public IntPtr CastSpell;
            public IntPtr GetStaticImage;
            public IntPtr Tick;
            public IntPtr RequestMove;
            public IntPtr SetTitle;

            public IntPtr OnRecv_new, OnSend_new, Recv_new, Send_new;

            public IntPtr OnDrawCmdList;
            public IntPtr SDL_Window;
            public IntPtr OnWndProc;
            public IntPtr GetStaticData;
            public IntPtr GetTileData;
            public IntPtr GetCliloc;
        }

        private readonly Dictionary<IntPtr, GraphicsResource> _resources = new Dictionary<IntPtr, GraphicsResource>();

        private Plugin(string path)
        {
            _path = path;
        }

        public bool IsValid { get; private set; }


        public static Plugin Create(string path)
        {
            path = Path.GetFullPath(Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Plugins", path));

            if (!File.Exists(path))
            {
                Log.Error($"Plugin '{path}' not found.");

                return null;
            }

            Log.Trace($"Loading plugin: {path}");

            Plugin p = new Plugin(path);
            p.Load();

            if (!p.IsValid)
            {
                Log.Warn($"Invalid plugin: {path}");

                return null;
            }

            Log.Trace($"Plugin: {path} loaded.");
            _plugins.Add(p);

            return p;
        }

        public static bool LoadInternalAssistant()
        {
            //If the plugin has already been created, don't create it again
            if (_plugins.Any(x => x._path == hardcodedInternalAssistantPath))
            {
                return false;
            }
            
            //NOTE: Temporary fix for empty xml files created with initial version of Assistant
            var dataPath = Path.Combine(Profile.DataPath, "Data");
            var spellsXmlPath = Path.Combine(dataPath, "spells.xml");
            if (File.Exists(spellsXmlPath) && File.ReadAllText(Path.Combine(Profile.DataPath, "Data", "spells.xml")).Trim().Length == 0)
            {
                File.Delete(spellsXmlPath);
                File.Delete(Path.Combine(dataPath, "skills.xml"));
                File.Delete(Path.Combine(dataPath, "bodies.xml"));
                File.Delete(Path.Combine(dataPath, "bufficons.xml"));
                File.Delete(Path.Combine(dataPath, "foods.xml"));
                File.Delete(Path.Combine(dataPath, "assistant.xml"));
                File.Delete(Path.Combine(Profile.ProfilePath, "Default.xml"));
            }
            
            var plugin = new Plugin(hardcodedInternalAssistantPath);
            plugin.Load();
            
            if (plugin.IsValid == false)
            {
                Log.Warn($"Invalid plugin: {plugin._path}");
                return false;
            }
            
            Log.Trace($"Plugin: {plugin._path} loaded.");
            _plugins.Add(plugin);
            return true;
        }


        public void Load()
        {
            _recv = OnPluginRecv;
            _send = OnPluginSend;
            _recv_new = OnPluginRecv_new;
            _send_new = OnPluginSend_new;
            _getPacketLength = PacketsTable.GetPacketLength;
            _getPlayerPosition = GetPlayerPosition;
            _castSpell = GameActions.CastSpell;
            _getStaticImage = GetStaticImage;
            _getUoFilePath = GetUOFilePath;
            _requestMove = RequestMove;
            _setTitle = SetWindowTitle;
            _get_static_data = GetStaticData;
            _get_tile_data = GetTileData;
            _get_cliloc = GetCliloc;

            /*
            SDL.SDL_SysWMinfo info = new SDL.SDL_SysWMinfo();
            SDL.SDL_VERSION(out info.version);
            SDL.SDL_GetWindowWMInfo(Client.Game.Window.Handle, ref info);

            IntPtr hwnd = IntPtr.Zero;

            if (info.subsystem == SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS)
                hwnd = info.info.win.window;

            PluginHeader header = new PluginHeader
            {
                ClientVersion = (int) Client.Version,
                Recv = Marshal.GetFunctionPointerForDelegate(_recv),
                Send = Marshal.GetFunctionPointerForDelegate(_send),
                GetPacketLength = Marshal.GetFunctionPointerForDelegate(_getPacketLength),
                GetPlayerPosition = Marshal.GetFunctionPointerForDelegate(_getPlayerPosition),
                CastSpell = Marshal.GetFunctionPointerForDelegate(_castSpell),
                GetStaticImage = Marshal.GetFunctionPointerForDelegate(_getStaticImage),
                HWND = hwnd,
                GetUOFilePath = Marshal.GetFunctionPointerForDelegate(_getUoFilePath),
                RequestMove = Marshal.GetFunctionPointerForDelegate(_requestMove),
                SetTitle = Marshal.GetFunctionPointerForDelegate(_setTitle),
                Recv_new = Marshal.GetFunctionPointerForDelegate(_recv_new),
                Send_new = Marshal.GetFunctionPointerForDelegate(_send_new),

                SDL_Window = Client.Game.Window.Handle,
                GetStaticData = Marshal.GetFunctionPointerForDelegate(_get_static_data),
                GetTileData = Marshal.GetFunctionPointerForDelegate(_get_tile_data),
                GetCliloc = Marshal.GetFunctionPointerForDelegate(_get_cliloc),
            };

            void* func = &header;
            
            if(Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
                UnblockPath(Path.GetDirectoryName(_path));

            try
            {
                IntPtr assptr = Native.LoadLibrary(_path);

                Log.Trace($"assembly: {assptr}");

                if (assptr == IntPtr.Zero)
                    throw new Exception("Invalid Assembly, Attempting managed load.");

                Log.Trace($"Searching for 'Install' entry point  -  {assptr}");

                IntPtr installPtr = Native.GetProcessAddress(assptr, "Install");

                Log.Trace($"Entry point: {installPtr}");

                if (installPtr == IntPtr.Zero)
                    throw new Exception("Invalid Entry Point, Attempting managed load.");

                Marshal.GetDelegateForFunctionPointer<OnInstall>(installPtr)(func);

                Console.WriteLine(">>> ADDRESS {0}", header.OnInitialize);
            }
            catch
            {
                try
                {
                    var asm = Assembly.LoadFile(_path);
                    var type = asm.GetType("Assistant.Engine");

                    if (type == null)
                    {
                        Log.Error(
                                    "Unable to find Plugin Type, API requires the public class Engine in namespace Assistant.");

                        return;
                    }

                    var meth = type.GetMethod("Install", BindingFlags.Public | BindingFlags.Static);

                    if (meth == null)
                    {
                        Log.Error("Engine class missing public static Install method Needs 'public static unsafe void Install(PluginHeader *plugin)' ");

                        return;
                    }

                    meth.Invoke(null, new object[] { (IntPtr) func });
                }
                catch (Exception err)
                {
                    Log.Error(
                                $"Plugin threw an error during Initialization. {err.Message} {err.StackTrace} {err.InnerException?.Message} {err.InnerException?.StackTrace}");

                    return;
                }
            }


            if (header.OnRecv != IntPtr.Zero)
                _onRecv = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>(header.OnRecv);

            if (header.OnSend != IntPtr.Zero)
                _onSend = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>(header.OnSend);

            if (header.OnHotkeyPressed != IntPtr.Zero)
                _onHotkeyPressed = Marshal.GetDelegateForFunctionPointer<OnHotkey>(header.OnHotkeyPressed);

            if (header.OnMouse != IntPtr.Zero)
                _onMouse = Marshal.GetDelegateForFunctionPointer<OnMouse>(header.OnMouse);

            if (header.OnPlayerPositionChanged != IntPtr.Zero)
                _onUpdatePlayerPosition = Marshal.GetDelegateForFunctionPointer<OnUpdatePlayerPosition>(header.OnPlayerPositionChanged);

            if (header.OnClientClosing != IntPtr.Zero)
                _onClientClose = Marshal.GetDelegateForFunctionPointer<OnClientClose>(header.OnClientClosing);

            if (header.OnInitialize != IntPtr.Zero)
                _onInitialize = Marshal.GetDelegateForFunctionPointer<OnInitialize>(header.OnInitialize);

            if (header.OnConnected != IntPtr.Zero)
                _onConnected = Marshal.GetDelegateForFunctionPointer<OnConnected>(header.OnConnected);

            if (header.OnDisconnected != IntPtr.Zero)
                _onDisconnected = Marshal.GetDelegateForFunctionPointer<OnDisconnected>(header.OnDisconnected);

            if (header.OnFocusGained != IntPtr.Zero)
                _onFocusGained = Marshal.GetDelegateForFunctionPointer<OnFocusGained>(header.OnFocusGained);

            if (header.OnFocusLost != IntPtr.Zero)
                _onFocusLost = Marshal.GetDelegateForFunctionPointer<OnFocusLost>(header.OnFocusLost);

            if (header.Tick != IntPtr.Zero)
                _tick = Marshal.GetDelegateForFunctionPointer<OnTick>(header.Tick);


            if (header.OnRecv_new != IntPtr.Zero)
                _onRecv_new = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv_new>(header.OnRecv_new);
            if (header.OnSend_new != IntPtr.Zero)
                _onSend_new = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv_new>(header.OnSend_new);

            if (header.OnDrawCmdList != IntPtr.Zero)
                _draw_cmd_list = Marshal.GetDelegateForFunctionPointer<OnDrawCmdList>(header.OnDrawCmdList);
            if (header.OnWndProc != IntPtr.Zero)
                _on_wnd_proc = Marshal.GetDelegateForFunctionPointer<OnWndProc>(header.OnWndProc);
            */
            
#if ENABLE_INTERNAL_ASSISTANT
            if (_path == hardcodedInternalAssistantPath)
            {
                try
                {
                    Assistant.Engine.Install(null);
                    
                    Assistant.Engine.UOSteamClient._sendToClient = OnPluginRecv;
                    Assistant.Engine.UOSteamClient._sendToServer = OnPluginSend;
                    Assistant.Engine.UOSteamClient._getPacketLength = PacketsTable.GetPacketLength;
                    Assistant.Engine.UOSteamClient._getPlayerPosition = GetPlayerPosition;
                    Assistant.Engine.UOSteamClient._castSpell = GameActions.CastSpell;
                    Assistant.Engine.UOSteamClient._getStaticImage = GetStaticImage;
                    Assistant.Engine.UOSteamClient._requestMove = RequestMove;
                    Assistant.Engine.UOSteamClient._setTitle = SetWindowTitle;
                    Assistant.Engine.UOSteamClient._uoFilePath = GetUOFilePath;

                    _onRecv = Assistant.Engine.UOSteamClient._recv;
                    _onSend = Assistant.Engine.UOSteamClient._send;
                    _onHotkeyPressed = Assistant.Engine.UOSteamClient._onHotkeyPressed;
                    _onMouse = Assistant.Engine.UOSteamClient._onMouse;
                    _onUpdatePlayerPosition = Assistant.Engine.UOSteamClient._onUpdatePlayerPosition;
                    _onClientClose = Assistant.Engine.UOSteamClient._onClientClose;
                    _onInitialize = Assistant.Engine.UOSteamClient._onInitialize;
                    _onConnected = Assistant.Engine.UOSteamClient._onConnected;
                    _onDisconnected = Assistant.Engine.UOSteamClient._onDisconnected;
                    _onFocusGained = Assistant.Engine.UOSteamClient._onFocusGained;
                    _onFocusLost = Assistant.Engine.UOSteamClient._onFocusLost;
                    _tick = Assistant.Engine.UOSteamClient._tick;
                }
                catch (Exception err)
                {
                    Log.Error(
                        $"Plugin threw an error during Initialization. {err.Message} {err.StackTrace} {err.InnerException?.Message} {err.InnerException?.StackTrace}");

                    return;
                }
            }
#endif

            IsValid = true;

            _onInitialize?.Invoke();
        }

        private static string GetUOFilePath()
        {
            return Settings.GlobalSettings.UltimaOnlineDirectory;
        }

        private static void SetWindowTitle(string str)
        {
#if DEV_BUILD
            Client.Game.Window.Title = $"{str} - ClassicUO [dev] - {CUOEnviroment.Version}";
#else
            Client.Game.Window.Title = $"{str} - ClassicUO - { CUOEnviroment.Version}";
#endif
        }

        private static bool GetStaticData(int index,
                                                 ref ulong flags, 
                                                 ref byte weight, 
                                                 ref byte layer, 
                                                 ref int count,
                                                 ref ushort animid,
                                                 ref ushort lightidx, 
                                                 ref byte height,
                                                 ref string name)
        {
            if (index >= 0 && index < Constants.MAX_STATIC_DATA_INDEX_COUNT)
            {
                ref var st = ref TileDataLoader.Instance.StaticData[index];

                flags = (ulong) st.Flags;
                weight = st.Weight;
                layer = st.Layer;
                count = st.Count;
                animid = st.AnimID;
                lightidx = st.LightIndex;
                height = st.Height;
                name = st.Name;
               
                return true;
            }

            return false;
        }

        private static bool GetTileData(int index, 
                                        ref ulong flags,
                                        ref ushort textid,
                                        ref string name)
        {
            if (index >= 0 && index < Constants.MAX_STATIC_DATA_INDEX_COUNT)
            {
                ref var st = ref TileDataLoader.Instance.LandData[index];

                flags = (ulong) st.Flags;
                textid = st.TexID;
                name = st.Name;

                return true;
            }

            return false;
        }

        private static bool GetCliloc(int cliloc, string args, bool capitalize, out string buffer)
        {
            buffer = ClilocLoader.Instance.Translate(cliloc, args, capitalize);

            return buffer != null;
        }

        private static void GetStaticImage(ushort g, ref ArtInfo info)
        {
            ArtLoader.Instance.TryGetEntryInfo(g, out long address, out long size, out long compressedsize);
            info.Address = address;
            info.Size = size;
            info.CompressedSize = compressedsize;
        }

        private static bool RequestMove(int dir, bool run)
        {
            return World.Player.Walk((Direction) dir, run);
        }

        private static bool GetPlayerPosition(out int x, out int y, out int z)
        {
            if (World.Player != null)
            {
                x = World.Player.X;
                y = World.Player.Y;
                z = World.Player.Z;

                return true;
            }

            x = y = z = 0;

            return false;
        }

        internal static void Tick()
        {
            foreach (Plugin t in _plugins)
                t._tick?.Invoke();
        }


        internal static bool ProcessRecvPacket(ref byte[] data, ref int length)
        {
            bool result = true;

            foreach (Plugin plugin in _plugins)
            {
                if (plugin._onRecv_new != null)
                {
                    if (!plugin._onRecv_new(data, ref length))
                    {
                        result = false;
                    }
                }
                else if (plugin._onRecv != null && !plugin._onRecv(ref data, ref length))
                    result = false;
            }

            return result;
        }

        internal static bool ProcessSendPacket(ref byte[] data, ref int length)
        {
            bool result = true;

            foreach (Plugin plugin in _plugins)
            {
                if (plugin._onSend_new != null)
                {
                    if (!plugin._onSend_new(data, ref length))
                    {
                        result = false;
                    }
                }
                else if (plugin._onSend != null && !plugin._onSend(ref data, ref length))
                    result = false;
            }

            return result;
        }

        internal static void OnClosing()
        {
            for (int i = 0; i < _plugins.Count; i++)
            {
                _plugins[i]._onClientClose?.Invoke();

                _plugins.RemoveAt(i--);
            }
        }

        internal static void OnFocusGained()
        {
            foreach (Plugin t in _plugins)
                t._onFocusGained?.Invoke();
        }

        internal static void OnFocusLost()
        {
            foreach (Plugin t in _plugins)
                t._onFocusLost?.Invoke();
        }


        internal static void OnConnected()
        {
            foreach (Plugin t in _plugins)
                t._onConnected?.Invoke();
        }

        internal static void OnDisconnected()
        {
            foreach (Plugin t in _plugins)
                t._onDisconnected?.Invoke();
        }

        internal static bool ProcessHotkeys(int key, int mod, bool ispressed)
        {
            bool result = true;


            if (!World.InGame ||
                (ProfileManager.Current != null &&
                ProfileManager.Current.ActivateChatAfterEnter &&
                UIManager.SystemChat?.IsActive == true) ||
                UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl)
            {
                return result;
            }

            foreach (Plugin plugin in _plugins)
            {
                if (plugin._onHotkeyPressed != null && !plugin._onHotkeyPressed(key, mod, ispressed))
                    result = false;
            }

            return result;
        }

        internal static void ProcessMouse(int button, int wheel)
        {
            foreach (Plugin plugin in _plugins)
                plugin._onMouse?.Invoke(button, wheel);
        }

        internal static void ProcessDrawCmdList(GraphicsDevice device)
        {
            foreach (Plugin plugin in Plugins)
            {
                if (plugin._draw_cmd_list != null)
                {
                    int cmd_count = 0;
                    plugin._draw_cmd_list.Invoke(out IntPtr cmdlist, ref cmd_count);

                    if (cmd_count != 0 && cmdlist != IntPtr.Zero)
                    {
                        plugin.HandleCmdList(device, cmdlist, cmd_count, plugin._resources);
                    }
                }
            }
        }

        internal static int ProcessWndProc(SDL.SDL_Event* e)
        {
            int result = 0;
            foreach (Plugin plugin in Plugins)
            {
                result |= plugin._on_wnd_proc?.Invoke(e) ?? 0;
            }

            return result;
        }

        internal static void UpdatePlayerPosition(int x, int y, int z)
        {
            foreach (Plugin plugin in _plugins)
            {
                try
                {
                    // TODO: need fixed on razor side
                    // if you quick entry (0.5-1 sec after start, without razor window loaded) - breaks CUO.
                    // With this fix - the razor does not work, but client does not crashed.
                    plugin._onUpdatePlayerPosition?.Invoke(x, y, z);
                }
                catch
                {
                    Log.Error("Plugin initialization failed, please re login");
                }
            }
        }

        private static bool OnPluginRecv(ref byte[] data, ref int length)
        {
            NetClient.EnqueuePacketFromPlugin(data, length);

            return true;
        }

        private static bool OnPluginSend(ref byte[] data, ref int length)
        {
            //if (data != null && data.Length != 0)
            //{
            //    // horrible workaround to avoid ghosting item when a plugin sends drag request item
            //    switch (data[0])
            //    {
            //        case 0x07:
            //            ItemHold.Clear();
            //            break;
            //    }
            //}


            if (NetClient.LoginSocket.IsDisposed && NetClient.Socket.IsConnected)
                NetClient.Socket.Send(data, length, true);
            else if (NetClient.Socket.IsDisposed && NetClient.LoginSocket.IsConnected)
                NetClient.LoginSocket.Send(data, length, true);

            return true;
        }

        private static bool OnPluginRecv_new(IntPtr buffer, ref int length)
        {
            byte[] data = new byte[length];
            Marshal.Copy(buffer, data, 0, length);
            
            return OnPluginRecv(ref data, ref length);
        }

        private static bool OnPluginSend_new(IntPtr buffer, ref int length)
        {
            byte[] data = new byte[length];
            Marshal.Copy(buffer, data, 0, length);

            return OnPluginSend(ref data, ref length);
        }

        
        //Code from https://stackoverflow.com/questions/6374673/unblock-file-from-within-net-4-c-sharp
        private static void UnblockPath(string path)
        {
            string[] files = System.IO.Directory.GetFiles(path);
            string[] dirs = System.IO.Directory.GetDirectories(path);

            foreach (string file in files)
            {
                if(file.EndsWith("dll") || file.EndsWith("exe"))
                    UnblockFile(file);
            }

            foreach (string dir in dirs)
            {
                UnblockPath(dir);
            }
        }

        private static bool UnblockFile(string fileName)
        {
            return DeleteFile(fileName + ":Zone.Identifier");
        }

        private void HandleCmdList(GraphicsDevice device, IntPtr ptr, int length, Dictionary<IntPtr, GraphicsResource> resources)
        {
            if (ptr == IntPtr.Zero || length <= 0)
                return;

            const int CMD_VIEWPORT = 0;
            const int CMD_SCISSOR = 1;
            const int CMD_BLEND_STATE = 2;
            const int CMD_RASTERIZE_STATE = 3;
            const int CMD_STENCIL_STATE = 4;
            const int CMD_SAMPLER_STATE = 5;
            const int CMD_SET_VERTEX_BUFFER = 6;
            const int CMD_SET_INDEX_BUFFER = 7;
            const int CMD_CREATE_VERTEX_BUFFER = 8;
            const int CMD_CREATE_INDEX_BUFFER = 9;
            const int CMD_CREATE_EFFECT = 10;
            const int CMD_CREATE_TEXTURE_2D = 11;
            const int CMD_SET_TEXTURE_DATA_2D = 12;
            const int CMD_INDEXED_PRIMITIVE_DATA = 13;
            const int CMD_CREATE_BASIC_EFFECT = 14;
            const int CMD_SET_VERTEX_DATA = 15;
            const int CMD_SET_INDEX_DATA = 16;
            const int CMD_DESTROY_RESOURCE = 17;
            const int CMD_BLEND_FACTOR = 18;
            const int CMD_NEW_BLEND_STATE = 19;
            const int CMD_NEW_RASTERIZE_STATE = 20;
            const int CMD_NEW_STENCIL_STATE = 21;
            const int CMD_NEW_SAMPLER_STATE = 22;


            Effect current_effect = null;


            var lastViewport = device.Viewport;
            var lastScissorBox = device.ScissorRectangle;

            var lastBlendFactor = device.BlendFactor;
            var lastBlendState = device.BlendState;
            var lastRasterizeState = device.RasterizerState;
            var lastDepthStencilState = device.DepthStencilState;
            var lastsampler = device.SamplerStates[0];


            //var blend_snap_AlphaBlendFunction = device.BlendState.AlphaBlendFunction;
            //var blend_snap_AlphaDestinationBlend = device.BlendState.AlphaDestinationBlend;
            //var blend_snap_AlphaSourceBlend = device.BlendState.AlphaSourceBlend;
            //var blend_snap_ColorBlendFunction = device.BlendState.ColorBlendFunction;
            //var blend_snap_ColorDestinationBlend = device.BlendState.ColorDestinationBlend;
            //var blend_snap_ColorSourceBlend = device.BlendState.ColorSourceBlend;
            //var blend_snap_ColorWriteChannels = device.BlendState.ColorWriteChannels;
            //var blend_snap_ColorWriteChannels1 = device.BlendState.ColorWriteChannels1;
            //var blend_snap_ColorWriteChannels2 = device.BlendState.ColorWriteChannels2;
            //var blend_snap_ColorWriteChannels3 = device.BlendState.ColorWriteChannels3;
            //var blend_snap_BlendFactor = device.BlendState.BlendFactor;
            //var blend_snap_MultiSampleMask = device.BlendState.MultiSampleMask;

            //var rasterize_snap_CullMode = device.RasterizerState.CullMode;
            //var rasterize_snap_DepthBias = device.RasterizerState.DepthBias;
            //var rasterize_snap_FillMode = device.RasterizerState.FillMode;
            //var rasterize_snap_MultiSampleAntiAlias = device.RasterizerState.MultiSampleAntiAlias;
            //var rasterize_snap_ScissorTestEnable = device.RasterizerState.ScissorTestEnable;
            //var rasterize_snap_SlopeScaleDepthBias = device.RasterizerState.SlopeScaleDepthBias;

            //var stencil_snap_DepthBufferEnable = device.DepthStencilState.DepthBufferEnable;
            //var stencil_snap_DepthBufferWriteEnable = device.DepthStencilState.DepthBufferWriteEnable;
            //var stencil_snap_DepthBufferFunction = device.DepthStencilState.DepthBufferFunction;
            //var stencil_snap_StencilEnable = device.DepthStencilState.StencilEnable;
            //var stencil_snap_StencilFunction = device.DepthStencilState.StencilFunction;
            //var stencil_snap_StencilPass = device.DepthStencilState.StencilPass;
            //var stencil_snap_StencilFail = device.DepthStencilState.StencilFail;
            //var stencil_snap_StencilDepthBufferFail = device.DepthStencilState.StencilDepthBufferFail;
            //var stencil_snap_TwoSidedStencilMode = device.DepthStencilState.TwoSidedStencilMode;
            //var stencil_snap_CounterClockwiseStencilFunction = device.DepthStencilState.CounterClockwiseStencilFunction;
            //var stencil_snap_CounterClockwiseStencilFail = device.DepthStencilState.CounterClockwiseStencilFail;
            //var stencil_snap_CounterClockwiseStencilPass = device.DepthStencilState.CounterClockwiseStencilPass;
            //var stencil_snap_CounterClockwiseStencilDepthBufferFail = device.DepthStencilState.CounterClockwiseStencilDepthBufferFail;
            //var stencil_snap_StencilMask = device.DepthStencilState.StencilMask;
            //var stencil_snap_StencilWriteMask = device.DepthStencilState.StencilWriteMask;
            //var stencil_snap_ReferenceStencil = device.DepthStencilState.ReferenceStencil;



            for (int i = 0; i < length; i++)
            {
                batch_cmd cmd = ((batch_cmd*) (ptr))[i];

                switch (cmd.type)
                {
                    case CMD_VIEWPORT:
                        ref var viewport = ref cmd.viewport;

                        device.Viewport = new Viewport(
                                                       viewport.x,
                                                       viewport.y,
                                                       viewport.w,
                                                       viewport.h);

                        break;

                    case CMD_SCISSOR:
                        ref var scissor = ref cmd.scissor;

                        device.ScissorRectangle = new Rectangle(
                                                                scissor.x,
                                                                scissor.y,
                                                                scissor.w,
                                                                scissor.h);

                        break;

                    case CMD_BLEND_FACTOR:

                        ref var blend_factor = ref cmd.new_blend_factor;

                        device.BlendFactor = blend_factor.color;

                        break;

                    case CMD_NEW_BLEND_STATE:
                        ref var blend = ref cmd.new_blend_state;

                        resources[blend.id] = new BlendState()
                        {
                            AlphaBlendFunction = blend.alpha_blend_func,
                            AlphaDestinationBlend = blend.alpha_dest_blend,
                            AlphaSourceBlend = blend.alpha_src_blend,
                            ColorBlendFunction = blend.color_blend_func,
                            ColorDestinationBlend = blend.color_dest_blend,
                            ColorSourceBlend = blend.color_src_blend,
                            ColorWriteChannels = blend.color_write_channels_0,
                            ColorWriteChannels1 = blend.color_write_channels_1,
                            ColorWriteChannels2 = blend.color_write_channels_2,
                            ColorWriteChannels3 = blend.color_write_channels_3,
                            BlendFactor = blend.blend_factor,
                            MultiSampleMask = blend.multiple_sample_mask
                        };

                        break;

                    case CMD_NEW_RASTERIZE_STATE:

                        ref var rasterize = ref cmd.new_rasterize_state;

                        resources[rasterize.id] = new RasterizerState()
                        {
                            CullMode = rasterize.cull_mode,
                            DepthBias = rasterize.depth_bias,
                            FillMode = rasterize.fill_mode,
                            MultiSampleAntiAlias = rasterize.multi_sample_aa,
                            ScissorTestEnable = rasterize.scissor_test_enabled,
                            SlopeScaleDepthBias = rasterize.slope_scale_depth_bias
                        };

                        break;

                    case CMD_NEW_STENCIL_STATE:

                        ref var stencil = ref cmd.new_stencil_state;

                        resources[stencil.id] = new DepthStencilState()
                        {
                            DepthBufferEnable = stencil.depth_buffer_enabled,
                            DepthBufferWriteEnable = stencil.depth_buffer_write_enabled,
                            DepthBufferFunction = stencil.depth_buffer_func,
                            StencilEnable = stencil.stencil_enabled,
                            StencilFunction = stencil.stencil_func,
                            StencilPass = stencil.stencil_pass,
                            StencilFail = stencil.stencil_fail,
                            StencilDepthBufferFail = stencil.stencil_depth_buffer_fail,
                            TwoSidedStencilMode = stencil.two_sided_stencil_mode,
                            CounterClockwiseStencilFunction = stencil.counter_clockwise_stencil_func,
                            CounterClockwiseStencilFail = stencil.counter_clockwise_stencil_fail,
                            CounterClockwiseStencilPass = stencil.counter_clockwise_stencil_pass,
                            CounterClockwiseStencilDepthBufferFail = stencil.counter_clockwise_stencil_depth_buffer_fail,
                            StencilMask = stencil.stencil_mask,
                            StencilWriteMask = stencil.stencil_write_mask,
                            ReferenceStencil = stencil.reference_stencil
                        };

                        
                        break;

                    case CMD_NEW_SAMPLER_STATE:

                        ref var sampler = ref cmd.new_sampler_state;

                        resources[sampler.id] = new SamplerState()
                        {
                            AddressU = sampler.address_u,
                            AddressV = sampler.address_v,
                            AddressW = sampler.address_w,
                            Filter = sampler.filter,
                            MaxAnisotropy = sampler.max_anisotropy,
                            MaxMipLevel = sampler.max_mip_level,
                            MipMapLevelOfDetailBias = sampler.mip_map_level_of_detail_bias
                        };

                        break;

                    case CMD_BLEND_STATE:

                        device.BlendState = resources[cmd.set_blend_state.id] as BlendState;

                        break;

                    case CMD_RASTERIZE_STATE:

                        device.RasterizerState = resources[cmd.set_rasterize_state.id] as RasterizerState;
                        
                        break;

                    case CMD_STENCIL_STATE:

                        device.DepthStencilState = resources[cmd.set_stencil_state.id] as DepthStencilState;

                        break;

                    case CMD_SAMPLER_STATE:

                        device.SamplerStates[cmd.set_sampler_state.index] = resources[cmd.set_sampler_state.id] as SamplerState;

                        break;

                    case CMD_SET_VERTEX_DATA:

                        ref var set_vertex_data = ref cmd.set_vertex_data;

                        var vertex_buffer = resources[set_vertex_data.id] as VertexBuffer;

                        vertex_buffer?.SetDataPointerEXT(0,
                                                         set_vertex_data.vertex_buffer_ptr,
                                                         set_vertex_data.vertex_buffer_length,
                                                         SetDataOptions.None);

                        break;

                    case CMD_SET_INDEX_DATA:

                        ref var set_index_data = ref cmd.set_index_data;

                        var index_buffer = resources[set_index_data.id] as IndexBuffer;

                        index_buffer?.SetDataPointerEXT(0,
                                                        set_index_data.indices_buffer_ptr,
                                                        set_index_data.indices_buffer_length,
                                                        SetDataOptions.None);

                        break;

                    case CMD_CREATE_VERTEX_BUFFER:

                        ref var create_vertex_buffer = ref cmd.create_vertex_buffer;

                        VertexElement[] elements = new VertexElement[create_vertex_buffer.decl_count];

                        for (int j = 0; j < elements.Length; j++)
                        {
                            elements[j] = ((VertexElement*) (create_vertex_buffer.declarations))[j];
                        }

                        VertexBuffer vb = create_vertex_buffer.is_dynamic ?
                                              new DynamicVertexBuffer(device,
                                                                new VertexDeclaration(create_vertex_buffer.size, elements),
                                                                create_vertex_buffer.vertex_elements_count,
                                                                create_vertex_buffer.buffer_usage)
                                              :
                                              new VertexBuffer(device,
                                                                 new VertexDeclaration(create_vertex_buffer.size, elements),
                                                                 create_vertex_buffer.vertex_elements_count,
                                                                 create_vertex_buffer.buffer_usage);

                        resources[create_vertex_buffer.id] = vb;

                        break;

                    case CMD_CREATE_INDEX_BUFFER:

                        ref var create_index_buffer = ref cmd.create_index_buffer;

                        IndexBuffer ib = create_index_buffer.is_dynamic ?
                                             new DynamicIndexBuffer(device, create_index_buffer.index_element_size, create_index_buffer.index_count, create_index_buffer.buffer_usage)
                                             :
                                             new IndexBuffer(device, create_index_buffer.index_element_size, create_index_buffer.index_count, create_index_buffer.buffer_usage);

                        resources[create_index_buffer.id] = ib;

                        break;

                    case CMD_SET_VERTEX_BUFFER:

                        ref var set_vertex_buffer = ref cmd.set_vertex_buffer;

                        vb = resources[set_vertex_buffer.id] as VertexBuffer;

                        device.SetVertexBuffer(vb);

                        break;

                    case CMD_SET_INDEX_BUFFER:

                        ref var set_index_buffer = ref cmd.set_index_buffer;

                        ib = resources[set_index_buffer.id] as IndexBuffer;

                        device.Indices = ib;

                        break;

                    case CMD_CREATE_EFFECT:

                        ref var create_effect = ref cmd.create_effect;

                        break;

                    case CMD_CREATE_BASIC_EFFECT:

                        ref var create_basic_effect = ref cmd.create_basic_effect;

                        if (!resources.TryGetValue(create_basic_effect.id, out GraphicsResource res))
                        {
                            res = new BasicEffect(device);
                            resources[create_basic_effect.id] = res;
                        }
                        else
                        {
                            BasicEffect be = res as BasicEffect;
                            be.World = create_basic_effect.world;
                            be.View = create_basic_effect.view;
                            be.Projection = create_basic_effect.projection;
                            be.TextureEnabled = create_basic_effect.texture_enabled;
                            be.Texture = resources[create_basic_effect.texture_id] as Texture2D;
                            be.VertexColorEnabled = create_basic_effect.vertex_color_enabled;

                            current_effect = be;
                        }

                        break;

                    case CMD_CREATE_TEXTURE_2D:

                        ref var create_texture_2d = ref cmd.create_texture_2d;

                        Texture2D texture;
                        if (create_texture_2d.is_render_target)
                        {
                            texture = new RenderTarget2D(device,
                                                         create_texture_2d.width,
                                                         create_texture_2d.height,
                                                         false,
                                                         create_texture_2d.format,
                                                         DepthFormat.Depth24Stencil8);
                        }
                        else
                        {
                            texture = new Texture2D(device,
                                                    create_texture_2d.width,
                                                    create_texture_2d.height,
                                                    false,
                                                    create_texture_2d.format);
                        }


                        resources[create_texture_2d.id] = texture;

                        break;

                    case CMD_SET_TEXTURE_DATA_2D:

                        ref var set_texture_data_2d = ref cmd.set_texture_data_2d;

                        texture = resources[set_texture_data_2d.id] as Texture2D;

                        texture?.SetDataPointerEXT(set_texture_data_2d.level,
                                                   new Rectangle(set_texture_data_2d.x,
                                                                     set_texture_data_2d.y,
                                                                     set_texture_data_2d.width,
                                                                     set_texture_data_2d.height
                                                                     ),
                                                  set_texture_data_2d.data,
                                                  set_texture_data_2d.data_length);

                        break;

                    case CMD_INDEXED_PRIMITIVE_DATA:

                        ref var indexed_primitive_data = ref cmd.indexed_primitive_data;

                        //device.Textures[0] = resources[indexed_primitive_data.texture_id] as Texture;

                        if (current_effect != null)
                        {
                            foreach (var pass in current_effect.CurrentTechnique.Passes)
                            {
                                pass.Apply();

                                device.DrawIndexedPrimitives(
                                                             indexed_primitive_data.primitive_type,
                                                             indexed_primitive_data.base_vertex,
                                                             indexed_primitive_data.min_vertex_index,
                                                             indexed_primitive_data.num_vertices,
                                                             indexed_primitive_data.start_index,
                                                             indexed_primitive_data.primitive_count);
                            }
                        }
                        else
                        {
                            device.DrawIndexedPrimitives(
                                                         indexed_primitive_data.primitive_type,
                                                         indexed_primitive_data.base_vertex,
                                                         indexed_primitive_data.min_vertex_index,
                                                         indexed_primitive_data.num_vertices,
                                                         indexed_primitive_data.start_index,
                                                         indexed_primitive_data.primitive_count);
                        }

                        break;

                    case CMD_DESTROY_RESOURCE:

                        ref var destroy_resource = ref cmd.destroy_resource;

                        resources[destroy_resource.id]?.Dispose();
                        resources.Remove(destroy_resource.id);

                        break;
                }
            }


            device.Viewport = lastViewport;
            device.ScissorRectangle = lastScissorBox;
            device.BlendFactor = lastBlendFactor;
            device.BlendState = lastBlendState;
            device.RasterizerState = lastRasterizeState;
            device.DepthStencilState = lastDepthStencilState;
            device.SamplerStates[0] = lastsampler;
        }

    }
}