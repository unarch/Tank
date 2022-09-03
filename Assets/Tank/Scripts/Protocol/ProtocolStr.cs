using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// 字符串协议模型
// 形式 名称, 参数1, 参数2, 参数3
public class ProtocolStr : ProtocolBase
{
    // 传输的字符串
    public string? str;

    // 解码器
    public override ProtocolBase Decode(byte[] readBuff, int start, int length)
    {
        ProtocolStr protocol = new ProtocolStr();
        protocol.str = System.Text.Encoding.UTF8.GetString(readBuff, start, length);
        return (ProtocolBase)protocol;
    }

    // 编码器
    public override byte[] Encode()
    {
        if (str == null) return new byte[]{};
        byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
        return b;
    }

    // 协议名称
    public override string GetName()
    {
        if (str == null) return "";
        if (str.Length == 0) return "";
        return str.Split(',')[0];
    }
    
    // 协议描述
    public override string GetDesc()
    {
        return str!;
    }


    
}
