﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JeehellRMP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static WindowInteropHelper Window;

        public MainWindow()
        {
            InitializeComponent();

            /* To get data from SimConnect you either poll at fixed intervals (bad) or you use the "old" windows
             * messaging to let SimConnect signal you when it has something to say. To do this you need to pass the handle
             * of the window that will receive the messages and handle that reception (traditionally by overriding DefWndProc).
             * 
             * WPF doesn't want anything to do with the Win32 past so all window messaging is gone. Fortunately 
             * class System.Windows.Interop is there to bring back these features.
             * 
             * TODO: Investigate if it is possible to create a (hidden) traditional W32 window inside SimData class and have that receive
             * messages or alternatively move all Interop code in SimData. MainWindow shouldn't really know anything about SimData. The
             * static Window variable is particularly ugly and SimData should probably be able to use reflection on the Application to
             * figure it out.
             */
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(Window.Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == SimData.WM_USER_SIMCONNECT)
            {
                try
                {
                    SimData.ReceiveMessage();
                }
                catch (COMException ex)
                {
                    //TODO: An exception is firing almost every time FS is closed. My guess is that a message is sent when simconnect discovers
                    //it is going away but by the we process it, it is already gone, causing a method call on a non existent object.
                    //TODO: The exception handler can probably be moved into the SimData class
                    Debug.WriteLine("MainWindow:COMException {0}:{1}", ex.ErrorCode, ex.Message);

                    //TODO: Remove messagebox after proper logging is available. Left it here to nag me into fixing this.
                    MessageBox.Show("MainWindow:COMException" + ex.ErrorCode + ":" + ex.Message);
                }
            }
            return IntPtr.Zero;
        }

        private void buttonTransfer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SimData.TransferKeyPressed();
        }

        private void Knob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var knob = sender as Ellipse;
            SimData.Knob knobTurned = (knob.Name == "OuterKnob") ? SimData.Knob.OuterKnob : SimData.Knob.InnerKnob;
            SimData.KnobDirection knobTurnDirection = e.LeftButton.HasFlag(MouseButtonState.Pressed) ? SimData.KnobDirection.CounterClockWise : SimData.KnobDirection.Clockwise;

            SimData.KnobTurned(knobTurned, knobTurnDirection);
        }
    }
}