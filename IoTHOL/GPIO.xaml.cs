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

using Windows.Devices.Gpio;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IoTHOL
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GPIO : Page, IDisposable
    {
        MainPage rootPage = MainPage.Current;

        private const int LED_PIN = 62;  
        private GpioPin pin;
        private GpioPinValue pinValue;
        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        public GPIO()
        {
            this.InitializeComponent();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //rootPage.NotifyUser("Status is Good", NotifyType.StatusMessage);
            //rootPage.NotifyUser("Status is Bad", NotifyType.ErrorMessage);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            InitGPIO();

            timer.Tick += Timer_Tick;            
            if (pin != null)
            {
                timer.Start();
            }
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                rootPage.NotifyUser("Status is Bad", NotifyType.ErrorMessage);
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            GpioStatus.Text = "GPIO pin initialized correctly.";
            rootPage.NotifyUser("Status is Good", NotifyType.StatusMessage);
        }

        private void Timer_Tick(object sender, object e)
        {
            if (pinValue == GpioPinValue.High)
            {
                pinValue = GpioPinValue.Low;
                pin.Write(pinValue);
                LED.Fill = redBrush;
            }
            else
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
                LED.Fill = grayBrush;
            }

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
            pin.Dispose();
            timer.Stop(); 
        }
    }
}
