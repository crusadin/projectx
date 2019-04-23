using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.Threading;
using Emgu.Util;
using Patagames.Ocr;
namespace ProjectX
{
    internal struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    internal struct Input
    {
        public int Type;
        public MouseInput MouseInput;
    }

    public class InventoryItem
    {
        public Bitmap inventoryItemImage { get; set; }
        public Point inventoryItemLocation { get; set; }
        public long inventoryItemId { get; set; }
    }


    class BlackDesertScreenCapture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public Bitmap CaptureScreen()
        {
            Rectangle bounds;
            var rect = new Rect();

            Process bdoProcess = BlackDesertProcess.GetBlackDesertProcess();

            IntPtr bdoProcessMainHandle = bdoProcess.MainWindowHandle;

            GetWindowRect(bdoProcessMainHandle, ref rect);
            bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

            var captureResult = new Bitmap(bounds.Width, bounds.Height);

            using (var g = Graphics.FromImage(captureResult))
            {
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return captureResult;

        }

    }

    class BlackDesertProcess
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static Process GetBlackDesertProcess()
        {
            Process[] processes = Process.GetProcessesByName("BlackDesert64");
            Process bdoProcess = processes.First();

            return bdoProcess;
        }

        public static void SetForegroundBlackDesert(Process bdoProcess)
        {
            SetForegroundWindow(bdoProcess.MainWindowHandle);
        }
    }

    class BlackDesertCooking
    {
        public static Bitmap GetCookingStatusImage(Bitmap bdoScreenCapture)
        {

            int x = 925;
            int y = 830;
            int width = 70;
            int height = 20;

            Rectangle crop = new Rectangle(x, y, width, height);

            var bmp = new Bitmap(crop.Width, crop.Height);

            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(bdoScreenCapture, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }
            return bmp;

        }

        public static bool GetCookingStatus(Bitmap cookingStatusImage)
        {

            return false;
        }
    }

    class BlackDesertTrading
    {
        public static void InitializeCrateStacking()
        {
            BlackDesertScreenCapture bdoSreenCapture = new BlackDesertScreenCapture();
            Bitmap capture = bdoSreenCapture.CaptureScreen();

            Point inventoryPoint = BlackDesertInventory.LocateInventoryPositionPoint(capture);
            Point mountInventoryPoint = BlackDesertInventory.LocateMountInventoryPositionPoint(capture);
            Point storageInventoryPoint = BlackDesertInventory.LocateStorageInventoryPositionPoint(capture);

            BlackDesertMouse.DragMouse(inventoryPoint.X, inventoryPoint.Y, 1659, 271, 1920, 1080);
            Thread.Sleep(1500);
            BlackDesertMouse.DragMouse(mountInventoryPoint.X, mountInventoryPoint.Y, 1639, 89, 1920, 1080);
            Thread.Sleep(1500);
            BlackDesertMouse.DragMouse(storageInventoryPoint.X, storageInventoryPoint.Y, 1236, 83, 1920, 1080);
            Thread.Sleep(1500);
        }

        public static InventoryItem MountHasCrates()
        {
            bool hasCrates = false;
            BlackDesertScreenCapture bdoSreenCapture = new BlackDesertScreenCapture();
            Bitmap capture = bdoSreenCapture.CaptureScreen();
            Point mountInventoryPoint = BlackDesertInventory.LocateMountInventoryPositionPoint(capture);

            int mountInventorySlotWidth = 40;
            int mountInventorySlotHeight = 40;
            int mountInventorySlotOffsetX = -131;
            int mountInventorySlotOffsetY = 85;

            mountInventoryPoint.Y = mountInventoryPoint.Y + mountInventorySlotOffsetY;
            mountInventoryPoint.X = mountInventoryPoint.X + mountInventorySlotOffsetX;

            Rectangle inventoryCrop = new Rectangle(mountInventoryPoint.X, mountInventoryPoint.Y, mountInventorySlotWidth, mountInventorySlotHeight);

            Bitmap inventoryImage = new Bitmap(inventoryCrop.Width, inventoryCrop.Height);

            using (var graphics = Graphics.FromImage(inventoryImage))
            {
                graphics.DrawImage(capture, new Rectangle(0, 0, inventoryImage.Width, inventoryImage.Height), inventoryCrop, GraphicsUnit.Pixel);
            }

            InventoryItem inventoryItem = new InventoryItem
            {
                inventoryItemId = 1,
                inventoryItemImage = inventoryImage,
                inventoryItemLocation = mountInventoryPoint
            };



            
            return inventoryItem;
        }
    }

    class BlackDesertInventory
    {
        public static Rectangle LocateInventoryPositionRect(Bitmap bdoScreenCapture)
        {
            Image<Bgr, byte> source = new Image<Bgr, byte>(bdoScreenCapture);
            Image<Bgr, byte> template = new Image<Bgr, byte>(Application.StartupPath + "\\CoreImageFiles\\inventory.jpg"); // Image A
            Image<Bgr, byte> imageToShow = source.Copy();

            Rectangle match = new Rectangle();

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > 0.9)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    match = new Rectangle(maxLocations[0], template.Size);

                }
            }
            return match;
        }

        public static Point LocateInventoryPositionPoint(Bitmap bdoScreenCapture)
        {
            Image<Bgr, byte> source = new Image<Bgr, byte>(bdoScreenCapture);
            Image<Bgr, byte> template = new Image<Bgr, byte>(Application.StartupPath + "\\CoreImageFiles\\inventory.jpg"); // Image A
            Image<Bgr, byte> imageToShow = source.Copy();

            Rectangle match = new Rectangle();
            Point[] minLocations, maxLocations;

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > 0.9)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    match = new Rectangle(maxLocations[0], template.Size);

                }
            }
            return maxLocations[0];
        }

        public static Point LocateMountInventoryPositionPoint(Bitmap bdoScreenCapture)
        {
            Image<Bgr, byte> source = new Image<Bgr, byte>(bdoScreenCapture);
            Image<Bgr, byte> template = new Image<Bgr, byte>(Application.StartupPath + "\\CoreImageFiles\\mount_inven.jpg"); // Image A
            Image<Bgr, byte> imageToShow = source.Copy();

            Rectangle match = new Rectangle();
            Point[] minLocations, maxLocations;

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > 0.9)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    match = new Rectangle(maxLocations[0], template.Size);

                }
            }
            return maxLocations[0];
        }

        public static Point LocateStorageInventoryPositionPoint(Bitmap bdoScreenCapture)
        {
            Image<Bgr, byte> source = new Image<Bgr, byte>(bdoScreenCapture);
            Image<Bgr, byte> template = new Image<Bgr, byte>(Application.StartupPath + "\\CoreImageFiles\\calpheon_city.jpg"); // Image A
            Image<Bgr, byte> imageToShow = source.Copy();

            Rectangle match = new Rectangle();
            Point[] minLocations, maxLocations;

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > 0.9)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    match = new Rectangle(maxLocations[0], template.Size);

                }
            }
            return maxLocations[0];
        }

        public static Bitmap GetInventoryPositionImage(Bitmap bdoScreenCapture, Rectangle position)
        {
            int width = 75;
            int height = 25;

            Rectangle crop = new Rectangle(position.X, position.Y, width, height);

            var inventoryBmp = new Bitmap(crop.Width, crop.Height);

            using (var gr = Graphics.FromImage(inventoryBmp))
            {
                gr.DrawImage(bdoScreenCapture, new Rectangle(0, 0, inventoryBmp.Width, inventoryBmp.Height), crop, GraphicsUnit.Pixel);
            }
            return inventoryBmp;
        }

        public static List<InventoryItem> GetInventoryPageImages(Bitmap bdoScreenCapture, Rectangle position)
        {
            int inventorySlotWidth = 40;
            int inventorySlotHeight = 40;
            int inventorySlotOffsetX = -156;
            int inventorySlotOffsetY = 78;
            int initialX = position.X;
            int inventoryIdCounter = 0;

            List<InventoryItem> inventoryItems = new List<InventoryItem>();

            for (int rowCount = 1; rowCount <= 8; rowCount++)
            {
                if (rowCount.Equals(1))
                {
                    position.Y = position.Y + inventorySlotOffsetY;
                }
                else
                {
                    position.Y = position.Y + 47;
                }

                for (int columnCount = 1; columnCount <= 8; columnCount++)
                {
                    if (columnCount.Equals(1))
                    {
                        position.X = position.X + inventorySlotOffsetX;
                    }
                    else
                    {
                        position.X = position.X + 48;

                    }


                    Rectangle inventoryCrop = new Rectangle(position.X, position.Y, inventorySlotWidth, inventorySlotHeight);

                    Bitmap inventoryImage = new Bitmap(inventoryCrop.Width, inventoryCrop.Height);

                    using (var graphics = Graphics.FromImage(inventoryImage))
                    {
                        graphics.DrawImage(bdoScreenCapture, new Rectangle(0, 0, inventoryImage.Width, inventoryImage.Height), inventoryCrop, GraphicsUnit.Pixel);
                    }
                    inventoryIdCounter++;
                    InventoryItem inventoryItem = new InventoryItem
                    {
                        inventoryItemId = inventoryIdCounter,
                        inventoryItemLocation = new Point(inventorySlotOffsetX, inventorySlotOffsetY),
                        inventoryItemImage = inventoryImage
                    };

                    inventoryItems.Add(inventoryItem);

                    if (columnCount.Equals(8))
                    {
                        position.X = initialX;
                    }
                }
            }

            return inventoryItems;
        }

        public static List<InventoryItem> GetInventoryImages()
        {
            BlackDesertScreenCapture bdoSreenCapture = new BlackDesertScreenCapture();
            List<InventoryItem> inventoryItems = new List<InventoryItem>();
            Bitmap capture = bdoSreenCapture.CaptureScreen();
            Rectangle inventoryRect = BlackDesertInventory.LocateInventoryPositionRect(capture);
            Point point = BlackDesertInventory.LocateInventoryPositionPoint(capture);
            int inventoryCounter = 1;
            for (int page = 1; page <= 3; page++)
            {
                if (page == 1)
                {
                    BlackDesertMouse.DragMouse(point.X, point.Y, 1659, 271, 1920, 1080);
                    capture = bdoSreenCapture.CaptureScreen();
                    inventoryRect = BlackDesertInventory.LocateInventoryPositionRect(capture);
                    point = BlackDesertInventory.LocateInventoryPositionPoint(capture);
                }
                else if (page == 2)
                {
                    BlackDesertMouse.DragMouse(1890, 527, 1890, 527, 1920, 1080);
                    capture = bdoSreenCapture.CaptureScreen();
                    inventoryRect = BlackDesertInventory.LocateInventoryPositionRect(capture);
                    point = BlackDesertInventory.LocateInventoryPositionPoint(capture);
                }
                else if (page == 3) {
                    BlackDesertMouse.DragMouse(1890, 527, 1890, 690, 1920, 1080);
                    capture = bdoSreenCapture.CaptureScreen();
                    inventoryRect = BlackDesertInventory.LocateInventoryPositionRect(capture);
                    point = BlackDesertInventory.LocateInventoryPositionPoint(capture);
                }
                var inventoryPageImages = BlackDesertInventory.GetInventoryPageImages(capture, inventoryRect);
                inventoryPageImages.ForEach(inventoryPageImage => {
                    inventoryPageImage.inventoryItemId = inventoryCounter;
                    inventoryItems.Add(inventoryPageImage);
                    inventoryCounter++;
                    });
            }

            return inventoryItems;
        }

        

    }

    class BlackDesertMouse
    {
        public const int InputMouse = 0;

        public const int MouseEventMove = 0x01;
        public const int MouseEventLeftDown = 0x02;
        public const int MouseEventLeftUp = 0x04;
        public const int MouseEventRightDown = 0x08;
        public const int MouseEventRightUp = 0x10;
        public const int MouseEventAbsolute = 0x8000;
        public const int MouseEventWheel = 0x800;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numInputs, Input[] inputs, int size);

        public static void MoveMouse(int positionX, int positionY, int maxX, int maxY)
        {
            Input[] input = new Input[1];
            input[0].Type = InputMouse;
            input[0].MouseInput.X = (positionX * 65535) / maxX;
            input[0].MouseInput.Y = (positionY * 65535) / maxY;
            input[0].MouseInput.Flags = MouseEventAbsolute | MouseEventMove | MouseEventLeftDown;
            SendInput(1, input, Marshal.SizeOf(input[0]));
        }

        public static void LeftDownMouse()
        {
            Input[] input = new Input[1];
            input[0].Type = InputMouse;
            input[0].MouseInput.Flags = MouseEventMove | MouseEventLeftDown;
            SendInput(1, input, Marshal.SizeOf(input[0]));
        }

        public static void LeftUpMouse()
        {
            Input[] input = new Input[1];
            input[0].Type = InputMouse;
            input[0].MouseInput.Flags = MouseEventMove | MouseEventLeftUp;
            SendInput(1, input, Marshal.SizeOf(input[0]));
        }

        public static void WheelDown(int ticks)
        {
            Input[] input = new Input[1];
            input[0].Type = InputMouse;
            input[0].MouseInput.Flags = MouseEventMove | MouseEventWheel;
            input[0].MouseInput.MouseData = 120;
            SendInput(1, input, Marshal.SizeOf(input[0]));
        }

        public static void DragMouse(int startX, int startY, int endX, int endY, int maxX, int maxY)
        {
            Process bdoProcess = BlackDesertProcess.GetBlackDesertProcess();
            BlackDesertProcess.SetForegroundBlackDesert(bdoProcess);
            MoveMouse(startX, startY, maxX, maxY);
            Thread.Sleep(100);
            LeftDownMouse();
            Thread.Sleep(100);
            LeftUpMouse();
            Thread.Sleep(100);
            LeftDownMouse();
            Thread.Sleep(100);
            MoveMouse(endX, endY, maxX, maxY);
            Thread.Sleep(100);
            LeftUpMouse();
        }

        public static void MoveMouseWheelDown(int positionX, int positionY, int maxX, int maxY, int ticks)
        {
            Process bdoProcess = BlackDesertProcess.GetBlackDesertProcess();
            BlackDesertProcess.SetForegroundBlackDesert(bdoProcess);
            MoveMouse(positionX, positionY, maxX, maxY);
            Thread.Sleep(1000);
            WheelDown(ticks);
        }
    }
}


