/*
 * Created on Thu Dec 12 2019
 *
 * The MIT License (MIT)
 * Copyright (c) 2019 Sahil Jain
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Runtime.InteropServices;
using GameplayAbilitySystem.Common.Components;
using GameplayAbilitySystem.GameplayEffects.Components;
using GameplayAbilitySystem.GameplayEffects.Systems;
using MyGameplayAbilitySystem.GameplayEffects.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericComponentType(typeof(PeriodicTickActionComponent<PeriodicTickDelegate>))]

namespace MyGameplayAbilitySystem.GameplayEffects.Systems {
    public delegate void PeriodicTickDelegate(int index, EntityCommandBuffer.Concurrent Ecb, Entity Target, Entity ParentGameplayEffectEntity);

    public class BasicPeriodicTickSystem : GameplayEffectTickSystem {

        protected override void OnCreate() {
            base.OnCreate();
        }

        struct TickJob : IJobForEachWithEntity<ParentGameplayEffectEntity, PeriodicTickTargetComponent, PeriodicTickComponent, PeriodicTickActionComponent<PeriodicTickDelegate>> {
            public EntityCommandBuffer.Concurrent Ecb;
            public void Execute(Entity entity, int index, ref ParentGameplayEffectEntity parentGameplayEffectEntity, ref PeriodicTickTargetComponent target, ref PeriodicTickComponent tick, ref PeriodicTickActionComponent<PeriodicTickDelegate> action) {
                if (tick.TickDurationLeft > 0) return;
                action.Tick.Invoke(index, Ecb, target, parentGameplayEffectEntity);
            }
        }
        protected override JobHandle Tick(JobHandle inputDeps) {
            var a = GetComponentDataFromEntity<ParentGameplayEffectEntity>();
            inputDeps = new TickJob
            {
                Ecb = m_EntityCommandBuffer.CreateCommandBuffer().ToConcurrent()


            }.Schedule(this, inputDeps);
            return inputDeps;
        }
    }
}