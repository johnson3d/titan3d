using EngineNS.Bricks.NodeGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure.Node
{
    public class UUnpackNode : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn ValuePin { get; set; } = new PinIn();

        Rtti.UTypeDesc mType;
        [Rtti.Meta]
        public Rtti.UTypeDesc Type
        {
            get => mType;
            set
            {
                if (mType == value)
                    return;
                mType = value;
                UpdateWithType(mType);
            }
        }

        protected void UpdateWithType(Rtti.UTypeDesc type)
        {
            Type generic = typeof(USuperBuffer<,>);
            var pros = type.SystemType.GetFields();
            Type[] typeArgs = new Type[2];
            for(int i = 0; i < pros.Length; i++)
            {
                typeArgs[0] = pros[i].FieldType;
                typeArgs[1] = UBufferCreator.GetBufferOperatorType(pros[i].FieldType);
                var tagType = generic.MakeGenericType(typeArgs);
                AddOutput(new PinOut(), pros[i].Name, UBufferCreator.CreateInstance(Rtti.UTypeDesc.TypeOf(tagType)));
            }

            typeArgs[0] = type.SystemType;
            typeArgs[1] = UBufferCreator.GetBufferOperatorType(type.SystemType);
            var inputType = generic.MakeGenericType(typeArgs);
            var inputBuffer = UBufferCreator.CreateInstance(Rtti.UTypeDesc.TypeOf(inputType));
            AddInput(ValuePin, "Value", inputBuffer);
        }

        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            for(int i=0; i<Outputs.Count; i++)
            {
                if (pin == Outputs[i])
                    return Outputs[i].Tag as UBufferCreator;
            }
            return null;
        }

        public override unsafe bool OnProcedure(UPgcGraph graph)
        {
            if (Type == null)
                return false;
            var input = graph.BufferCache.FindBuffer(ValuePin);
            if (input.ElementSize != System.Runtime.InteropServices.Marshal.SizeOf(Type.SystemType))
                return false;
            var buffers = new UBufferConponent[Outputs.Count];
            for(int i=0; i<Outputs.Count;++i)
            {
                buffers[i] = graph.BufferCache.FindBuffer(Outputs[i]);
            }

            for(int z=0; z<input.Depth; z++)
            {
                for(int y = 0; y<input.Height; y++)
                {
                    for(int x = 0; x<input.Width; x++)
                    {
                        var size = System.Runtime.InteropServices.Marshal.SizeOf(Type.SystemType);
                        byte* inValAdr = input.GetSuperPixelAddress(x, y, z);
                        for(int bufIdx = 0; bufIdx < buffers.Length; bufIdx++)
                        {
                            var bc = Outputs[bufIdx].Tag as UBufferCreator;
                            var fieldTypeSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(bc.ElementType.SystemType);
                            var valAdr = buffers[bufIdx].GetSuperPixelAddress(x, y, z);
                            if (valAdr == null)
                                continue;
                            var offset = System.Runtime.InteropServices.Marshal.OffsetOf(Type.SystemType, Outputs[bufIdx].Name).ToInt32();
                            var inputFieldAdr = inValAdr + offset;
                            CoreSDK.MemoryCopy(valAdr, inputFieldAdr, fieldTypeSize);
                        }
                    }
                }
            }

            return true;
        }
    }

    public class UPackNode : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        UBufferCreator mResultBuffer;
        public UBufferCreator ResultBuffer => mResultBuffer;

        Rtti.UTypeDesc mType;
        [Rtti.Meta]
        public Rtti.UTypeDesc Type 
        {
            get => mType;
            set
            {
                if (mType == value)
                    return;
                mType = value;
                UpdateWithType(mType);
            }
        }

        protected void UpdateWithType(Rtti.UTypeDesc type)
        {
            Type generic = typeof(USuperBuffer<,>);
            var pros = type.SystemType.GetFields();
            Type[] typeArgs = new Type[2];
            for (int i=0; i < pros.Length; i++)
            {
                typeArgs[0] = pros[i].FieldType;
                typeArgs[1] = UBufferCreator.GetBufferOperatorType(pros[i].FieldType);
                var tagType = generic.MakeGenericType(typeArgs);
                AddInput(new PinIn(), pros[i].Name, UBufferCreator.CreateInstance(Rtti.UTypeDesc.TypeOf(tagType)));
            }

            typeArgs[0] = type.SystemType;
            typeArgs[1] = UBufferCreator.GetBufferOperatorType(type.SystemType);
            var resultType = generic.MakeGenericType(typeArgs);
            mResultBuffer = UBufferCreator.CreateInstance(Rtti.UTypeDesc.TypeOf(resultType));
            AddOutput(ResultPin, "Result", mResultBuffer);
        }

        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
                return mResultBuffer;
            return null;
        }

        public override unsafe bool OnProcedure(UPgcGraph graph)
        {
            if (Type == null)
                return false;
            var result = graph.BufferCache.FindBuffer(ResultPin);
            if (result.ElementSize != System.Runtime.InteropServices.Marshal.SizeOf(Type.SystemType))
                return false;

            var buffers = new UBufferConponent[Inputs.Count];
            for(int i=0; i<Inputs.Count; i++)
            {
                buffers[i] = graph.BufferCache.FindBuffer(Inputs[i]);
                if (buffers[i] == null)
                    return false;
            }

            for(int z = 0; z < result.Depth; z++)
            {
                for(int y = 0; y < result.Height; y++)
                {
                    for(int x = 0; x < result.Width; x++)
                    {
                        var size = System.Runtime.InteropServices.Marshal.SizeOf(Type.SystemType);
                        byte* resultAdr = result.GetSuperPixelAddress(x, y, z);
                        for(int bufIdx = 0; bufIdx < buffers.Length; bufIdx++)
                        {
                            var bc = Inputs[bufIdx].Tag as UBufferCreator;
                            var fieldTypeSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(bc.ElementType.SystemType);
                            var valAdr = buffers[bufIdx].GetSuperPixelAddress(x, y, z);
                            if (valAdr == null)
                                continue;
                            var offset = System.Runtime.InteropServices.Marshal.OffsetOf(Type.SystemType, Inputs[bufIdx].Name).ToInt32();
                            var resultFieldAdr = resultAdr + offset;
                            CoreSDK.MemoryCopy(resultFieldAdr, valAdr, fieldTypeSize);
                        }
                    }
                }
            }

            return true;
        }
    }
}
