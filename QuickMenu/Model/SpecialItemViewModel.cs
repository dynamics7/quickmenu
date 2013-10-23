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
using Microsoft.Phone.Tasks;
using System.Threading;
using System.IO.IsolatedStorage;
namespace QuickMenu
{
    public class SpecialItemViewModel : ItemViewModel
    {

        static SpecialItemViewModel()
        {
            PreloadStates();
        }
        public SpecialItemViewModel()
        {
            IsSpecial = true;
        }

        public static event EventHandler WorkerStateChanged;

        public SpecialItemViewModel(string internalName, int index)
        {
            InternalName = internalName;
            SpecialItemIndex = index;
            IsSpecial = true;
            IconUri = "/Images/item" + internalName + ".png";
            ToggleVisible = Visibility.Visible;
            _isEnabled = StateFromPreloadedList();
            OnChange("IsEnabled");

            if (internalName == "Search" || internalName == "InternetSharing")
                ToggleVisible = Visibility.Collapsed;
        }

        public override string Title
        {
            get
            {
                string internalName = InternalName;
                if (internalName == "Bluetooth")
                    return LocalizedResources.Bluetooth;
                else if (internalName == "Wifi")
                    return LocalizedResources.Wifi;
                else if (internalName == "Phone")
                    return LocalizedResources.Phone;
                else if (internalName == "BatterySaving")
                    return LocalizedResources.BatterySaving;
                else if (internalName == "DataConnection")
                    return LocalizedResources.DataConnection;
                else if (internalName == "Search")
                    return LocalizedResources.Search;
                else if (internalName == "Accelerometer")
                    return LocalizedResources.Accelerometer;
                else if (internalName == "InternetSharing")
                    return LocalizedResources.InternetSharing;
                return "<noname>";
            }
            set
            {
            }
        }

        #region "Change state code"
        private class ChangeStateParam
        {
            public uint deviceType;
            public uint value;
        }

        private Object ChangeStateSyncObject = new Object();

        private void ChangeStateThread(object p)
        {
            Monitor.Enter(ChangeStateSyncObject);
            var dispatcher = System.Windows.Deployment.Current.Dispatcher;
            if (WorkerStateChanged != null)
            {
                dispatcher.BeginInvoke(new Action(() =>
                {
                    var args = new WorkerStateEventArgs();
                    args.IsBusy = true;
                    WorkerStateChanged(this, args);
                }));
            }
            var param = p as ChangeStateParam;
            InteropSvc.InteropLib.Instance.SetRadioState(param.deviceType, param.value, 0U);
            if (WorkerStateChanged != null)
            {
                dispatcher.BeginInvoke(new Action(() =>
                {
                    var args = new WorkerStateEventArgs();
                    args.IsBusy = false;
                    WorkerStateChanged(this, args);
                }));
            }
            Monitor.Exit(ChangeStateSyncObject);
        }

        private static bool IsAccelerometerEnabled()
        {
            uint result = InteropSvc.InteropLib.Instance.GetPower("ACC1:", 1);
            return (result == 4) ? false : true;
        }

        private static bool SetAccelerometerMode(bool mode)
        {
            InteropSvc.InteropLib.Instance.EnableUiOrientationChange(mode);
            return InteropSvc.InteropLib.Instance.SetPower("ACC1:", 1, (mode == true) ? 0U : 4U);
        }

        private static uint dwWifi, dwPhone, dwBluetooth, dwBatterySaving, dwDataEnabled, dwAccel;
        public static void PreloadStates()
        {
            InteropSvc.InteropLib.Instance.GetRadioStates(out dwWifi, out dwPhone, out dwBluetooth);
            dwBatterySaving = InteropSvc.InteropLib.Instance.GetBatterySavingsMode();
            dwDataEnabled = InteropSvc.InteropLib.Instance.GetDataEnabled();
            dwAccel = IsAccelerometerEnabled() ? 1U : 0U;
        }

        #endregion

        private bool _isEnabled = false;

