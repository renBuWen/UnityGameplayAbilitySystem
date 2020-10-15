/*
 * Created on Fri Dec 27 2019
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

using System;
using System.Collections;
using System.Runtime.InteropServices;
using GameplayAbilitySystem.Abilities.Components;
using GameplayAbilitySystem.Abilities.Systems;
using GameplayAbilitySystem.Abilities.Systems.Generic;
using GameplayAbilitySystem.AbilitySystem.Enums;
using GameplayAbilitySystem.ExtensionMethods;
using MyGameplayAbilitySystem.AbilitySystem.MonoBehaviours;
using MyGameplayAbilitySystem.GameplayEffects.Components;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace MyGameplayAbilitySystem.Abilities.DefaultAttack {

    public class DefaultAttack2AbilitySystem {
        // public class AbilityCooldownSystem : GenericAbilityCooldownSystem<DefaultAttackAbilityTag> {
        //     protected override ComponentType[] CooldownEffects =>
        //         new ComponentType[] {
        //             ComponentType.ReadOnly<GlobalCooldownGameplayEffectComponent>()
        //         };

        // }

        // public class AbilityAvailabilitySystem : AbilityAvailabilitySystem<DefaultAttackAbilityTag> {
        //     // private EntityQuery m_Query;
        //     // protected override void OnCreate() {
        //     //     this.m_Query = GetEntityQuery(ComponentType.ReadOnly<DefaultAttackAbilityActive>(), ComponentType.ReadWrite<AbilityStateComponent>());
        //     // }

        //     [RequireComponentTag(typeof(DefaultAttackAbilityActive))]
        //     struct SystemJob : IJobForEach<AbilityStateComponent> {
        //         public void Execute(ref AbilityStateComponent abilityState) {
        //             abilityState |= (int)AbilityStates.ACTIVE;
        //         }
        //     }
        //     protected override JobHandle UpdateAbilityAvailability(JobHandle inputDeps) {
        //         // Check for existence of AbilityActive tag
        //         inputDeps = inputDeps.ScheduleJob(new SystemJob(), this);
        //         return inputDeps;
        //     }
        // }

        // public class AssignAbilityIdentifierSystem : GenericAssignAbilityIdentifierSystem<DefaultAttackAbilityTag> { }

    }

}
