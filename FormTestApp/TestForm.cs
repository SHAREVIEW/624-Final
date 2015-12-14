///////////////////////////////////////////////////////////////////////////////
// TestForm.cs - Windows Forms test dialog for WintabDN
//
// Copyright (c) 2010, Wacom Technology Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using WintabDN;
using System.Threading;
using System.Windows;
using System.Diagnostics;

namespace FormTestApp
{
    public partial class TestForm : Form
    {
        private CWintabContext m_logContext = null; 
        private CWintabData m_wtData = null;
        private UInt32 m_maxPkts = 1;   // max num pkts to capture/display at a time

        private Int32 m_pkX = 0;
        private Int32 m_pkY = 0;
        private UInt32 m_pressure = 0;
        private UInt32 m_pkTime = 0;
        private UInt32 m_pkTimeLast = 0;

        private Point m_lastPoint = Point.Empty;
        private Graphics m_graphics;
        private Pen m_pen;
        private Pen m_backPen;
        private Pen test_pen;

        private int strokeNum;

        private bool autoRecognize;

        private int currentShape;

        private TemplateRecognizer TR;
        private ShapeTemplate Shapes;

        private System.Windows.Forms.Timer compareTimer;
        private System.Windows.Forms.Timer timeoutTimer;
        private System.Windows.Forms.Timer reenableTimer;

        private List<List<List<int>>> sets;
        private int currentSet;
        private int currentSeq;
        private int seqIndex;

        private double maxTime;
        private double timeLeft;
        private double timerInterval;

        private bool screenEnabled;
        private bool timeoutEnabled;

        private bool debugButtonsOn;
        private bool showCurrentShape;

		 // These constants can be used to force Wintab X/Y data to map into a
		 // a 10000 x 10000 grid, as an example of mapping tablet data to values
		 // that make sense for your application.
        private const Int32 m_TABEXTX = 10000;
        private const Int32 m_TABEXTY = 10000;

        private bool m_showingTextButton = true;

        ///////////////////////////////////////////////////////////////////////
        public TestForm()
        {
            InitializeComponent();
            FakeEnable();
            this.FormClosing += new FormClosingEventHandler(TestForm_FormClosing);
            TR = new TemplateRecognizer();
            Shapes = new ShapeTemplate();

            compareTimer = new System.Windows.Forms.Timer();
            compareTimer.Tick += new EventHandler(CompareShapes);
            compareTimer.Interval = 400;

            timerInterval = .1;

            timeoutTimer = new System.Windows.Forms.Timer();
            timeoutTimer.Tick += new EventHandler(TimeTick);
            timeoutTimer.Interval = (int)(timerInterval * 1000);

            reenableTimer = new System.Windows.Forms.Timer();
            reenableTimer.Tick += new EventHandler(ReenableScreen);
            reenableTimer.Interval = 3000;

            maxTime = 20;
            timeLeft = maxTime;

            strokeNum = 0;
            autoRecognize = true;
            loadSets();
            currentSet = 0;
            currentSeq = 0;
            seqIndex = 0;

            screenEnabled = true;
            timeoutEnabled = true;

            currentShape = sets[currentSet][currentSeq][seqIndex];
            CurrentTemplateLabel.Text = String.Format("{3}", currentSet, currentSeq, seqIndex, Shapes.getShape(currentShape).name);
            TimeLeftLabel.Text = String.Format("Time left: {0:F1} s", timeLeft);

            debugButtonsOn = false;
            showCurrentShape = true;

            this.KeyPreview = true;
            this.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
        }

