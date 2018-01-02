using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace IoTHOL
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "MDS Demo";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() {Title = "GPIO", ClassType = typeof(IoTHub)},
            new Scenario() {Title = "I2C", ClassType = typeof(I2C)},
            new Scenario() {Title = "IoT Hub", ClassType = typeof(IoTHub)}
        };
       

    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
