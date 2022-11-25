using InputSimulator.Model;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static InputSimulator.Helpers.Constants;

namespace InputSimulator
{
    public class DataCollector : IDisposable
    {
        int totalEventsSaved = 0;
        public DataCollector(string dbPath, int _batchSize)
        {
            batchSize = _batchSize;
            Task.Run(async () => { db_async = await InitDatabase(dbPath); }).Wait();
        }

        #region Public Methods
        public void CollectMouseEvent(MouseEvent me)
        {
            if (MouseEventBatchSaved())
            {
                if (mouseEventMidSaveBatch.Count > 0)
                {
                    mouseEventSaveBatch = new List<MouseEvent>(mouseEventMidSaveBatch);
                    mouseEventMidSaveBatch.Clear();
                }

                mouseEventSaveBatch.Add(me);
                if (mouseEventSaveBatch.Count >= batchSize)
                {
                    mouseEventSaveBatchTask = SaveMouseEventBatch();
                }
            }
            else
            {
                mouseEventMidSaveBatch.Add(me);
            }
        }

        public void CollectKeyboardEvent(KeyboardEvent ke)
        {
            if (KeyboardEventBatchSaved())
            {
                if (keyboardEventMidSaveBatch.Count > 0)
                {
                    keyboardEventSaveBatch = new List<KeyboardEvent>(keyboardEventMidSaveBatch);
                    keyboardEventMidSaveBatch.Clear();
                }

                keyboardEventSaveBatch.Add(ke);
                if (keyboardEventSaveBatch.Count >= batchSize)
                {
                    keyboardEventSaveBatchTask = SaveKeyboardEventBatch();
                }
            }
            else
            {
                keyboardEventMidSaveBatch.Add(ke);
            }
        }
        #endregion

        #region Private Methods
        private async Task<SQLiteAsyncConnection> InitDatabase(string dbPath)
        {
            SQLiteAsyncConnection con = new SQLiteAsyncConnection(dbPath);

            await con.CreateTableAsync<MouseEvent>();
            await con.CreateTableAsync<KeyboardEvent>();
            return con;
        }

        private async Task SaveMouseEventBatch()
        {
            await db_async.InsertAllAsync(mouseEventSaveBatch);
            totalEventsSaved += mouseEventSaveBatch.Count;
            mouseEventSaveBatch.Clear();
            return;
        }

        private async Task SaveKeyboardEventBatch()
        {
            await db_async.InsertAllAsync(keyboardEventSaveBatch);
            totalEventsSaved += keyboardEventSaveBatch.Count;
            keyboardEventSaveBatch.Clear();
            return;
        }

        private bool MouseEventBatchSaved()
        {
            if (mouseEventSaveBatchTask != null)
            {
                return mouseEventSaveBatchTask.IsCompleted;
            }
            return true;
        }

        private bool KeyboardEventBatchSaved()
        {
            if (keyboardEventSaveBatchTask != null)
            {
                return keyboardEventSaveBatchTask.IsCompleted;
            }
            return true;
        }

        public void Dispose()
        {
            mouseEventSaveBatch.AddRange(mouseEventMidSaveBatch);
            keyboardEventSaveBatch.AddRange(keyboardEventMidSaveBatch);
            SaveMouseEventBatch().Wait();
            SaveKeyboardEventBatch().Wait();
        }
        #endregion

        #region Private Fields
        private SQLiteAsyncConnection db_async;
        private List<MouseEvent> mouseEventSaveBatch = new List<MouseEvent>();
        private List<MouseEvent> mouseEventMidSaveBatch = new List<MouseEvent>();
        private Task mouseEventSaveBatchTask = null;

        private List<KeyboardEvent> keyboardEventSaveBatch = new List<KeyboardEvent>();
        private List<KeyboardEvent> keyboardEventMidSaveBatch = new List<KeyboardEvent>();
        private Task keyboardEventSaveBatchTask = null;

        private int batchSize = 100;
        #endregion
    }
}
