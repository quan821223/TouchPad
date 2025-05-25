using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaTouchPad.Model.CommunicationM
{
    public abstract class CommunicationToolHandle
    {
        public abstract void OpenConnection(CommunicationToolHandle parm);
        public abstract void CloseConnection();

        public abstract Task SendAsync(byte[] data);
        public abstract Task<byte[]> ReceiveAsync();

        public abstract Task<byte[]> ReceiveAsync_SC_EndoflineTimeOut();

        public abstract Task<CommunicationContain> SC_SendRecData(byte[] data);
        public abstract Task<byte[]> SendRecData(byte[] data);

        public abstract bool IsConnected();
        public abstract int BytesToRead();
        public abstract int ReadByte();
    }
    public enum CommunicationType
    {
        Com,
        TcpClient,
        // 其他通訊類型
    }
    public class CommunicationContain
    {
        public string from { get; set; }
        public string to { get; set; }
        public byte[] Senddata { get; set; }
        public byte[] Recdata { get; set; }
        public string StrSenddata { get; set; }
        public string StrRecdata { get; set; }
        public Errortype errortype { get; set; }
    }
    public enum Errortype
    {
        Null,
        DataLoss,
        NoResponse,
        IncompleteData,
        InvalidData,
        OutofSpec,
        InvalidRef,
        DeviceErr,
        Unknow
    }
}
