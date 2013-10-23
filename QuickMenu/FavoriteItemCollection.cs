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
using System.IO.IsolatedStorage;

namespace QuickMenu
{
    public class FavoriteItemCollection
    {

        public static event EventHandler CollectionChanged;

        private static string itemCollection = "";
        private static bool _isLoaded = false;
        static FavoriteItemCollection()
        {

        }

        public static void Load()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("favItems"))
            {
                itemCollection = IsolatedStorageSettings.ApplicationSettings["favItems"] as string;
                itemCollection = itemCollection.Trim(';');
                _isLoaded = true;
            }
        }
        public static void Flush()
        {
            itemCollection = itemCollection.Trim(';');
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("favItems"))
                IsolatedStorageSettings.ApplicationSettings.Add("favItems", itemCollection);
            else
                IsolatedStorageSettings.ApplicationSettings["favItems"] = itemCollection;
        }

        public static void Ping()
        {
            if (_isLoaded == false)
                Load();
        }

        public static string[] Items()
        {
            if (_isLoaded == false)
                Load();
            if (itemCollection == "")
                return null;
            itemCollection = itemCollection.TrimEnd(';');
            var items = itemCollection.Split(';');
            return items;
        }
        public static void Add(string item)
        {
            itemCollection = itemCollection.TrimEnd(';');
            itemCollection += ";" + item;
            if (CollectionChanged != null)
                CollectionChanged(null, new EventArgs());
        }

        public static void Remove(string item)
        {
            itemCollection = itemCollection.Replace(item + ";", "");
            itemCollection = itemCollection.Replace(item, "");
            if (CollectionChanged != null)
                CollectionChanged(null, new EventArgs());
        }

        public static bool Contains(string s)
        {
            if (_isLoaded == false)
                Load();
            if (itemCollection.IndexOf(s) != -1)
                return true;
            return false;
        }

        public static void UpdateFromArray(string[] items)
        {
            if (items.Length > 0)
            {
                string result = items[0];
                for (int i = 1; i < items.Length; i++)
                {
                    result += ";" + items[i];
                }
                itemCollection = result;
            }
            else
            {
                itemCollection = "";
            }
            if (CollectionChanged != null)
                CollectionChanged(null, new EventArgs());
        }

        public static void MoveUp(string s)
        {
            string[] items = Items();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == s)
                {
                    if (i > 0)
                    {
                        string temp = items[i - 1];
                        items[i - 1] = items[i];
                        items[i] = temp;
                        break;
                    }
                }
            }
            UpdateFromArray(items);
        }

        public static void MoveDown(string s)
        {
            string[] items = Items();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == s)
                {
                    if (i < (items.Length - 1))
                    {
                        string temp = items[i + 1];
                        items[i + 1] = items[i];
                        items[i] = temp;
                        break;
                    }
                }
            }
            UpdateFromArray(items);
        }

    }
}
