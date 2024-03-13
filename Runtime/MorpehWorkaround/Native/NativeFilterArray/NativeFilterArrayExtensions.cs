#if MORPEH_BURST
using Scellecs.Morpeh.Native;
using Scellecs.Morpeh.Workaround.WorldAllocator;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Workaround
{
    public static unsafe class NativeFilterArrayExtensions
    {
        private const int SIZE_OF_INT = sizeof(int);
        private const int ENTITIES_BURST_THRESHOLD = 5000;

        public static NativeFilterArray AsNativeFilterArray(this Filter filter)
        {
            var allocator = filter.world.GetUpdateAllocator();

            int archetypesCount = filter.archetypes.length;
            int slotsCount = 0;
            int entitiesCount = 0;

            for (int i = 0; i < archetypesCount; i++)
            {
                var bitmap = filter.archetypes.data[i].entities;
                slotsCount += bitmap.length;
                entitiesCount += bitmap.count;
            }

            if (entitiesCount < ENTITIES_BURST_THRESHOLD)
            {
                var entityIdsPtr = allocator.Allocate<int>(entitiesCount, NativeArrayOptions.UninitializedMemory);
                int counter = 0;

                for (int i = 0; i < archetypesCount; i++)
                {
                    var bitmap = filter.archetypes.data[i].entities;

                    foreach (var entityId in bitmap)
                    {
                        entityIdsPtr[counter++] = entityId;
                    }
                }

                return new NativeFilterArray(default, filter.world.AsNative(), entitiesCount, entityIdsPtr);
            }
            else
            {

                int slotsCountAligned = Align128Bit(slotsCount * SIZE_OF_INT) / SIZE_OF_INT;
                int entitiesCountAligned = Align128Bit(entitiesCount * SIZE_OF_INT) / SIZE_OF_INT;

                var slotsPtr = allocator.Allocate<int>(slotsCountAligned, NativeArrayOptions.UninitializedMemory);
                var datasPtr = allocator.Allocate<int>(slotsCountAligned, NativeArrayOptions.UninitializedMemory);
                var countBitsCumulativePtr = allocator.Allocate<int>(slotsCountAligned, NativeArrayOptions.UninitializedMemory);
                var entityIdsPtr = allocator.Allocate<int>(entitiesCountAligned, NativeArrayOptions.UninitializedMemory);

                var alignmentOverageSize = (slotsCountAligned - slotsCount) * SIZE_OF_INT;

                if (alignmentOverageSize > 0)
                {
                    UnsafeUtility.MemClear(slotsPtr + slotsCount, alignmentOverageSize);
                    UnsafeUtility.MemClear(datasPtr + slotsCount, alignmentOverageSize);
                    UnsafeUtility.MemClear(countBitsCumulativePtr + slotsCount, alignmentOverageSize);
                }

                var preprocessJobs = stackalloc JobHandle[archetypesCount];
                int startIndex = 0;
                int countBitsCumulativeGlobal = 0;

                for (int i = 0; i < archetypesCount; i++)
                {
                    var bitmap = filter.archetypes.data[i].entities;
                    var nativeBitmap = bitmap.AsNative();

                    preprocessJobs[i] = new PreprocessCopyBitmapJob()
                    {
                        sourceSlots = nativeBitmap.slotsPtr,
                        sourceDatas = nativeBitmap.dataPtr,
                        slots = slotsPtr,
                        datas = datasPtr,
                        cumulativeCount = countBitsCumulativePtr,
                        lastIndex = nativeBitmap.lastIndexPtr,
                        startIndex = startIndex,
                        cumulativeCountBitsGlobal = countBitsCumulativeGlobal
                    }
                    .Schedule();

                    startIndex += bitmap.length;
                    countBitsCumulativeGlobal += bitmap.count;
                }

                var resultJob = new WriteEntityIdsToFilterArrayJob()
                {
                    slots = (int4*)slotsPtr,
                    datas = (int4*)datasPtr,
                    cumulativeCount = (int4*)countBitsCumulativePtr,
                    destination = entityIdsPtr
                }
                .ScheduleParallel(slotsCountAligned / SIZE_OF_INT, 32, JobHandleUnsafeUtility.CombineDependencies(preprocessJobs, archetypesCount));

                return new NativeFilterArray(resultJob, filter.world.AsNative(), entitiesCount, entityIdsPtr);
            }
        }

        private static int Align128Bit(int size)
        {
            return ((size + 15) >> 4) << 4;
        }
    }

    [BurstCompile]
    internal unsafe struct PreprocessCopyBitmapJob : IJob
    {
        [NativeDisableUnsafePtrRestriction] public int* slots;
        [NativeDisableUnsafePtrRestriction] public int* datas;
        [NativeDisableUnsafePtrRestriction] public int* cumulativeCount;

        [NativeDisableUnsafePtrRestriction] public int* sourceSlots;
        [NativeDisableUnsafePtrRestriction] public int* sourceDatas;
        [NativeDisableUnsafePtrRestriction] public int* lastIndex;

        [ReadOnly] public int startIndex;
        [ReadOnly] public int cumulativeCountBitsGlobal;

        public void Execute()
        {
            int currentIndex = startIndex;
            int countBits = cumulativeCountBitsGlobal;
            int last = *lastIndex;

            for (int i = 0; i < last; i += 2)
            {
                var dataIndex = sourceSlots[i] - 1;

                if (dataIndex < 0)
                {
                    continue;
                }

                var data = sourceDatas[i >> 1];
                slots[currentIndex] = dataIndex;
                datas[currentIndex] = data;
                cumulativeCount[currentIndex] = countBits;

                countBits += math.countbits(data);
                currentIndex++;
            }
        }
    }

    [BurstCompile]
    internal unsafe struct WriteEntityIdsToFilterArrayJob : IJobFor
    {
        [NativeDisableUnsafePtrRestriction] public int4* slots;
        [NativeDisableUnsafePtrRestriction] public int4* datas;
        [NativeDisableUnsafePtrRestriction] public int4* cumulativeCount;
        [NativeDisableUnsafePtrRestriction] public int* destination;

        public void Execute(int index)
        {
            int destinationStartIndex = cumulativeCount[index].x;

            int4 slot = slots[index];
            int4 data = datas[index];
            int4 dataIndices = slot * NativeBitMap.BITS_PER_FIELD;

            while (math.any(data))
            {
                int4 tzcnt = math.tzcnt(data);

                for (int j = 0; j < 4; j++)
                {
                    if (tzcnt[j] != 32)
                    {
                        destination[destinationStartIndex++] = tzcnt[j] + dataIndices[j];
                    }
                }

                int4 mask = new int4(data != 0);
                data &= data - mask;
            }
        }
    }
}
#endif
