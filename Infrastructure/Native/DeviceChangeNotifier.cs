using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Infrastructure.Native
{
    public class DeviceChangeNotifier : Form
    {
        public delegate void DeviceNotifyDelegate(Message msg);

        public static event DeviceNotifyDelegate DeviceNotify;

        private static DeviceChangeNotifier mInstance;

        public static void Start()
        {
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
        }

        private static void runForm()
        {
            Application.Run(new DeviceChangeNotifier());
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