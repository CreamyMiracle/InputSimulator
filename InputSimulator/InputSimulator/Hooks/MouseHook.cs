using System;
using System.Drawing;
using System.Runtime.InteropServices;
using InputSimulator.Flags;
using InputSimulator.Model;
using static InputSimulator.Helpers.Constants;

namespace InputSimulator.Hooks
{
    public class MouseHook
    {
        private int hHook;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        private const int WM_MBUTTONROLL = 0x20A;
        public const int WH_MOUSE_LL = 14;
        public Win32API.MouseHookProc hProc;
        public int SetHook()
        {
            hProc = new Win32API.MouseHookProc(MouseHookProc);
            hHook = Win32API.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
            return hHook;
        }
        public void UnHook()
        {
            Win32API.UnhookWindowsHookEx(hHook);
        }
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Win32API.MouseHookStruct MyMouseHookStruct = (Win32API.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32API.MouseHookStruct));
            if (nCode < 0)
            {
                return Win32API.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                if (MouseEvent != null)
                {
                    int x = MyMouseHookStruct.pt.x;
                    int y = MyMouseHookStruct.pt.y;
                    MouseEvent e = null;
                    switch ((Int32)wParam)
                    {
                        case WM_MOUSEMOVE:
                            e = new MouseEvent(MouseEventFlag.MoveNoCoalesce, x, y, 0);
                            break;
                        case WM_LBUTTONDOWN:
                            e = new MouseEvent(MouseEventFlag.LeftDown, x, y, 0);
                            break;
                        case WM_RBUTTONDOWN:
                            e = new MouseEvent(MouseEventFlag.RightDown, x, y, 0);
                            break;
                        case WM_MBUTTONDOWN:
                            e = new MouseEvent(MouseEventFlag.MiddleDown, x, y, 0);
                            break;
                        case WM_LBUTTONUP:
                            e = new MouseEvent(MouseEventFlag.LeftUp, x, y, 0);
                            break;
                        case WM_RBUTTONUP:
                            e = new MouseEvent(MouseEventFlag.RightUp, x, y, 0);
                            break;
                        case WM_MBUTTONUP:
                            e = new MouseEvent(MouseEventFlag.MiddleUp, x, y, 0);
                            break;
                        case WM_MBUTTONROLL:
                            e = new MouseEvent(MouseEventFlag.Wheel, x, y, 120);
                            break;
                        default:
                            Console.WriteLine("Unknown mouse event");
                            break;
                    }
                    MouseEvent(this, e);
                }
                return Win32API.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }

        public delegate void MouseClickHandler(object sender, MouseEvent e);
        public event MouseClickHandler MouseEvent;
    }
}