        private void loadSets()
        {
            sets = new List<List<List<int>>>();
            //@"C:\Users\Admin\Documents\GitHub\624-Final\Templates\Sets.txt"
            //"..\..\..\..\Templates\Sets.txt"
            if (System.IO.File.Exists("../../../Templates/Sets.txt"))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader("../../../Templates/Sets.txt", true))
                {
                    while(true)
                    {
                        string seqNumStr = file.ReadLine();
                        if (seqNumStr == null)
                            break;
                        sets.Add(new List<List<int>>());

                        int numSequences = Int32.Parse(seqNumStr);
                        for(int i=0;i<numSequences;i++)
                        {
                            string seqStr = file.ReadLine();
                            //Console.WriteLine(seqStr);
                            string[] splitStr = seqStr.Split(',');
                            List<int> seq = new List<int>();
                            for(int j=0;j<splitStr.Length;j++)
                            {
                                seq.Add(Int32.Parse(splitStr[j]));
                            }
                            sets[sets.Count - 1].Add(seq);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("WARNING: No shape sets found!");
            }
        }

        ///////////////////////////////////////////////////////////////////////
        public HCTX HLogContext { get { return m_logContext.HCtx; } }

         ///////////////////////////////////////////////////////////////////////
        private void TestForm_FormClosing(Object sender, FormClosingEventArgs e)
        {
            CloseCurrentContext();
        }

        ///////////////////////////////////////////////////////////////////////
        

        private void FakeEnable()
        {
            ClearDisplay();
            Enable_Scribble(true);

            // Control the system cursor with the pen.
            // TODO: set to false to NOT control the system cursor with pen.
            bool controlSystemCursor = true;

            // Open a context and try to capture pen data;
            InitDataCapture(m_TABEXTX, m_TABEXTY, controlSystemCursor);
        }

        ///////////////////////////////////////////////////////////////////////

        /*
        private void scribbleButton_Click(object sender, EventArgs e)
        {
            ClearDisplay();
            Enable_Scribble(true);

            // Control the system cursor with the pen.
            // TODO: set to false to NOT control the system cursor with pen.
            bool controlSystemCursor = true;

            // Open a context and try to capture pen data;
            InitDataCapture(m_TABEXTX, m_TABEXTY, controlSystemCursor);
        }
        */
 
        ///////////////////////////////////////////////////////////////////////
        private void clearButton_Click(object sender, EventArgs e)
        {
            ClearDisplay();
            TR.ResetPoints(Shapes.getShape(currentShape), false, currentSet, currentSeq, seqIndex);
            strokeNum = 0;
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_IsWintabAvailable()
        {
            if (CWintabInfo.IsWintabAvailable())
            {
                TraceMsg("Wintab was found!\n");
            }
            else
            {
                TraceMsg("Wintab was not found!\nCheck to see if tablet driver service is running.\n");
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDeviceInfo()
        {
            //TraceMsg("DeviceInfo: " + CWintabInfo.GetDeviceInfo() + "\n");
            string devInfo = CWintabInfo.GetDeviceInfo();
            TraceMsg("DeviceInfo: " + devInfo + "\n");
        }


        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDefaultDigitizingContext()
        {
            CWintabContext context = CWintabInfo.GetDefaultDigitizingContext();

            TraceMsg("Default Digitizing Context:\n");
            TraceMsg("\tSysOrgX, SysOrgY, SysExtX, SysExtY\t[" +
                context.SysOrgX + "," + context.SysOrgY + "," +
                context.SysExtX + "," + context.SysExtY + "]\n");

            TraceMsg("\tInOrgX, InOrgY, InExtX, InExtY\t[" +
                context.InOrgX + "," + context.InOrgY + "," +
                context.InExtX + "," + context.InExtY + "]\n");

            TraceMsg("\tOutOrgX, OutOrgY, OutExtX, OutExt\t[" +
                context.OutOrgX + "," + context.OutOrgY + "," +
                context.OutExtX + "," + context.OutExtY + "]\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDefaultSystemContext()
        {
            CWintabContext context = CWintabInfo.GetDefaultSystemContext();

            TraceMsg("Default System Context:\n");
            TraceMsg("\tSysOrgX, SysOrgY, SysExtX, SysExtY\t[" +
                context.SysOrgX + "," + context.SysOrgY + "," +
                context.SysExtX + "," + context.SysExtY + "]\n");

            TraceMsg("\tInOrgX, InOrgY, InExtX, InExtY\t[" +
                context.InOrgX + "," + context.InOrgY + "," +
                context.InExtX + "," + context.InExtY + "]\n");

            TraceMsg("\tOutOrgX, OutOrgY, OutExtX, OutExt\t[" +
                context.OutOrgX + "," + context.OutOrgY + "," +
                context.OutExtX + "," + context.OutExtY + "]\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDefaultDeviceIndex()
        {
            Int32 devIndex = CWintabInfo.GetDefaultDeviceIndex();

            TraceMsg("Default device index is: " + devIndex + (devIndex == -1 ? " (virtual device)\n" : "\n"));
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDeviceAxis()
        {
            WintabAxis axis;

            // Get virtual device axis for X, Y and Z.
            axis = CWintabInfo.GetDeviceAxis(-1, EAxisDimension.AXIS_X);

            TraceMsg("Device axis X for virtual device:\n");
            TraceMsg("\taxMin, axMax, axUnits, axResolution: " + axis.axMin + "," + axis.axMax + "," + axis.axUnits + "," + axis.axResolution.ToString() + "\n");

            axis = CWintabInfo.GetDeviceAxis(-1, EAxisDimension.AXIS_Y);
            TraceMsg("Device axis Y for virtual device:\n");
            TraceMsg("\taxMin, axMax, axUnits, axResolution: " + axis.axMin + "," + axis.axMax + "," + axis.axUnits + "," + axis.axResolution.ToString() + "\n");

            axis = CWintabInfo.GetDeviceAxis(-1, EAxisDimension.AXIS_Z);
            TraceMsg("Device axis Z for virtual device:\n");
            TraceMsg("\taxMin, axMax, axUnits, axResolution: " + axis.axMin + "," + axis.axMax + "," + axis.axUnits + "," + axis.axResolution.ToString() + "\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDeviceOrientation()
        {
            bool tiltSupported = false;
            WintabAxisArray axisArray = CWintabInfo.GetDeviceOrientation(out tiltSupported);
            TraceMsg("Device orientation:\n");
            TraceMsg("\ttilt supported for current tablet: " + (tiltSupported ? "YES\n" : "NO\n"));

            if (tiltSupported)
            {
                for (int idx = 0; idx < axisArray.array.Length; idx++)
                {
                TraceMsg("\t[" + idx + "] axMin, axMax, axResolution, axUnits: " +
                    axisArray.array[idx].axMin + "," +
                    axisArray.array[idx].axMax + "," +
                    axisArray.array[idx].axResolution + "," +
                    axisArray.array[idx].axUnits + "\n");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDeviceRotation()
        {
            bool rotationSupported = false;
            WintabAxisArray axisArray = CWintabInfo.GetDeviceRotation(out rotationSupported);
            TraceMsg("Device rotation:\n");
            TraceMsg("\trotation supported for current tablet: " + (rotationSupported ? "YES\n" : "NO\n"));

            if (rotationSupported)
            {
                for (int idx = 0; idx < axisArray.array.Length; idx++)
                {
                    TraceMsg("\t[" + idx + "] axMin, axMax, axResolution, axUnits: " +
                        axisArray.array[idx].axMin + "," +
                        axisArray.array[idx].axMax + "," +
                        axisArray.array[idx].axResolution + "," +
                        axisArray.array[idx].axUnits + "\n");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetNumberOfDevices()
        {
            UInt32 numDevices = CWintabInfo.GetNumberOfDevices();
            TraceMsg("Number of tablets connected: " + numDevices + "\n");
        }


        ///////////////////////////////////////////////////////////////////////
        private void Test_IsStylusActive()
        {
            bool isStylusActive = CWintabInfo.IsStylusActive();
            TraceMsg("Is stylus active: " + (isStylusActive ? "YES\n" : "NO\n"));
        }


        ///////////////////////////////////////////////////////////////////////
        private void Test_GetStylusName()
        {
            TraceMsg("Stylus name (puck):   " + CWintabInfo.GetStylusName(EWTICursorNameIndex.CSR_NAME_PUCK) + "\n");
            TraceMsg("Stylus name (pen):    " + CWintabInfo.GetStylusName(EWTICursorNameIndex.CSR_NAME_PRESSURE_STYLUS) + "\n");
            TraceMsg("Stylus name (eraser): " + CWintabInfo.GetStylusName(EWTICursorNameIndex.CSR_NAME_ERASER) + "\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetExtensionMask()
        {
            TraceMsg("Extension touchring mask:   0x" + CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_TOUCHRING).ToString("x") + "\n");
            TraceMsg("Extension touchstring mask: 0x" + CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_TOUCHSTRIP).ToString("x") + "\n");
            TraceMsg("Extension express key mask: 0x" + CWintabExtensions.GetWTExtensionMask(EWTXExtensionTag.WTX_EXPKEYS2).ToString("x") + "\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_Context()
        {
            bool status = false;
            CWintabContext logContext = null;

            try
            {
                logContext = OpenTestDigitizerContext();
                if (logContext == null)
                {
                    TraceMsg("Test_Context: FAILED OpenTestDigitizerContext - bailing out...\n");
                    return;
                }

                status = logContext.Enable(true);
                TraceMsg("Context Enable: " + (status ? "PASSED" : "FAILED") + "\n");

                status = logContext.SetOverlapOrder(false);
                TraceMsg("Context SetOverlapOrder to bottom: " + (status ? "PASSED" : "FAILED") + "\n");
                status = logContext.SetOverlapOrder(true);
                TraceMsg("Context SetOverlapOrder to top: " + (status ? "PASSED" : "FAILED") + "\n");

                TraceMsg("Modified Context:\n");
                TraceMsg("  Name: " + logContext.Name + "\n");
                TraceMsg("  Options: " + logContext.Options + "\n");
                TraceMsg("  Status: " + logContext.Status + "\n");
                TraceMsg("  Locks: " + logContext.Locks + "\n");
                TraceMsg("  MsgBase: " + logContext.MsgBase + "\n");
                TraceMsg("  Device: " + logContext.Device + "\n");
                TraceMsg("  PktRate: 0x" + logContext.PktRate.ToString("x") + "\n");
                TraceMsg("  PktData: 0x" + ((uint)logContext.PktData).ToString("x") + "\n");
                TraceMsg("  PktMode: 0x" + ((uint)logContext.PktMode).ToString("x") + "\n");
                TraceMsg("  MoveMask: " + logContext.MoveMask + "\n");
                TraceMsg("  BZtnDnMask: 0x" + logContext.BtnDnMask.ToString("x") + "\n");
                TraceMsg("  BtnUpMask: 0x" + logContext.BtnUpMask.ToString("x") + "\n");
                TraceMsg("  InOrgX: " + logContext.InOrgX + "\n");
                TraceMsg("  InOrgY: " + logContext.InOrgY + "\n");
                TraceMsg("  InOrgZ: " + logContext.InOrgZ + "\n");
                TraceMsg("  InExtX: " + logContext.InExtX + "\n");
                TraceMsg("  InExtY: " + logContext.InExtY + "\n");
                TraceMsg("  InExtZ: " + logContext.InExtZ + "\n");
                TraceMsg("  OutOrgX: " + logContext.OutOrgX + "\n");
                TraceMsg("  OutOrgY: " + logContext.OutOrgY + "\n");
                TraceMsg("  OutOrgZ: " + logContext.OutOrgZ + "\n");
                TraceMsg("  OutExtX: " + logContext.OutExtX + "\n");
                TraceMsg("  OutExtY: " + logContext.OutExtY + "\n");
                TraceMsg("  OutExtZ: " + logContext.OutExtZ + "\n");
                TraceMsg("  SensX: " + logContext.SensX + "\n");
                TraceMsg("  SensY: " + logContext.SensY + "\n");
                TraceMsg("  SensZ: " + logContext.SensZ + "\n");
                TraceMsg("  SysMode: " + logContext.SysMode + "\n");
                TraceMsg("  SysOrgX: " + logContext.SysOrgX + "\n");
                TraceMsg("  SysOrgY: " + logContext.SysOrgY + "\n");
                TraceMsg("  SysExtX: " + logContext.SysExtX + "\n");
                TraceMsg("  SysExtY: " + logContext.SysExtY + "\n");
                TraceMsg("  SysSensX: " + logContext.SysSensX + "\n");
                TraceMsg("  SysSensY: " + logContext.SysSensY + "\n");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (logContext != null)
                {
                    status = logContext.Close();
                    TraceMsg("Context Close: " + (status ? "PASSED" : "FAILED") + "\n");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private CWintabContext OpenTestDigitizerContext(
            int width_I = m_TABEXTX, int height_I = m_TABEXTY, bool ctrlSysCursor = true)
        {
            bool status = false;
            CWintabContext logContext = null;

            try
            {
                // Get the default digitizing context.
                // Default is to receive data events.
                logContext = CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_MESSAGES);

                // Set system cursor if caller wants it.
                if (ctrlSysCursor)
                {
                    logContext.Options |= (uint)ECTXOptionValues.CXO_SYSTEM;
                }

                if (logContext == null)
                {
                    TraceMsg("FAILED to get default digitizing context.\n");
                    return null;
                }

                // Modify the digitizing region.
                logContext.Name = "WintabDN Event Data Context";

                // output in a grid of the specified dimensions.
                logContext.OutOrgX = logContext.OutOrgY = 0;
                logContext.OutExtX = width_I;
                logContext.OutExtY = height_I;


                // Open the context, which will also tell Wintab to send data packets.
                status = logContext.Open();

                TraceMsg("Context Open: " + (status ? "PASSED [ctx=" + logContext.HCtx + "]" : "FAILED") + "\n");
            }
            catch (Exception ex)
            {
                TraceMsg("OpenTestDigitizerContext ERROR: " + ex.ToString());
            }

            return logContext;
        }

        ///////////////////////////////////////////////////////////////////////
        private CWintabContext OpenTestSystemContext(
            int width_I = m_TABEXTX, int height_I = m_TABEXTY, bool ctrlSysCursor = true)
        {
            bool status = false;
            CWintabContext logContext = null;

            try
            {
                // Get the default system context.
                // Default is to receive data events.
                //logContext = CWintabInfo.GetDefaultDigitizingContext(ECTXOptionValues.CXO_MESSAGES);
                logContext = CWintabInfo.GetDefaultSystemContext(ECTXOptionValues.CXO_MESSAGES);

                // Set system cursor if caller wants it.
                if (ctrlSysCursor)
                {
                    logContext.Options |= (uint)ECTXOptionValues.CXO_SYSTEM;
                }
                else
                {
                    logContext.Options &= ~(uint)ECTXOptionValues.CXO_SYSTEM;
                }

                if (logContext == null)
                {
                    TraceMsg("FAILED to get default digitizing context.\n");
                    return null;
                }

                // Modify the digitizing region.
                logContext.Name = "WintabDN Event Data Context";

                WintabAxis tabletX = CWintabInfo.GetTabletAxis(EAxisDimension.AXIS_X);
                WintabAxis tabletY = CWintabInfo.GetTabletAxis(EAxisDimension.AXIS_Y);

                logContext.InOrgX = 0;
                logContext.InOrgY = 0;
                logContext.InExtX = tabletX.axMax;
                logContext.InExtY = tabletY.axMax;

                // SetSystemExtents() is (almost) a NO-OP redundant if you opened a system context.
                SetSystemExtents( ref logContext );

                // Open the context, which will also tell Wintab to send data packets.
                status = logContext.Open();

                TraceMsg("Context Open: " + (status ? "PASSED [ctx=" + logContext.HCtx + "]" : "FAILED") + "\n");
            }
            catch (Exception ex)
            {
                TraceMsg("OpenTestDigitizerContext ERROR: " + ex.ToString());
            }

            return logContext;
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_DataPacketQueueSize()
        {
            bool status = false;
            UInt32 numPackets = 0;
            CWintabContext logContext = null;

            try
            {
                logContext = OpenTestDigitizerContext();

                if (logContext == null)
                {
                    TraceMsg("Test_DataPacketQueueSize: FAILED OpenTestDigitizerContext - bailing out...\n");
                    return;
                }

                CWintabData wtData = new CWintabData(logContext);
                TraceMsg("Creating CWintabData object: " + (wtData != null ? "PASSED" : "FAILED") + "\n");
                if (wtData == null)
                {
                    throw new Exception("Could not create CWintabData object.");
                }

                numPackets = wtData.GetPacketQueueSize();
                TraceMsg("Initial packet queue size: " + numPackets + "\n");

                status = wtData.SetPacketQueueSize(17);
                TraceMsg("Setting packet queue size: " + (status ? "PASSED" : "FAILED") + "\n");

                numPackets = wtData.GetPacketQueueSize();
                TraceMsg("Modified packet queue size: " + numPackets + "\n");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (logContext != null)
                {
                    status = logContext.Close();
                    TraceMsg("Context Close: " + (status ? "PASSED" : "FAILED") + "\n");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_MaxPressure()
        {
            TraceMsg("Max normal pressure is: " + CWintabInfo.GetMaxPressure() + "\n");
            TraceMsg("Max tangential pressure is: " + CWintabInfo.GetMaxPressure(false) + "\n");
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_GetDataPackets(UInt32 maxNumPkts_I)
        {
            // Set up to capture/display maxNumPkts_I packet at a time.
            m_maxPkts = maxNumPkts_I;

            // Open a context and try to capture pen data.
            InitDataCapture();

            // Touch pen to the tablet.  You should see data appear in the TestForm window.
        }

        ///////////////////////////////////////////////////////////////////////
        private void Test_QueryDataPackets()
        {
            QueryDataForm qdForm = new QueryDataForm();

            qdForm.ShowDialog();

        }



        ///////////////////////////////////////////////////////////////////////
        private void Enable_Scribble(bool enable = false)
        {
            if (enable)
            {
                // Set up to capture 1 packet at a time.
                m_maxPkts = 1;

                // Init scribble graphics.
                //m_graphics = CreateGraphics();
                m_graphics = scribblePanel.CreateGraphics();
                m_graphics.SmoothingMode = SmoothingMode.AntiAlias;

                m_pen = new Pen(Color.Black);
                m_backPen = new Pen(Color.White);

                //scribbleButton.BackColor = Color.Lime;
                //scribbleLabel.Visible = true;
               

                // You should now be able to scribble in the scribblePanel.
            }
            else
            {
                // Remove scribble context.
                CloseCurrentContext();

                // Turn off graphics.
                if (m_graphics != null)
                {
                    scribblePanel.Invalidate();
                    m_graphics = null;
                }

                //scribbleButton.BackColor = Color.WhiteSmoke;
                //scribbleLabel.Visible = false;
                
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Helper functions
        //

        ///////////////////////////////////////////////////////////////////////
        private void InitDataCapture(
				int ctxWidth_I = m_TABEXTX, int ctxHeight_I = m_TABEXTY, bool ctrlSysCursor_I = true)
        {
            try
            {
                // Close context from any previous test.
                CloseCurrentContext();

                TraceMsg("Opening context...\n");

                m_logContext = OpenTestSystemContext(ctxWidth_I, ctxHeight_I, ctrlSysCursor_I);

                if (m_logContext == null)
                {
                    TraceMsg("Test_DataPacketQueueSize: FAILED OpenTestSystemContext - bailing out...\n");
                    return;
                }

                // Create a data object and set its WT_PACKET handler.
                m_wtData = new CWintabData(m_logContext);
                m_wtData.SetWTPacketEventHandler(MyWTPacketEventHandler);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        ///////////////////////////////////////////////////////////////////////
        private void CloseCurrentContext()
        {
            try
            {
                TraceMsg("Closing context...\n");
                if (m_logContext != null)
                {
                    m_logContext.Close();
                    m_logContext = null;
                    m_wtData = null;
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        ///////////////////////////////////////////////////////////////////////
        void TraceMsg(string msg)
        {
            /*
            testTextBox.AppendText(msg);

            // Scroll to bottom of list.
            testTextBox.SelectionLength = 0;
            testTextBox.SelectionStart = testTextBox.Text.Length;
            testTextBox.ScrollToCaret();
            */
        }

        ///////////////////////////////////////////////////////////////////////
        // Sets logContext.Out
        //
        // Note: 
        // SystemParameters.VirtualScreenLeft{Top} and SystemParameters.VirtualScreenWidth{Height} 
        // don't always give correct answers.
        //
        // Uncomment the TODO code below that enumerates all system displays 
        // if you want to customize.
        // Else assume the passed-in extents were already set by call to WTInfo,
        // in which case we still have to invert the Y extent.
        private void SetSystemExtents(ref CWintabContext logContext)
        {
           //TODO Rectangle rect = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

           //TODO foreach (Screen screen in Screen.AllScreens)
           //TODO    rect = Rectangle.Union(rect, screen.Bounds);

           //TODO logContext.OutOrgX = rect.Left;
           //TODO logContext.OutOrgY = rect.Top;
           //TODO logContext.OutExtX = rect.Width;
           //TODO logContext.OutExtY = rect.Height;

           // In Wintab, the tablet origin is lower left.  Move origin to upper left
           // so that it coincides with screen origin.
           logContext.OutExtY = -logContext.OutExtY;
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Called when Wintab WT_PACKET events are received.
        /// </summary>
        /// <param name="sender_I">The EventMessage object sending the report.</param>
        /// <param name="eventArgs_I">eventArgs_I.Message.WParam contains ID of packet containing the data.</param>
        public void MyWTPacketEventHandler(Object sender_I, MessageReceivedEventArgs eventArgs_I)
        {
            //System.Diagnostics.Debug.WriteLine("Received WT_PACKET event");
            if (m_wtData == null)
            {
                return;
            }

            try
            {
                if (m_maxPkts == 1)
                {
                    uint pktID = (uint)eventArgs_I.Message.WParam;
                    WintabPacket pkt = m_wtData.GetDataPacket((uint)eventArgs_I.Message.LParam, pktID);
                    //DEPRECATED WintabPacket pkt = m_wtData.GetDataPacket(pktID);

                    if (pkt.pkContext != 0)
                    {
                        m_pkX = pkt.pkX;
                        m_pkY = pkt.pkY;
                        m_pressure = pkt.pkNormalPressure;

								//Trace.WriteLine("SCREEN: pkX: " + pkt.pkX + ", pkY:" + pkt.pkY + ", pressure: " + pkt.pkNormalPressure);

                        m_pkTime = pkt.pkTime;
                        //Console.WriteLine("time: " + m_pkTime);
                        if (m_graphics == null)
                        {
                            // display data mode
                            TraceMsg("Received WT_PACKET event[" + pktID + "]: X/Y/P = " + 
                                pkt.pkX + " / " + pkt.pkY + " / " + pkt.pkNormalPressure + "\n");
                        }
                        else
                        {
                            // scribble mode
                            int clientWidth = scribblePanel.Width;
                            int clientHeight = scribblePanel.Height;

                            // m_pkX and m_pkY are in screen (system) coordinates.

                            /*
                            //Converts the program to use relative coordinates.
                            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

                            double percentX = m_pkX / (double)workingRectangle.Width;
                            double percentY = m_pkY / (double)workingRectangle.Height;

                            int newX = (int)(percentX * clientWidth);
                            int newY = (int)(percentY * clientHeight);

                            Point clientPoint = new Point(newX, newY);
                            */


                            Point clientPoint = scribblePanel.PointToClient(new Point(m_pkX, m_pkY));

                            if (clientPoint.X < 0)
                                return;
                            if (clientPoint.Y < 0)
                                return;

                            if (clientPoint.X > scribblePanel.Width)
                                return;
                            if (clientPoint.Y > scribblePanel.Height)
                                return;

                            //Trace.WriteLine("CLIENT:   X: " + clientPoint.X + ", Y:" + clientPoint.Y);

                            if (m_lastPoint.Equals(Point.Empty))
                            {
                                m_lastPoint = clientPoint;
                                m_pkTimeLast = m_pkTime;
                            }

                            //m_pen.Width = (float)(m_pressure / 200);
                            m_pen.Width = (float)3.0;

                            if (m_pressure > 0 && screenEnabled)
                            {
                                if(!timeoutTimer.Enabled && timeoutEnabled && currentSeq != 0)
                                {
                                    timeoutTimer.Start();
                                }
                                if(compareTimer.Enabled)
                                {
                                    compareTimer.Stop();
                                }

                                uint unixTimestamp = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                                if (m_pkTime - m_pkTimeLast > 50)
                                {
                                    Console.WriteLine("new stroke");
                                    m_graphics.DrawRectangle(m_pen, clientPoint.X, clientPoint.Y, 1, 1);
                                    strokeNum++;
                                    DrawPoint dp = new DrawPoint(clientPoint.X, clientPoint.Y, strokeNum, unixTimestamp, m_pressure);
                                    TR.AddStartPoint(dp);
                                    m_lastPoint = clientPoint;
                                    m_pkTimeLast = m_pkTime;
                                }
                                else
                                {
                                    m_graphics.DrawLine(m_pen, clientPoint, m_lastPoint);
                                    DrawPoint dp = new DrawPoint(clientPoint.X, clientPoint.Y, strokeNum, unixTimestamp, m_pressure);
                                    TR.AddPoint(dp);
                                    compareTimer.Start();
                                    m_lastPoint = clientPoint;
                                    m_pkTimeLast = m_pkTime;
                                }
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FAILED to get packet data: " + ex.ToString());
            }
        }

        private void ClearDisplay()
        {
            //testTextBox.Clear();
            scribblePanel.Invalidate();
        }

        private void testQDPButton_Click(object sender, EventArgs e)
        {
            Test_QueryDataPackets();
        }

		  private void scribblePanel_Resize(object sender, EventArgs e)
		  {
			  if (m_graphics != null)
			  {
				  m_graphics.Dispose();
				  m_graphics = scribblePanel.CreateGraphics();
				  m_graphics.SmoothingMode = SmoothingMode.AntiAlias;

				  Trace.WriteLine(
					  "ScribblePanel: X:" + scribblePanel.Left + ",Y:" +  scribblePanel.Top + 
					  ", W:" + scribblePanel.Width + ", H:" + scribblePanel.Height);
			  }
		  }

        private void button1_Click(object sender, EventArgs e)
        {
            AttemptRecognition();
        }

        private bool AttemptRecognition()
        {
            if (TR.TryRecognizeShape(Shapes.getShape(currentShape)))
            {
                Console.WriteLine("Success");
                //label1.Visible = true;
                //TemplateNoLabel.Visible = false;
                //DrawGrid(Color.Green);
                return true;
            }
            else
            {
                Console.WriteLine("Fail");
                //label1.Visible = false;
                //TemplateNoLabel.Visible = true;
                DrawGrid(Color.Red);
                return false;
            }
        }

        private void DrawGrid(Color c)
        {
            List<DrawPoint> p= TR.getPoints();

            double minX = p[0].X;
            double minY = p[0].Y;
            double maxX = p[0].X;
            double maxY = p[0].Y;

            for (int i = 1; i < p.Count;i++)
            {
                double x = p[i].X;
                double y = p[i].Y;

                if (x < minX)
                    minX = x;
                if (y < minY)
                    minY = y;

                if (x > maxX)
                    maxX = x;
                if (y > maxY)
                    maxY = y;
            }

            test_pen = new Pen(c);
            m_graphics.DrawLine(test_pen, new Point((int)minX, (int)minY),new Point((int)minX, (int)maxY));
            m_graphics.DrawLine(test_pen, new Point((int)minX, (int)maxY), new Point((int)maxX, (int)maxY));
            m_graphics.DrawLine(test_pen, new Point((int)maxX, (int)maxY), new Point((int)maxX, (int)minY));
            m_graphics.DrawLine(test_pen, new Point((int)maxX, (int)minY), new Point((int)minX, (int)minY));
        }

        private void TemplateToggle_Click(object sender, EventArgs e)
        {
            showTemplates();
        }

        private void showTemplates()
        {
            Shape shape = Shapes.getShape(currentShape);
            for (int j = 0; j < shape.numTemplates; j++)
            {
                List<DrawPoint> template = shape.getTemplate(j);

                Pen template_pen = new Pen(Color.Red);

                m_graphics.DrawRectangle(template_pen, (int)template[0].X, (int)template[0].Y, 1, 1);

                for (int i = 1; i < template.Count; i++)
                {
                    if(template[i-1].stroke == template[i].stroke)
                        m_graphics.DrawLine(template_pen, template[i - 1].ToPoint(), template[i].ToPoint());
                }
            }
        }

        private void WipeBoxNoLog()
        {
            TR.ResetPointsNoLog();
            ClearDisplay();
            strokeNum = 0;
        }

        private void WipeBoxWithLog()
        {
            TR.ResetPoints(Shapes.getShape(currentShape), false, currentSet, currentSeq, seqIndex);
            ClearDisplay();
            strokeNum = 0;
        }

        //Add a template to the list
        private void button2_Click(object sender, EventArgs e)
        {
            Shapes.getShape(currentShape).addTemplate(TR.getPoints());
            WipeBoxNoLog();
        }

        private void ClearTemplates_Click(object sender, EventArgs e)
        {
            Shapes.getShape(currentShape).clearTemplates();
            WipeBoxNoLog();
        }

        public bool haveTemplates()
        {
            if (Shapes.getShape(currentShape).numTemplates > 0)
                return true;
            return false;
        }

        private void CompareShapes(Object source, EventArgs e)
        {
            compareTimer.Stop();
            if (haveTemplates() && autoRecognize)
            {
                Console.WriteLine("Attempting recognition.\n");
                if(AttemptRecognition())
                {
                    TR.ResetPoints(Shapes.getShape(currentShape), true, currentSet, currentSeq, seqIndex);
                    scribblePanel.Invalidate();
                    strokeNum = 0;
                    GetNextShape();
                }
            }
        }

        private void ChangeShape_Click(object sender, EventArgs e)
        {
            debugNextShape();
        }

        private void debugNextShape()
        {
            seqIndex++;
            if (seqIndex >= sets[currentSet][currentSeq].Count)
            {
                //sequence completed
                seqIndex = 0;
                currentSeq = (currentSeq + 1) % sets[currentSet].Count;
                if(currentSeq == 0)
                {
                    currentSet = (currentSet + 1) % sets.Count;
                }
            }

            currentShape = sets[currentSet][currentSeq][seqIndex];

            WipeBoxNoLog();
            UpdateCurrentShape();
        }

        private void GetNextShape()
        {
            //currentShape = (currentShape + 1) % Shapes.NUM_SHAPES;

            seqIndex++;
            if(seqIndex >= sets[currentSet][currentSeq].Count)
            {
                //sequence completed
                SequenceCompleted();
            }


            WipeBoxNoLog();
            UpdateCurrentShape();
        }

        private void TemplateDump_Click(object sender, EventArgs e)
        {
            Shape dumper = Shapes.getShape(currentShape);
            dumper.appendTemplatesToFile();
            WipeBoxNoLog();
        }

        private void ToggleRecognize_Click(object sender, EventArgs e)
        {
            autoRecognize = !autoRecognize;
            if(autoRecognize)
            {
                Console.WriteLine("Now autorecognizing.");
            }
            else
            {
                Console.WriteLine("Not autorecognizing.");
            }
        }

        private void EndDrawingPhase()
        {
            compareTimer.Stop();
            timeoutTimer.Stop();
            screenEnabled = false;
            reenableTimer.Start();
        }

        private void EndTest()
        {
            TimeUpLabel.Visible = false;
            SetCompleteLabel.Visible = false;
            SequenceCompleteLabel.Visible = false;
            compareTimer.Stop();
            timeoutTimer.Stop();
            screenEnabled = false;
            TestingCompleteLabel.Visible = true;
        }

        private void TimeTick(object sender, EventArgs e)
        {
            timeLeft -= timerInterval;
            TimeLeftLabel.Text = String.Format("Time left: {0:F1} s",timeLeft);
            if(timeLeft < .1)
            {
                EndDrawingPhase();
                WipeBoxWithLog();
                TimeLeftLabel.Text = "Time left: 0 s";
                currentSeq = 0;
                seqIndex = 0;
                TimeUpLabel.Visible = true;
                currentSet = (currentSet + 1) % sets.Count;
                if (currentSet == 0)
                    EndTest();
            }
        }

        private void ReenableScreen(object sender, EventArgs e)
        {
            reenableTimer.Stop();
            TimeUpLabel.Visible = false;
            SequenceCompleteLabel.Visible = false;
            SetCompleteLabel.Visible = false;
            screenEnabled = true;
            timeLeft = maxTime;
            TimeLeftLabel.Text = String.Format("Time left: {0:F1} s", timeLeft);
            UpdateCurrentShape();
        }

        private void SequenceCompleted()
        {
            currentSeq = (currentSeq + 1) % sets[currentSet].Count;
            seqIndex = 0;
            WipeBoxNoLog();

            if(currentSeq == 0)
            {
                //set finished
                currentSet = (currentSet + 1) % sets.Count;
                if(currentSet == 0)
                {
                    EndTest();
                }
                else
                {
                    EndDrawingPhase();
                    SetCompleteLabel.Visible = true;
                }
            }
            else
            {
                //sequence finished
                EndDrawingPhase();
                SequenceCompleteLabel.Visible = true;
            }
        }

        private void StartStopTimerButton_Click(object sender, EventArgs e)
        {
            timeoutEnabled = !timeoutEnabled;
            if (timeoutTimer.Enabled)
            {
                timeoutTimer.Stop();
            }
            else
            {
                if(timeLeft != maxTime)
                    timeoutTimer.Start();
            }
        }

        private void ClearButton2_Click(object sender, EventArgs e)
        {
            ClearDisplay();
            TR.ResetPoints(Shapes.getShape(currentShape), false, currentSet, currentSeq, seqIndex);
            strokeNum = 0;
        }

        private void UpdateCurrentShape()
        {
            currentShape = sets[currentSet][currentSeq][seqIndex];
            CurrentTemplateLabel.Text = String.Format("{3} - {4} left", currentSet, currentSeq, seqIndex, Shapes.getShape(currentShape).name, sets[currentSet][currentSeq].Count - seqIndex);
        }

        void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'q')
            {
                debugButtonsOn = !debugButtonsOn;
                if(debugButtonsOn)
                {
                    AddTemplate.Enabled = true;
                    AddTemplate.Visible = true;
                    TemplateDump.Enabled = true;
                    TemplateDump.Visible = true;
                    ChangeShape.Enabled = true;
                    ChangeShape.Visible = true;
                }
                else
                {
                    AddTemplate.Enabled = false;
                    AddTemplate.Visible = false;
                    TemplateDump.Enabled = false;
                    TemplateDump.Visible = false;
                    ChangeShape.Enabled = false;
                    ChangeShape.Visible = false;
                }
            }
            if(e.KeyChar == 't')
            {
                showCurrentShape = !showCurrentShape;
                if(showCurrentShape)
                {
                    CurrentTemplateLabel.Visible = true;
                    CurrentTemplateLabel.Enabled = true;
                    UpdateCurrentShape();
                }
                else
                {
                    CurrentTemplateLabel.Visible = false;
                    CurrentTemplateLabel.Enabled = false;
                }
            }
        }
    }
}
