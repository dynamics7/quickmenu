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
using System.Threading;
using System.IO.IsolatedStorage;
using System.ComponentModel;

namespace QuickMenu
{
    public class MainViewModel : BaseViewModel
    {

        public MainViewModel()
        {
            if (!DesignerProperties.IsInDesignTool)
                SpecialItemViewModel.WorkerStateChanged += new EventHandler(SpecialItemViewModel_WorkerStateChanged);
        }

        private void SpecialItemViewModel_WorkerStateChanged(object sender, EventArgs e)
        {
            var args = e as WorkerStateEventArgs;
            IsWorkerBusy = args.IsBusy;
            if (BusyStateChanged != null)
                BusyStateChanged(this, new EventArgs());
        }

        public event EventHandler BusyStateChanged;

        #region "Busy state handling"

        private bool _isLoadingList = false;
        public bool IsLoadingList
        {
            get
            {
                return _isLoadingList;
            }
            private set
            {
                _isLoadingList = value;
                OnChange("IsLoadingList");
                OnChange("IsBusy");
                OnChange("ShowProgressBar");
            }
        }

        private bool _isWorkerBusy = false;
        public bool IsWorkerBusy
        {
            get
            {
                return _isWorkerBusy;
            }
            set
            {
                _isWorkerBusy = value;
                OnChange("IsWorkerBusy");
                OnChange("IsBusy");
                OnChange("ShowProgressBar");
            }
        }

        /// <summary>
        /// Returns true if any long-term operation is in progress.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isLoadingList | _isWorkerBusy;
            }
        }

        /// <summary>
        /// Show global progress bar or not (depends on IsBusy)
        /// </summary>
        public Visibility ShowProgressBar
        {
            get
            {
                return IsBusy ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region "App list"

        private static List<ItemViewModel> _itemCache = null;

        /// <summary>
        /// Loads items asynchronously.
        /// </summary>
        public void LoadItemListAsync()
        {
            if (_itemCache == null)
            {
                var thread = new Thread(LoadItemsThread);
                thread.Start();
            }
        }

        /// <summary>
        /// Full item list
        /// </summary>
        public List<ItemViewModel> Items
        {
            get
            {
                return _itemCache;
            }
            set
            {
                _itemCache = value;
                OnChange("Items");
            }
        }

        private void LoadItemsThread()
        {
            var dispatcher = System.Windows.Deployment.Current.Dispatcher;
            dispatcher.BeginInvoke(new Action(() =>
            {
                IsLoadingList = true;
                if (BusyStateChanged != null)
                    BusyStateChanged(this, new EventArgs());
            }));
            _pinnedItemCache = GetActualPinnedItemList();
            dispatcher.BeginInvoke(new Action(() =>
           {
               OnChange("PinnedItems");
           }));
            var list = GetActualItemsList();
            list.Sort(new ItemViewModelComparer());
            
            _itemCache = list;
            dispatcher.BeginInvoke(new Action(() =>
            {
                IsLoadingList = false;
                OnChange("Items");
                if (BusyStateChanged != null)
                    BusyStateChanged(this, new EventArgs());
            }));
        }

        private static List<ItemViewModel> GetActualItemsList()
        {
            var list = new List<ItemViewModel>();
            foreach (var item in SpecialItemRepository.Items)
            {
                list.Add(item);
            }
            var apps = ApplicationApi.GetAllVisibleApplications();
            foreach (var app in apps)
            {
                string guid = app.ProductID().ToString().ToLower();
                if (guid.Contains("9b921ed5-73a9-4b36-88ea-1b8db509a5be"))
                {
                    string name = app.Title;
                    string imgpath = app.ImagePath;
                    guid = "{" + guid.ToUpper() + "}";
                    //bool uninstallable = app.IsUninstallable;
                    //var a = new ApplicationApi.Application(Guid.Parse(guid));
                    //a.Uninstall();
                    //Thread.Sleep(3000);
                }
                var item = new ItemViewModel(app.Title, app.Author, null);
                item.ApplicationInfo = app;
                item.IconUri = app.ApplicationIcon;
                list.Add(item);
            }
            /*
            var apps = ApplicationApi.GetAllVisibleApplications();
            foreach (var app in apps)
            {
                var item = new ItemViewModel(app.Title(), app.Author(), null);
                item.ApplicationInfo = app;
                item.IconUri = app.ApplicationIcon;
                list.Add(item);
            }*/
            return list;
        }

        #endregion

        #region "Pinned item list"

        private List<ItemViewModel> _pinnedItemCache = null;

        /// <summary>
        /// Pinned item list
        /// </summary>
        public List<ItemViewModel> PinnedItems
        {
            get
            {
                /*
                if (_pinnedItemCache == null)
                    _pinnedItemCache = GetActualPinnedItemList();
                */
                return _pinnedItemCache;
            }
            set
            {
                _pinnedItemCache = value;
                OnChange("PinnedItems");
            }
        }

        /// <summary>
        /// Reload pinned item list.
        /// </summary>
        public void LoadPinnedItemList()
        {
            PinnedItems = GetActualPinnedItemList();
        }

        private static List<ItemViewModel> GetActualPinnedItemList()
        {
            FavoriteItemCollection.Ping();
            List<ItemViewModel> list = new List<ItemViewModel>();
            var lines = FavoriteItemCollection.Items();
            if (lines != null)
            {
                int x = 0;
                foreach (var line in lines)
                {
                    var item = ItemFactory.Create(line.Replace("fav", ""), x);
                    if (item != null)
                    {
                        list.Add(item);
                        x++;
                    }
                }
            }
            return list;
        }


        #endregion

    }
}
