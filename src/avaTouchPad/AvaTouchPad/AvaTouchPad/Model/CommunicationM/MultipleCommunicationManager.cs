using AvaTouchPad.Model.CommunicationM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AvaTouchPad.Model.CommunicationM
{
    public class MultipleCommunicationManager
    {
        private readonly Dictionary<int, CommunicationToolHandle> _communicationHandlers;
        /// <summary>
        /// 事件通知機制（Event Pattern）
        /// 目的:
        /// <para>* 實現loose coupling : 訂閱者不需要直接依賴於 'MultipleCommunicationManager' </para>
        /// <para>* 支援多個訂閱者:使多個viewmodel 或其他組件可以同時監聽這個事件                </para>
        /// <para>* 非同步通知:事件可以在不阻塞主要操作的情況下通知所有訂閱者                     </para>
        /// </summary>
        public event EventHandler<CommunicationHandlerEventArgs> HandlerChanged;
        public string DeviceName { get; set; }
        public MultipleCommunicationManager()
        {
            _communicationHandlers = new Dictionary<int, CommunicationToolHandle>();

        }
        /// <summary>
        /// 自定義事件參數類別 - 封裝事件相關的數據
        /// <para>* 類型安全：提供強類型的事件參數</para>
        /// <para>* 資料封裝：將相關的數據（Number 和 Handler）組織在一起  </para>
        /// <para>* 不可變性：使用只讀屬性確保數據在傳遞過程中不被修改 </para>
        /// </summary>
        public class CommunicationHandlerEventArgs : EventArgs
        {
            public int Number { get; }
            public CommunicationToolHandle Handler { get; }

            public CommunicationHandlerEventArgs(int number, CommunicationToolHandle handler)
            {
                Number = number;
                Handler = handler;

            }
        }
        /// <summary>
        /// 事件觸發方法 - 提供一個統一的方法來觸發事件
        /// <para> 封裝事件觸發邏輯                 </para>
        /// <para> 使用 ?. 運算符避免空引用異常     </para>
        /// <para> virtual 允許子類覆寫事件觸發行為 </para>
        /// <para> 統一的事件觸發點，便於維護和調試  </para>   
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandlerChanged(CommunicationHandlerEventArgs e)
        {
            HandlerChanged?.Invoke(this, e);
        }
        /// <summary>
        /// 線程安全的處理器獲取方法
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>

        public CommunicationToolHandle GetHandler(int number)
        {
            lock (_communicationHandlers)
            {
                return _communicationHandlers.TryGetValue(number, out var handler) ? handler : null;
            }
        }
        public bool IsConnected(int number)
        {

            if (_communicationHandlers.TryGetValue(number, out var handler))
            {
                try
                {
                    bool Devicestatus = false;
                    Devicestatus = handler.IsConnected();
                    OnHandlerChanged(new CommunicationHandlerEventArgs(number, handler));
                    return Devicestatus;
                }
                catch (Exception ex)
                {
                    // 記錄錯誤並拋出異常
                    throw new InvalidOperationException($"Failed to connect device {number}", ex);

                }
            }
            return false;
        }

        // 註冊 No. 與通訊 Handler
        public void RegisterCommunicationHandler(int number, CommunicationToolHandle handler)
        {
            //if (!_communicationHandlers.ContainsKey(number))
            //{
            //    _communicationHandlers[number] = handler;
            //    OnHandlerChanged(new CommunicationHandlerEventArgs(number, handler));
            //}

            lock (_communicationHandlers)
            {
                _communicationHandlers[number] = handler; // 直接替換或添加新的處理器
                OnHandlerChanged(new CommunicationHandlerEventArgs(number, handler));
            }
        }

        public void ConnectDevice(int number, CommunicationToolHandle parm)
        {
            //if (_communicationHandlers.ContainsKey(number))
            //{
            //    var handler = _communicationHandlers[number];
            //    handler.OpenConnection(parm);                       
            //}
            if (_communicationHandlers.TryGetValue(number, out var handler))
            {
                try
                {
                    handler.OpenConnection(parm);
                    OnHandlerChanged(new CommunicationHandlerEventArgs(number, handler));
                }
                catch (Exception ex)
                {
                    // 記錄錯誤並拋出異常
                    throw new InvalidOperationException($"Failed to connect device {number}", ex);
                }
            }
        }
        public void DisconnectDevice(int number)
        {
            //if (_communicationHandlers.ContainsKey(number))
            //{
            //    var handler = _communicationHandlers[number];
            //    handler.CloseConnection();
            //}

            if (_communicationHandlers.TryGetValue(number, out var handler))
            {
                try
                {
                    handler.CloseConnection();
                    OnHandlerChanged(new CommunicationHandlerEventArgs(number, handler));
                }
                catch (Exception ex)
                {
                    // 記錄錯誤並拋出異常
                    throw new InvalidOperationException($"Failed to disconnect device {number}", ex);
                }
            }
        }

        // 發送資料
        public async Task SendDataAsync(int number, byte[] data)
        {

            if (_communicationHandlers.ContainsKey(number))
            {
                var handler = _communicationHandlers[number];
                //wait handler.ConnectAsync();
                await handler.SendAsync(data);
                //await handler.DisconnectAsync();
            }
            else
            {
                throw new ArgumentException($"No communication handler found for number: {number}");
            }
        }

        // 接收資料
        public async Task<byte[]> ReceiveDataAsync(int number)
        {
            if (_communicationHandlers.ContainsKey(number))
            {
                var handler = _communicationHandlers[number];
                //await handler.ConnectAsync();
                var data = await handler.ReceiveAsync();
                //await handler.DisconnectAsync();
                return data;
            }
            else
            {
                throw new ArgumentException($"No communication handler found for number: {number}");
            }
        }

        public async Task<byte[]> SendRdDataAsync(int number, byte[] data)
        {

            byte[] buffer = new byte[4096];

            if (_communicationHandlers.ContainsKey(number))
            {
                var handler = _communicationHandlers[number];
                //wait handler.ConnectAsync();

                buffer = await handler.SendRecData(data);
                return buffer;
            }
            else
            {
                return buffer;
                throw new ArgumentException($"No communication handler found for number: {number}");
            }
        }
        public int BytesToRead(int number)
        {
            if (_communicationHandlers.ContainsKey(number))
            {
                var handler = _communicationHandlers[number];
                int ReturnNum = handler.BytesToRead();

                return ReturnNum;
            }
            else
            {
                return 0;
                throw new ArgumentException($"No communication handler found for number: {number}");
            }
        }

        public int ReadByte(int number)
        {
            if (_communicationHandlers.ContainsKey(number))
            {
                var handler = _communicationHandlers[number];
                int ReturnNum = handler.ReadByte();

                return ReturnNum;
            }
            else
            {
                return 0;
                throw new ArgumentException($"No communication handler found for number: {number}");
            }
        }


    }
}