        /// <summary>
        /// returns true if underlying object's state is "enabled"
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (_isEnabled != value)
                {
                    uint radioType = 555U;
                    uint radioVal = (value == true) ? 1U : 0U;

                    string internalName = InternalName;
                    if (internalName == "Bluetooth")
                    {
                        radioType = (uint)InteropSvc.InteropLib.RadioDeviceType.RADIODEVICES_BLUETOOTH;
                        //InteropSvc.InteropLib.Instance.SetRadioState((uint)InteropSvc.InteropLib.RadioDeviceType.RADIODEVICES_BLUETOOTH, (value == true) ? 1U : 0U, 0U);
                    }
                    else if (internalName == "Phone")
                    {
                        radioType = (uint)InteropSvc.InteropLib.RadioDeviceType.RADIODEVICES_PHONE;
                        //InteropSvc.InteropLib.Instance.SetRadioState((uint)InteropSvc.InteropLib.RadioDeviceType.RADIODEVICES_PHONE, (value == true) ? 1U : 0U, 0U);
                    }
                    else if (internalName == "Wifi")
                    {
                        radioType = (uint)InteropSvc.InteropLib.RadioDeviceType.RADIODEVICES_WIFI;
                        //InteropSvc.InteropLib.Instance.SetRadioState((uint)InteropSvc.InteropLib.RadioDeviceType.RADIODEVICES_WIFI, (value == true) ? 1U : 0U, 0U);
                    }
                    else if (internalName == "BatterySaving")
                    {
                        InteropSvc.InteropLib.Instance.SetBatterySavingsMode((value == true) ? 1U : 0U);
                    }
                    else if (internalName == "DataConnection")
                    {
                        if (value == true)
                        {
                            int askEveryTime = 0;
                            if (IsolatedStorageSettings.ApplicationSettings.Contains("AskEveryTime"))
                                askEveryTime = (int)IsolatedStorageSettings.ApplicationSettings["AskEveryTime"];
                            
                            if (askEveryTime == 0 || askEveryTime == 1)
                            {
                                if (MessageBox.Show(LocalizedResources.DataConnectionWarning, LocalizedResources.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                                {
                                    value = false;
                                }
                                if (askEveryTime == 0)
                                {
                                    IsolatedStorageSettings.ApplicationSettings.Add("AskEveryTime", 0);
                                    if (MessageBox.Show(LocalizedResources.DataConnectionQuestion, LocalizedResources.DataConnection, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                                    {
                                        IsolatedStorageSettings.ApplicationSettings["AskEveryTime"] = 1;
                                    }
                                    else
                                    {
                                        IsolatedStorageSettings.ApplicationSettings["AskEveryTime"] = 2;
                                    }
                                }
                            }
                        }
                        if (value == true)
                        {
                            InteropSvc.InteropLib.Instance.SetDataEnabled(1U);
                        }
                        else
                        {
                            InteropSvc.InteropLib.Instance.SetDataEnabled(0U);
                        }
                    }
                    else if (internalName == "Accelerometer")
                    {
                        SetAccelerometerMode(value);
                    }
                    
                    if (radioType != 555U)
                    {
                        var param = new ChangeStateParam();
                        param.deviceType = radioType;
                        param.value = radioVal;
                        var thread = new Thread(ChangeStateThread);
                        thread.Start(param);
                    }
                    _isEnabled = value;
                    OnChange("IsEnabled");
                    OnChange("SubText");
                }
            }
        }

        private bool StateFromPreloadedList()
        {
            bool state = false;
            string internalName = InternalName;
            switch (internalName)
            {
                case "Bluetooth":
                    state = (dwBluetooth > 0) ? true : false;
                    break;
                case "Wifi":
                    state = (dwWifi > 0) ? true : false;
                    break;
                case "Phone":
                    state = (dwPhone > 0) ? true : false;
                    break;
                case "BatterySaving":
                    state = (dwBatterySaving > 0) ? true : false;
                    break;
                case "DataConnection":
                    state = (dwDataEnabled > 0) ? true : false;
                    break;
                case "Accelerometer":
                    state = (dwAccel > 0) ? true : false;
                    break;

            }
            return state;
        }

        public override string SubText
        {
            get
            {
                if (ToggleVisible == Visibility.Visible)
                {
                    bool state = IsEnabled;

                    if (state == true)
                        return LocalizedResources.On;
                    else
                        return LocalizedResources.Off;
                }
                else
                {
                    return "";
                }
            }
            set
            {
            }
        }

        private Visibility _toggleVisible = Visibility.Collapsed;
        public override Visibility ToggleVisible
        {
            get
            {
                return _toggleVisible;
            }
            set
            {
                if (_toggleVisible != value)
                {
                    _toggleVisible = value;
                    OnChange("ToggleVisible");
                }
            }
        }

        public override void OnClick()
        {
            string internalName = InternalName;
            if (internalName == "Bluetooth")
            {
                var task = new ConnectionSettingsTask();
                task.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
                task.Show();
            }
            else if (internalName == "Wifi")
            {
                var task = new ConnectionSettingsTask();
                task.ConnectionSettingsType = ConnectionSettingsType.WiFi;
                task.Show();
            }
            else if (internalName == "Phone")
            {
                //ApplicationApi.LaunchSession("app://5B04B775-356B-4AA0-AAF8-6491FFEA5602/Start");
                ApplicationApi.LaunchSession("app://5B04B775-356B-4AA0-AAF8-6491FFEA561C/CallSettings");
            }
            else if (internalName == "Search")
            {
                ApplicationApi.LaunchSession("app://5B04B775-356B-4AA0-AAF8-6491FFEA5661/SearchHome?QuerySource=HardwareBtn");
            }
            else if (internalName == "BatterySaving")
            {
                ApplicationApi.LaunchSession("app://AFE91DD5-26FB-4ba6-A5A4-4BCEDE8FB3E6/_default");
            }
            else if (internalName == "DataConnection")
            {
                ApplicationApi.LaunchSession("app://5B04B775-356B-4AA0-AAF8-6491FFEA561F/_default");
            }
            else if (internalName == "Accelerometer")
            {
                ApplicationApi.LaunchSession("app://126b3759-8034-4fff-b987-3c0c6f9136f3/_default");
            }
            else if (internalName == "InternetSharing")
            {
                ApplicationApi.LaunchSession("app://5B04B775-356B-4AA0-AAF8-6491FFEA5629/_default");
            }
        }
    }
}
