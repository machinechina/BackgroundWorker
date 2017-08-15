using System.Threading;
using System.Windows.Forms;
using static Infrastructure.Native.DeviceChangeNotifierForm;

namespace Infrastructure.Native
{
    public class DeviceChangeNotifier
    {
        public static event DeviceNotifyDelegate DeviceNotify;

        public static void Start()
        {
            DeviceChangeNotifierForm.DeviceNotify -= DeviceNotify;
            DeviceChangeNotifierForm.DeviceNotify += DeviceNotify;
            DeviceChangeNotifierForm.Start();
        }

        public static void Stop()
        {
            DeviceChangeNotifierForm.DeviceNotify -= DeviceNotify;
            DeviceChangeNotifierForm.Stop();
        }
    }


    public class DeviceChangeNotifierForm : Form
    {
        public delegate void DeviceNotifyDelegate(Message msg);

        public static event DeviceNotifyDelegate DeviceNotify;

        private static DeviceChangeNotifierForm mInstance;
        private static bool _isRunning = false;

        public static void Start()
        {
            if (_isRunning)
                return;
            _isRunning = true;

            Thread t = new Thread(runForm);
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }

        public static void Stop()
        {
            if (mInstance == null)
                return;//Notifier not started
            DeviceNotify = null;
            mInstance.Invoke(new MethodInvoker(mInstance.endForm));
            _isRunning = false;
        }

        private static void runForm()
        {
            Application.Run(new DeviceChangeNotifierForm());
        }

        private void endForm()
        {
            this.Close();
        }

        protected override void SetVisibleCore(bool value)
        {
            // Prevent window getting visible
            if (mInstance == null) CreateHandle();
            mInstance = this;
            value = false;
            base.SetVisibleCore(value);
        }

        protected override void WndProc(ref Message m)
        {
            // Trap WM_DEVICECHANGE
            if (m.Msg == 0x219)
            {
                DeviceNotify?.Invoke(m);
            }

            base.WndProc(ref m);
        }
    }
}