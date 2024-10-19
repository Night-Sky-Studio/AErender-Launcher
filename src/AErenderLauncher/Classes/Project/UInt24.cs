using System;

namespace AErenderLauncher.Classes.Project;

public struct UInt24 {
    public readonly byte _b0, _b1, _b2;
    public UInt24(UInt32 val) {
        _b0 = (byte)((val >> 16) & 0xFF);
        _b1 = (byte)((val >> 8) & 0xFF);
        _b2 = (byte)(val & 0xFF);
    }

    public UInt24(byte[] val) {
        _b0 = val[0];
        _b1 = val[1];
        _b2 = val[2];
    }
        
    public UInt32 ToUInt32() {
        return (uint)_b0 << 16 | (uint)_b1 << 8 | _b2;
    }

    public byte[] ToByteArray() {
        return new[] { _b0, _b1, _b2 };
    }

    public override string ToString() {
        return ToUInt32().ToString();
    }

    public static implicit operator UInt32(UInt24 u) {
        return u.ToUInt32();
    }

    public static implicit operator byte[](UInt24 u) {
        return u.ToByteArray();
    }

    public static implicit operator UInt24(byte[] u) {
        return new UInt24(u);
    }
        
    public static implicit operator UInt24(UInt32 u) {
        return new UInt24(u);
    }

    public UInt32 Value => ToUInt32();
}