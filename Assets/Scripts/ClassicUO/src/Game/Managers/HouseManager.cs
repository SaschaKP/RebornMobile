

using System;
using System.Collections.Generic;

using ClassicUO.Game.GameObjects;

namespace ClassicUO.Game.Managers
{
    internal class HouseManager
    {
        private readonly Dictionary<uint, House> _houses = new Dictionary<uint, House>();

        public IReadOnlyCollection<House> Houses => _houses.Values;

        public void Add(uint serial, House revision)
        {
            _houses[serial] = revision;
        }

        public bool TryGetHouse(uint serial, out House house)
        {
            return _houses.TryGetValue(serial, out house);
        }

        public bool TryToRemove(uint serial, int distance)
        {
            if (!IsHouseInRange(serial, distance))
            {
                if (_houses.TryGetValue(serial, out var house))
                {
                    house.ClearComponents();
                    _houses.Remove(serial);
                }
                else
                {

                }
            

                return true;
            }

            return false;
        }

        public bool IsHouseInRange(uint serial, int distance)
        {
            if (TryGetHouse(serial, out _))
            {
                int currX = World.RangeSize.X;
                int currY = World.RangeSize.Y;

                //if (World.Player.IsMoving)
                //{
                //    Mobile.Step step = World.Player.Steps.Back();

                //    currX = step.X;
                //    currY = step.Y;
                //}
                //else
                //{
                //    currX = World.Player.X;
                //    currY = World.Player.Y;
                //}

                Item found = World.Items.Get(serial);

                if (found == null)
                    return true;

                distance += found.MultiDistanceBonus;

                return Math.Abs(found.X - currX) <= distance && Math.Abs(found.Y - currY) <= distance;
            }

            return false;
        }

        public bool EntityIntoHouse(uint house, GameObject obj)
        {
            if (obj != null && TryGetHouse(house, out _))
            {
                Item found = World.Items.Get(house);

                if (found == null || !found.MultiInfo.HasValue)
                    return true;

                int minX = found.X + found.MultiInfo.Value.X;
                int maxX = found.X + found.MultiInfo.Value.Y;
                int minY = found.Y + found.MultiInfo.Value.Width;
                int maxY = found.Y + found.MultiInfo.Value.Height;

                return obj.X >= minX &&
                       obj.X <= maxX &&
                       obj.Y >= minY &&
                       obj.Y <= maxY;
            }

            return false;
        }

        public void Remove(uint serial)
        {
            if (TryGetHouse(serial, out House house))
            {
                house.ClearComponents();
                _houses.Remove(serial);
            }
        }

        public void RemoveMultiTargetHouse()
        {
            if (_houses.TryGetValue(0, out var house))
            {
                house.ClearComponents();
                _houses.Remove(0);
            }
        }

        public bool Exists(uint serial)
        {
            return _houses.ContainsKey(serial);
        }

        public void Clear()
        {
            foreach (KeyValuePair<uint, House> house in _houses) house.Value.ClearComponents();
            _houses.Clear();
        }
    }
}