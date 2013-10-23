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
    public class ItemViewModelComparer : IComparer<ItemViewModel>
    {

        public int Compare(ItemViewModel x, ItemViewModel y)
        {
            if (x.IsSpecial && !y.IsSpecial)
                return -1;
            else if (!x.IsSpecial && y.IsSpecial)
                return 1;
            else if (x.IsSpecial && y.IsSpecial)
                return x.SpecialItemIndex - y.SpecialItemIndex;
            if (x.Title == null && y.Title != null)
                return 1;
            else if (x.Title != null && y.Title == null)
                return -1;
            else if (x.Title == null && y.Title == null)
                return 0;
            return x.Title.CompareTo(y.Title);
        }
    }
}
