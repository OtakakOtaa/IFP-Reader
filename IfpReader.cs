using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace IFP_reader;

public sealed partial class IfpReader : BinaryReader
{
    private IfpHeader? _ifpDefinition;
    
    
    public IfpReader(string filePath) : base(new FileStream(filePath, FileMode.Open), IfpEncoding) { }
    private IfpReader(Stream input) : base(input) { }
    private IfpReader(Stream input, Encoding encoding) : base(input, encoding) { }
    private IfpReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

    
    
    public IfpHeader ReadHeader(bool asPeek = false)
    {
        BaseStream.Position = 0;
        
        var fileMagic = ReadFixedString(IfpHeader.FileMagicLength, asPeek: true);
        if (fileMagic != IfpHeader.FileMagicDefinition) throw HeaderException;
        BaseStream.Position = IfpHeader.FileMagicLength;

        var tailOffset = ReadInt32();
        var fileName = ReadFixedString(IfpHeader.StringConstantLength);
        var numberOfAnims = ReadInt32();
        var headerOffset = BaseStream.Position;

        BaseStream.Position = asPeek ? 0 : headerOffset;
        
        _ifpDefinition = new IfpHeader
        {
            FileName = fileName, 
            HeaderOffset = (int)headerOffset,
            TailOffset = tailOffset,
            AnimsCount = numberOfAnims,
        };

        return _ifpDefinition.Value;
    }

    public bool TryReadNextAnim(out IfpAnimRecord anim)
    {
        anim = default;
        
        _ifpDefinition ??= ReadHeader();
        if (BaseStream.Position == 0)
        {
            BaseStream.Position = _ifpDefinition.Value.HeaderOffset;
        }

        if (CheckForEnd()) return false;
        
        var animName = ReadFixedString(IfpHeader.StringConstantLength);
        var objsCount = ReadInt32();
        var framesDataSize = ReadInt32();
        BaseStream.Position += 4; // step over unknown value
        var objects = ReadNextSpanOf(objsCount, ReadNextObject);

        anim = new IfpAnimRecord
        {
            Name = animName,
            Objects = objects,
        };

        return true;
    }

    public bool CheckForEnd()
    {
        _ifpDefinition ??= ReadHeader();
        
        var isOverRead = BaseStream.Position >= _ifpDefinition.Value.TailOffset + IfpHeader.FileDefinitionOffset;
        
        return _ifpDefinition.Value.AnimsCount == 0 || isOverRead;
    }
    
    public char[] PeekAllDataAsChar()
    {
        BaseStream.Position = 0;
        var buffer = new byte[BaseStream.Length];
        var _ = BaseStream.Read(buffer, 0, (int)BaseStream.Length);
        var res = buffer.Select(s => (char)s).ToArray();
        BaseStream.Position = 0;

        return res;
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private IfpAnimObject ReadNextObject()
    {
        var objName = ReadFixedString(IfpHeader.StringConstantLength);
        var framesType = (IfpFrameType)ReadInt32();
        var framesCount = ReadInt32();
        var boneId = ReadInt32();
        var frames = ReadNextSpanOf(framesCount, ReadNextFrame, framesType);
        
        return new IfpAnimObject
        {
            Name = objName,
            FramesType = framesType,
            BoneId = boneId,
            Frames = frames,
        };
    }
    
    private IfpFrame ReadNextFrame(IfpFrameType frameType)
    {
        var rotation = ReadQuaternion();
        var time = ReadInt16();

        Vector3 translation = default;
        if (frameType is IfpFrameType.Root)
        {
            translation = ReadVector3();
        }
        
        return new IfpFrame
        {
            FrameType = frameType,
            Rotation = rotation,
            Time = time,
            Translation = translation,
        };
    }

    private Quaternion ReadQuaternion()
    {
        return new Quaternion
        {
            X = ReadInt16(),
            Y = ReadInt16(),
            Z = ReadInt16(),
            W = ReadInt16(),
        };
    }
    private Vector3 ReadVector3()
    {
        return new Vector3
        {
            X = ReadInt16(),
            Y = ReadInt16(),
            Z = ReadInt16(),
        };
    }

    private string ReadFixedString(int length, bool asPeek = false)
    {
        var str = Encoding.UTF8.GetString(ReadBytes(length));
        if (asPeek)
        {
            BaseStream.Position -= length;
        }
        
        return str[.. GetLatsStringPayloadIndex(str)];
    }

    private int GetLatsStringPayloadIndex(string str)
    {
        var chars = str.ToCharArray();

        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] == N0) return i;
        }

        return chars.Length;
    }
    
    private TType[] ReadNextSpanOf<TType>(int count, Func<TType> stepFunc) where TType : struct
    {
        var finalSpan = new TType[count];
        
        for (var i = 0; i < count; i++)
        {
            finalSpan[i] = stepFunc();
        }

        return finalSpan;
    }
    
    [SuppressMessage("ReSharper", "HeapView.CanAvoidClosure")]
    private TType[] ReadNextSpanOf<TType, TParam>(int count, Func<TParam, TType> stepFunc, TParam param) where TType : struct
    {
        return ReadNextSpanOf(count, () => stepFunc(param));
    }
}