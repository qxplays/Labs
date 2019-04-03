using System;


namespace Domain.PublicDataContracts.ForChat {
    public enum MessageType {
        Data=0x00,
        Text=0x01,
        Info=0x02,
        KeepAlive=0xFF
    }


    [Flags]
    public enum DownloadType {
        User = 0x00,
        Server = 0x01,
        Upload = 0x02,
        Download = 0x04
    }
}