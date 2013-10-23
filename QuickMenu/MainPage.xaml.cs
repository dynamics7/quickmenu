using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

namespace QuickMenu
{
    public partial class MainPage : PhoneApplicationPage
    {

        private MainViewModel viewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }


        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            Microsoft.Phone.Shell.SystemTray.IsVisible = (e.Orientation == PageOrientation.Portrait || e.Orientation == PageOrientation.PortraitUp ||
                e.Orientation == PageOrientation.PortraitDown);
        }

        private bool _prevWorkerState = false, _prevLoadingListState = false;
        private void viewModel_BusyStateChanged(object sender, EventArgs e)
        {
            if (_prevWorkerState != viewModel.IsWorkerBusy)
            {
                _prevWorkerState = viewModel.IsWorkerBusy;
                if (_prevWorkerState == true)
                {
                    VisualStateManager.GoToState(this, "Busy", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Normal", true);
                }
            }
            if (_prevLoadingListState != viewModel.IsLoadingList)
            {
                _prevLoadingListState = viewModel.IsLoadingList;
            }
        }


        private void lstApplications_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        }

        public static T FindParent<T>(UIElement control) where T : UIElement
        {
            UIElement p = VisualTreeHelper.GetParent(control) as UIElement;
            if (p != null)
            {
                if (p is T)
                    return p as T;
                else
                    return FindParent<T>(p);
            }
            return null;
        }

        private void lstApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as ItemViewModel;
                item.OnClick();
                //throw new Exception("Quit");
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            InteropSvc.InteropLib.Initialize();
            if (InteropSvc.InteropLib.Instance.HasRootAccess() == false)
            {
                MessageBox.Show(LocalizedResources.NoRootAccess, LocalizedResources.Error, MessageBoxButton.OK);
                throw new Exception("Quit");
            }
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("notFirstRun"))
            {
                FavoriteItemCollection.Add("favPhone");
                FavoriteItemCollection.Add("favBluetooth");
                FavoriteItemCollection.Add("favWifi");
                FavoriteItemCollection.Add("favDataConnection");
                FavoriteItemCollection.Add("favBatterySaving");
                FavoriteItemCollection.Add("favSearch");
                FavoriteItemCollection.Add("fav" + "{0be0455c-c8d5-df11-a844-00237de2db9e}");
                IsolatedStorageSettings.ApplicationSettings.Add("notFirstRun", "");
                FavoriteItemCollection.Flush();
            }
            viewModel.BusyStateChanged += new EventHandler(viewModel_BusyStateChanged);
            viewModel.LoadItemListAsync();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ItemViewModel item = null;
            if (myPanorama.SelectedIndex == 0)
            {
                ListBoxItem selectedListBoxItem = lstPinned.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
                item = lstPinned.ItemContainerGenerator.ItemFromContainer(selectedListBoxItem) as ItemViewModel;
            }
            else
            {
                ListBoxItem selectedListBoxItem = lstApplications.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
                item = lstApplications.ItemContainerGenerator.ItemFromContainer(selectedListBoxItem) as ItemViewModel;
            }
            item.IsPinned = !item.IsPinned;
            if (myPanorama.SelectedIndex == 0)
            {
                foreach (var it in viewModel.Items)
                {
                    if (it.Title == item.Title)
                    {
                        it.IsPinned = false;
                    }
                }
            }
            FavoriteItemCollection.Flush();
            viewModel.LoadPinnedItemList();
        }


        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            ItemViewModel item = null;
            if (myPanorama.SelectedIndex == 0)
            {
                ListBoxItem selectedListBoxItem = lstPinned.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
                item = lstPinned.ItemContainerGenerator.ItemFromContainer(selectedListBoxItem) as ItemViewModel;
            }
            if (item != null)
                FavoriteItemCollection.MoveUp("fav" + item.Hash);
            FavoriteItemCollection.Flush();
            viewModel.LoadPinnedItemList();

        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            ItemViewModel item = null;
            if (myPanorama.SelectedIndex == 0)
            {
                ListBoxItem selectedListBoxItem = lstPinned.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
                item = lstPinned.ItemContainerGenerator.ItemFromContainer(selectedListBoxItem) as ItemViewModel;
            }
            if (item != null)
                FavoriteItemCollection.MoveDown("fav" + item.Hash);
            FavoriteItemCollection.Flush();
            viewModel.LoadPinnedItemList();
        }

        private void ShowMenuItem(MenuItem item, bool show)
        {
            item.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            object v = sender;
            var menu = sender as ContextMenu;
            bool show = false;
            if (myPanorama.SelectedIndex == 0)
            {
                show = true;
            }
            ShowMenuItem(menu.Items[0] as MenuItem, show);
            ShowMenuItem(menu.Items[1] as MenuItem, show);
        }
    }
}