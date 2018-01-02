using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using System.Diagnostics;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTHOL
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class I2C : Page, IDisposable
    {
        MainPage rootPage = MainPage.Current;

        private const string I2C_CONTROLLER_NAME = "I2C0";

        private I2cDevice htdu21d;

        private DispatcherTimer ReadSensorTimer;

        private HTU21D HTU21DSensor;

        private static float humidity = 0;
        private static float temperature = 0;
        string time;

        public I2C()
        {
            this.InitializeComponent();            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            InitializeI2CDevice();            
        }

        private async void InitializeI2CDevice()
        {
            try
            {
                //Todo
                string advanced_query_syntax = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);
                DeviceInformationCollection device_information_collection =
                    await DeviceInformation.FindAllAsync(advanced_query_syntax);
                string deviceId = device_information_collection[0].Id;

                I2cConnectionSettings htdu21d_connection =
                    new I2cConnectionSettings(HTU21D.HTDU21D_I2C_ADDRESS);
                htdu21d_connection.BusSpeed = I2cBusSpeed.FastMode;
                htdu21d_connection.SharingMode = I2cSharingMode.Shared;

                htdu21d = await I2cDevice.FromIdAsync(deviceId, htdu21d_connection);
                rootPage.NotifyUser("Status is Good", NotifyType.StatusMessage);

                HTU21DSensor = new HTU21D(ref htdu21d);

                if (HTU21DSensor != null)
                {
                    ReadSensorTimer = new DispatcherTimer();
                    ReadSensorTimer.Interval = TimeSpan.FromMilliseconds(3000);
                    ReadSensorTimer.Tick += Timer_Tick;
                    ReadSensorTimer.Start();
                }
                
            }
            catch (Exception e)
            {
                HTU21DSensor = null; 
                Debug.WriteLine(e.ToString());
                rootPage.NotifyUser("InitializeI2CDevice error", NotifyType.ErrorMessage);
                return;
            }            
        }
        

        private async void Timer_Tick(object sender, object e)
        {
            //Todo 
            humidity = HTU21DSensor.Humidity();
            temperature = HTU21DSensor.Temperature();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                    () =>
                    {
                        // UI can be accessed here
                        UpdateScreen();
                    });

            Debug.WriteLine("Humidity : " + humidity + " Temperature : " + temperature);
        }

        private void UpdateScreen()
        {
            time = DateTime.Now.ToLocalTime().ToString();
            DateTime newDateTime = DateTime.Parse(time);
            DateTime koreaDateTime = newDateTime.AddHours(17);

            TimeStamp.Text = koreaDateTime.ToString();

            Humidty.Text = string.Format("{0:N2}%RH", humidity);
            Temperature.Text = string.Format("{0:N2}C", temperature);
        }

        

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward && e.Uri == null)
            {
                return;
            }

            Dispose();
        }

        public void Dispose()
        {
            if (HTU21DSensor != null)
            {
                ReadSensorTimer.Stop();
                htdu21d.Dispose();
            }
        }
    }
}
