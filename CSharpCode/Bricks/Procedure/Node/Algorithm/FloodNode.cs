using System.Collections.Generic;

namespace EngineNS.Bricks.Procedure.Algorithm
{
    public class FloodNode
    {
        const string FlowScatterPath = "FlowScatter";
        const string FlowAtomicPath = "FlowAtomic";
        public static UBufferConponent GenerateFlood_Scatter(UBufferConponent HeightMap, float water_amount, int step_multiplier)
        {
            return null;
            //int width = HeightMap.Width;
            //int count = width * width;
            //ComputeShader ComputeFlow = Resources.Load(FlowScatterPath) as ComputeShader;

            //int SimulateFlowKernel = ComputeFlow.FindKernel("SimulateFlow");
            //int DrawPoolKernel = ComputeFlow.FindKernel("DrawPool");

            //RenderTexture TempResult = new RenderTexture(width, width, 0,RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            //TempResult.enableRandomWrite = true;
            //TempResult.Create();

            //float[] heightArray = new float[count];
            //for (int i = 0; i < width; i++) for (int j = 0; j < width; j++) heightArray[i + j * width] = HeightMap.GetPixel(i, j).r;
            //ComputeBuffer HeightBuffer = new ComputeBuffer(count, sizeof(float));
            //HeightBuffer.SetData(heightArray);

            //float[] waterArray = new float[count];
            //for (int i = 0; i < count; i++) waterArray[i] = water_amount;
            //ComputeBuffer WaterBuffer = new ComputeBuffer(count, sizeof(float));
            //WaterBuffer.SetData(waterArray);

            //// Compute Target
            //int cycles = width * step_multiplier;
            //int groupX = Mathf.CeilToInt(width / 8.0f);
            //{
            //    ComputeFlow.SetInt("Width", width);
            //    ComputeFlow.SetInt("Height", width);

            //    ComputeFlow.SetBuffer(SimulateFlowKernel, "HeightBuffer", HeightBuffer);
            //    ComputeFlow.SetBuffer(SimulateFlowKernel, "WaterBuffer", WaterBuffer);
            //    ComputeFlow.SetBuffer(DrawPoolKernel, "WaterBuffer", WaterBuffer);
            //    ComputeFlow.SetTexture(DrawPoolKernel, "Result", TempResult);

            //    for (int i = 0; i < cycles; i++)
            //    {
            //        for (int j = 0; j < 5; j++)
            //        {
            //            ComputeFlow.SetInt("Step", j);
            //            ComputeFlow.Dispatch(SimulateFlowKernel, groupX, groupX, 1);
            //        }
            //    }
            //    ComputeFlow.Dispatch(DrawPoolKernel, groupX, groupX, 1);
            //}

            //Debug.Log("End Compute Flow");

            //HeightBuffer.Dispose();
            //WaterBuffer.Dispose();

            //Texture2D tex = new Texture2D(TempResult.width, TempResult.width, TextureFormat.RGB24, false);
            //RenderTexture.active = TempResult;
            //tex.ReadPixels(new Rect(0, 0, TempResult.width, TempResult.height), 0, 0);
            //tex.Apply();
            //return tex;
        }
        public static UBufferConponent GenerateFlood_Atomic(UBufferConponent HeightMap, float water_amount, int step_multiplier)
        {
            return null;
            //int width = HeightMap.width;
            //int count = width * width;
            //ComputeShader ComputeFlowAtomic = Resources.Load(FlowAtomicPath) as ComputeShader;

            //int CalculateTargetKernel = ComputeFlowAtomic.FindKernel("CalculateTarget");
            //int SimulateFlowKernel = ComputeFlowAtomic.FindKernel("SimulateFlow");
            //int DrawPoolKernel = ComputeFlowAtomic.FindKernel("DrawPool");

            //RenderTexture TempResult = new RenderTexture(width, width, 0,RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            //TempResult.enableRandomWrite = true;
            //TempResult.Create();

            //int[] heightArray = new int[count];
            //for (int i = 0; i < width; i++) for (int j = 0; j < width; j++) heightArray[i + j * width] =  Mathf.FloorToInt(HeightMap.GetPixel(i, j).r * 1000);
            //ComputeBuffer HeightBuffer = new ComputeBuffer(count, sizeof(int));
            //HeightBuffer.SetData(heightArray);

            //int[] waterArray = new int[count];
            //for (int i = 0; i < count; i++) waterArray[i] = Mathf.FloorToInt(water_amount * 1000);
            //ComputeBuffer WaterBuffer = new ComputeBuffer(count, sizeof(int));
            //WaterBuffer.SetData(waterArray);

            //int[] targetArray = new int[count];
            //for (int i = 0; i < count; i++) targetArray[i] = 0;
            //ComputeBuffer TargetBuffer = new ComputeBuffer(count, sizeof(int));
            //TargetBuffer.SetData(targetArray);

            //// Compute Target
            //int cycles = width * step_multiplier;
            //int groupX = Mathf.CeilToInt(width / 8.0f);
            //{
            //    ComputeFlowAtomic.SetInt("Width", width);
            //    ComputeFlowAtomic.SetInt("Height", width);

            //    ComputeFlowAtomic.SetBuffer(CalculateTargetKernel, "HeightBuffer", HeightBuffer);
            //    ComputeFlowAtomic.SetBuffer(CalculateTargetKernel, "WaterBuffer", WaterBuffer);
            //    ComputeFlowAtomic.SetBuffer(CalculateTargetKernel, "TargetBuffer", TargetBuffer);

            //    ComputeFlowAtomic.SetBuffer(SimulateFlowKernel, "HeightBuffer", HeightBuffer);
            //    ComputeFlowAtomic.SetBuffer(SimulateFlowKernel, "WaterBuffer", WaterBuffer);
            //    ComputeFlowAtomic.SetBuffer(SimulateFlowKernel, "TargetBuffer", TargetBuffer);

            //    ComputeFlowAtomic.SetBuffer(DrawPoolKernel, "WaterBuffer", WaterBuffer);
            //    ComputeFlowAtomic.SetTexture(DrawPoolKernel, "Result", TempResult);

            //    for (int i = 0; i < cycles; i++)
            //    {
            //        ComputeFlowAtomic.Dispatch(CalculateTargetKernel, groupX, groupX, 1);
            //        ComputeFlowAtomic.Dispatch(SimulateFlowKernel, groupX, groupX, 1);
            //    }


            //    ComputeFlowAtomic.Dispatch(DrawPoolKernel, groupX, groupX, 1);
            //}

            //Debug.Log("End Compute Flow Atomic");

            //HeightBuffer.Dispose();
            //WaterBuffer.Dispose();
            //TargetBuffer.Dispose();

            //Texture2D tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            //RenderTexture.active = TempResult;
            //tex.ReadPixels(new Rect(0, 0, TempResult.width, TempResult.height), 0, 0);
            //tex.Apply();
            //return tex;
        }
    }
}