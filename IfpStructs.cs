using System.Numerics;

namespace IFP_reader;

public struct IfpAnimRecord
{
    public string Name { init; get; }
    public IfpAnimObject[] Objects { init; get; }
}

public struct IfpAnimObject
{
    public string Name { init; get; }
    public IfpFrameType FramesType { init; get; }
    public Int32 BoneId { init; get; }
    public IfpFrame[] Frames { init; get; }
}

public enum IfpFrameType : Int32
{
    None = 0,
    Child = 3,
    Root = 4,
}

public struct IfpFrame
{
    public IfpFrameType FrameType { init; get; }
    public Quaternion Rotation { init; get; }
    
    /// <summary> in seconds </summary>
    public Int16 Time { init; get; }
    
    /// <summary>if IpfFrame.FrameType is Child - value of that field will be equals with default</summary>
    public Vector3 Translation { init; get; }
}
