using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace QuickMenu
{
    public class SpecialItemRepository
    {
        private static List<SpecialItemViewModel> _items = new List<SpecialItemViewModel>();
        private static bool _isLoaded = false;

        public static void Load()
        {
            _items.Clear();
            _items.Add(new SpecialItemViewModel("Phone", 0));
            _items.Add(new SpecialItemViewModel("Bluetooth", 1));
            _items.Add(new SpecialItemViewModel("Wifi", 2));
            _items.Add(new SpecialItemViewModel("DataConnection", 3));
            _items.Add(new SpecialItemViewModel("BatterySaving", 4));
            _items.Add(new SpecialItemViewModel("Accelerometer", 5));
            _items.Add(new SpecialItemViewModel("InternetSharing", 6));
            _items.Add(new SpecialItemViewModel("Search", 7));
            _isLoaded = true;
        }

        public static List<SpecialItemViewModel> Items
        {
            get
            {
                if (_isLoaded == false)
                    Load();
                return _items;
            }
        }

        public static SpecialItemViewModel GetByName(string s)
        {
            if (_isLoaded == false)
                Load();
            foreach (var item in _items)
            {
                if (item.InternalName == s)
                    return item;
            }
            return null;
        }
    }
}
