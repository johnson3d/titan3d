#ifndef _WAVE_TASK_H_
#define _WAVE_TASK_H_

#define WAVE_SIZE 32
#define WAVE_COUNT 2
#define GROUP_SIZE (WAVE_SIZE * WAVE_COUNT)

groupshared uint WorkBoundary[GROUP_SIZE];

template<typename Task>
void DoTasks(Task task,uint groupIndex, uint numOfSubTask)
{
    const uint waveCount = WAVE_SIZE;
    uint waveLane = groupIndex & (waveCount - 1); //求余为lane
    uint waveOffset = groupIndex & ~(waveCount - 1); //得到本wave在Group里面的偏移
    uint sourceLane = waveLane;
        
    int start = 0;
    int end = 0;
    uint workStart = 0;
    uint workEnd = 0;
    bool isLaneWork = numOfSubTask != 0;
    if (WaveActiveAnyTrue(numOfSubTask != 0))
    {
        workStart = WavePrefixSum(numOfSubTask);
        workEnd = workStart + numOfSubTask;
        end = WaveReadLaneAt(workEnd, waveCount - 1);

        uint compactIndex = WavePrefixCountBits(isLaneWork);

        if (isLaneWork)
        {
            WorkBoundary[waveOffset + compactIndex] = waveLane;
        }
        GroupMemoryBarrier();
            
        sourceLane = WorkBoundary[compactIndex];
            
        workEnd = WaveReadLaneAt(workEnd, sourceLane);
            
        if (waveLane >= WaveActiveCountBits(isLaneWork))
        {
            workEnd = end + waveCount;
        }
    }
        
    int curSubTaskGroup = 0;
    while (start < end)
    {
        WorkBoundary[groupIndex] = 0;
        GroupMemoryBarrier();
            
        uint endInWave = workEnd - start - 1;
        if (endInWave < numOfSubTask)
        {
            WorkBoundary[waveOffset + endInWave] = 1;
        }
        GroupMemoryBarrier();
            
        bool isEndInWave = WorkBoundary[groupIndex];
            
        uint subTaskIndexGroup = curSubTaskGroup + WavePrefixCountBits(isEndInWave);
        uint curSource = WaveReadLaneAt(sourceLane, subTaskIndexGroup);
            
        uint curStart = select(subTaskIndexGroup > 0, WaveReadLaneAt(workEnd, subTaskIndexGroup - 1), 0);
        uint subTaskIndex = start + waveLane - curStart;
            
        Task subTask = task.CreateSubTask(curSource);
        if (start + waveLane < end)
        {
            subTask.RunSubTask(subTaskIndex);
        }
            
        start += waveCount;
        subTaskIndexGroup += WaveActiveCountBits(isEndInWave);
    }
}

struct TtTestTask
{
    float2 TaskData;
    TtTestTask CreateSubTask(uint taskLane)
    {
        TtTestTask result = (TtTestTask) 0;
        return result;
    }
    void RunSubTask(uint subTaskIndex)
    {
        
    }
};

#endif//#ifndef _WAVE_TASK_H_