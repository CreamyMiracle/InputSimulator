using InputSimulator.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InputSimulator
{
    public class ScreenCam
    {
        public ScreenCam(string workDir, string dbPath)
        {
            _workDir = workDir;
            db = InitDatabase(dbPath);
        }

        public Bitmap CaptureScreen()
        {
            Rectangle captureRectangle = Screen.PrimaryScreen.Bounds;
            Bitmap captureBitmap = new Bitmap(captureRectangle.Width, captureRectangle.Height, PixelFormat.Format32bppArgb);
            Graphics captureGraphics = Graphics.FromImage(captureBitmap);
            captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);

            return captureBitmap;
        }

        public string SaveImageToDisk(Bitmap bmp)
        {
            string path = Path.Combine(_workDir, Guid.NewGuid().ToString() + ".bmp");
            bmp.Save(path, ImageFormat.Bmp);
            return path;
        }

        public void SaveImageToDB(string path, int testCase)
        {
            db.Insert(new Screenshot(path, testCase));
        }


        public Bitmap ReadFromDisk(int testCase)
        {
            Screenshot sc = db.Get<Screenshot>(testCase);
            return new Bitmap(sc.Path);
        }

        #region Private Methods
        private SQLiteConnection InitDatabase(string dbPath)
        {
            SQLiteConnection con = new SQLiteConnection(dbPath);

            con.CreateTable<Screenshot>();
            return con;
        }
        #endregion

        #region Private Fields
        private SQLiteConnection db;
        private string _workDir;
        #endregion
    }
}
