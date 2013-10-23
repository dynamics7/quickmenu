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

namespace QuickMenu
{
    public class ItemFactory
    {

        public static ItemViewModel Create(string internalName, int index)
        {
            ItemViewModel item = null;
            if (internalName.StartsWith("{"))
            {
                var info = new ApplicationApi.ApplicationInfo(internalName);
                if (info.IsValid())
                {
                    item = new ItemViewModel(info);
                }
            }
            else
            {
                item = SpecialItemRepository.GetByName(internalName);
                if (item == null)
                    item = new SpecialItemViewModel(internalName, index);
            }
            return item;
        }
    }
}
