

using System;

using Microsoft.Xna.Framework;

using SDL2;

namespace ClassicUO.Input
{
    internal static class Mouse
    {
        public const int MOUSE_DELAY_DOUBLE_CLICK = 350;

        public static uint LastLeftButtonClickTime { get; set; }

        public static uint LastMidButtonClickTime { get; set; }

        public static uint LastRightButtonClickTime { get; set; }

        public static bool CancelDoubleClick { get; set; }

        public static bool LButtonPressed { get; set; }

        public static bool RButtonPressed { get; set; }

        public static bool MButtonPressed { get; set; }

        public static bool XButtonPressed { get; set; }

        public static bool IsDragging { get; set; }

        public static Point Position;

        public static Point RealPosition;

        public static Point LDropPosition;

        public static Point RDropPosition;

        public static Point MDropPosition;

        public static Point LDroppedOffset => LButtonPressed ? RealPosition - LDropPosition : Point.Zero;

        public static Point RDroppedOffset => RButtonPressed ? RealPosition - RDropPosition : Point.Zero;

        public static Point MDroppedOffset => MButtonPressed ? RealPosition - MDropPosition : Point.Zero;

        public static bool MouseInWindow { get; set; }

        public static void Begin()
        {
            SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_TRUE);
        }

        public static void End()
        {
            if (!(LButtonPressed || RButtonPressed || MButtonPressed))
                SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_FALSE);
        }

        public static void Update()
        {
            if (!MouseInWindow)
            {
                SDL.SDL_GetGlobalMouseState(out int x, out int y);
                SDL.SDL_GetWindowPosition(Client.Game.Window.Handle, out int winX, out int winY);
                Position.X = x - winX;
                Position.Y = y - winY;
            }
            //else if (SDL.SDL_GetRelativeMouseMode() == SDL.SDL_bool.SDL_TRUE)
            //{
            //    Console.WriteLine("MOUSE RELATIVE!");
            //    SDL.SDL_GetRelativeMouseState(out Position.X, out Position.Y);
            //}
            else
                SDL.SDL_GetMouseState(out Position.X, out Position.Y);

            // Scale the mouse coordinates for the faux-backbuffer
            Position.X = (int) ((double) Position.X * Client.Game.GraphicManager.PreferredBackBufferWidth / Client.Game.Window.ClientBounds.Width);
            Position.Y = (int) ((double) Position.Y * Client.Game.GraphicManager.PreferredBackBufferHeight / Client.Game.Window.ClientBounds.Height);

            IsDragging = LButtonPressed || RButtonPressed || MButtonPressed;
            RealPosition = Position;
        }
    }
}