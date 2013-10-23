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
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;

namespace QuickMenu
{
    public class ItemViewModel : BaseViewModel
    {

        public ItemViewModel()
        {
            //FavoriteItemCollection.CollectionChanged += new EventHandler(FavoriteItemCollection_CollectionChanged);
        }

        private void FavoriteItemCollection_CollectionChanged(object sender, EventArgs e)
        {    
            /*
            _isPinnedStateLoaded = false;
            OnChange("IsPinned");
            OnChange("IsPinnedVisibility");
             * */
        }
        public ItemViewModel(string title, string subtext, string uri)
        {
            _title = title;
            _subText = subtext;
            _iconUri = uri;
        }

        public ItemViewModel(ApplicationApi.ApplicationInfo newApplicationInfo)
        {
            ApplicationInfo = newApplicationInfo;
            if (ApplicationInfo != null)
            {
                _title = ApplicationInfo.Title;
                _subText = ApplicationInfo.Author;
                _iconUri = newApplicationInfo.ApplicationIcon;
            }
        }

        public int SpecialItemIndex { get; set; }

        private ApplicationApi.ApplicationInfo _applicationInfo = null;

        /// <summary>
        /// Application info store.
        /// </summary>
        public ApplicationApi.ApplicationInfo ApplicationInfo
        {
            get
            {
                return _applicationInfo;
            }
            set
            {
                if (_applicationInfo != value)
                {
                    _applicationInfo = value;
                    if (value != null)
                    {
                        Title = value.Title;
                        SubText = value.Author;

                        OnChange("Title");
                        OnChange("SubText");
                    }
                }
            }
        }


        /// <summary>
        /// Toggle state
        /// </summary>
        public virtual bool IsEnabled
        {
            get
            {
                return true;
            }
            set
            {
            }
        }


        private string _title;
        /// <summary>
        /// Title
        /// </summary>
        public virtual string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnChange("Title");
            }
        }

        private string _subText;
        /// <summary>
        /// SubText (usually Author's name)
        /// </summary>
        public virtual string SubText
        {
            get
            {
                return _subText;
            }
            set
            {
                _subText = value;
                OnChange("SubText");
            }
        }

        private string _iconUri = null;

        /// <summary>
        /// Application icon uri (absolute or relative)
        /// </summary>
        public string IconUri
        {
            get
            {
                return _iconUri;
            }
            set
            {
                _iconUri = value;
                OnChange("IconUri");
                OnChange("Icon");
            }
        }
        

        private BitmapImage _icon = null;
        private bool _iconTriedLoading = false;

        /// <summary>
        /// Application icon constructed using IconUri
        /// </summary>
        public BitmapImage Icon
        {
            get
            {
                if (_iconUri == null)
                    return null;
                if (_icon == null && _iconTriedLoading == false)
                {
                    if (_iconUri.Contains("res://") && _iconUri.IndexOf('!') != -1)
                        _iconUri = "/Images/" + _iconUri.Substring(_iconUri.IndexOf('!') + 1);
                    Uri uri = null;
                    try
                    {
                        uri = new Uri(_iconUri, UriKind.RelativeOrAbsolute);
                    }
                    catch (Exception ex)
                    {
                        uri = null;
                    }
                    if (uri == null)
                    {
                        try
                        {
                            uri = new Uri(_iconUri, UriKind.Relative);
                        }
                        catch (Exception ex)
                        {
                            uri = null;
                        }
                    }
                    //if (_iconUri.StartsWith("/Images/"))
                    //{
                    //    MessageBox.Show("Icon: " + _iconUri, Title, MessageBoxButton.OK);
                    //}
                    if (uri != null)
                    {
                        _icon = new BitmapImage(uri);
                    }

                    _iconTriedLoading = true;
                }
                return _icon;
            }
        }


        private bool _isSpecial = false;

        /// <summary>
        /// Returns true if item is special.
        /// Special are usually items not present in start menu by default, or those having toggle.
        /// </summary>
        public bool IsSpecial 
        { 
            get 
            {
                return _isSpecial;
            } 
            set 
            {
                _isSpecial = value;
                OnChange("IsSpecial");
                OnChange("Hash");
            }
        }

        /// <summary>
        /// Internal name:
        /// * for standard items: String representation of GUID
        /// * for special items: short name (like "Bluetooth")
        /// </summary>
        public string InternalName { get; set; }


        private string _hashCalculated = null;
        public string Hash
        {
            get
            {
                if (IsSpecial == true)
                    return InternalName;
                if (_hashCalculated == null)
                    _hashCalculated = "{" + _applicationInfo.ProductID().ToString() + "}";
                return _hashCalculated;
            }
        }


       
        /* cached */
        private bool _isPinned = false;
        private bool _isPinnedStateLoaded = false;

        /// <summary>
        /// Pin state. Returns true if item is pinned.
        /// </summary>
        public bool IsPinned
        {
            get
            {
                if (_isPinnedStateLoaded == false)
                    _isPinned = FavoriteItemCollection.Contains("fav" + Hash);
                return _isPinned;
            }
            set
            {
                if (_isPinned != value || _isPinnedStateLoaded == false)
                {
                    _isPinned = value;
                    _isPinnedStateLoaded = true;
                    if (value == true)
                    {
                        if (!FavoriteItemCollection.Contains("fav" + Hash))
                            FavoriteItemCollection.Add("fav" + Hash);
                    }
                    else
                    {
                        FavoriteItemCollection.Remove("fav" + Hash);
                    }
                    OnChange("IsPinned");
                    OnChange("IsPinnedVisibility");
                    OnChange("PinUnpinText");
                }
            }
        }

        /// <summary>
        /// IsPinned-analogue for Visibility.
        /// </summary>
        public Visibility IsPinnedVisibility
        {
            get
            {
                return (IsPinned == true) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Returns toggle visibility state
        /// </summary>
        public virtual Visibility ToggleVisible
        {
            get
            {
                return Visibility.Collapsed;
            }
            set
            {
            }
        }

        /// <summary>
        /// On-click handler
        /// </summary>
        public virtual void OnClick()
        {
            if (_applicationInfo != null)
            {
                ApplicationApi.LaunchSession(_applicationInfo.GetUri());
            }
        }

        public string PinUnpinText
        {
            get
            {
                if (IsPinned)
                    return LocalizedResources.Unpin;
                else
                    return LocalizedResources.Pin;
            }
        }
    }
}
