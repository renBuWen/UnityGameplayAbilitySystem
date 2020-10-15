/*
 * Created on Mon Nov 04 2019
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

using System.Collections;
using GameplayAbilitySystem.Abilities.Components;
using GameplayAbilitySystem.Attributes.Components;
using GameplayAbilitySystem.Common.Editor;
using GameplayAbilitySystem.GameplayEffects.Components;
using MyGameplayAbilitySystem.AbilitySystem.MonoBehaviours;
using MyGameplayAbilitySystem.GameplayEffects.Components;
using MyGameplayAbilitySystem.GameplayEffects.Systems;
using Unity.Entities;
using UnityEngine;
using Components = GameplayAbilitySystem.Attributes.Components;
using GameplayAbilitySystem.Common.Components;
using System;

namespace MyGameplayAbilitySystem.Abilities.DefaultAttack {
    [AbilitySystemDisplayName("Default Attack Ability")]
    public struct DefaultAttackAbilityTag : IAbilityTagComponent, IComponentData {
        private const string AnimationStartTriggerName = "DoSwingAttack";
        private const string swingStateName = "Weapon.Swing";

        public int AbilityIdentifier => 1;

        public object EmptyPayload => new BasicMeleeAbilityPayload();

        public void CreateCooldownEntities(EntityManager dstManager, Entity actorEntity) {


            var tickEntity = new PeriodicTickActionComponent<PeriodicTickDelegate>()
                                .SetTickFunction(
                                    ((index, Ecb, entity, parentGameplayEffectEntity) => {
                                        new PermanentAttributeModifierTag() { }.CreateAttributeModifier<ManaAttributeComponent, Components.Operators.Add>(index, Ecb, entity, 0.1f);
                                    })
                                )
                                .CreateEntity(dstManager);

            dstManager.SetComponentData<PeriodicTickComponent>(tickEntity, new PeriodicTickComponent()
            {
                TickPeriod = 0.2f,
                TickDurationLeft = 1
            });
            dstManager.SetComponentData<PeriodicTickTargetComponent>(tickEntity, actorEntity);

            Entity cooldownEntity1 = new GlobalCooldownGameplayEffectComponent().Instantiate(dstManager, actorEntity, 1f);
            dstManager.SetComponentData<ParentGameplayEffectEntity>(tickEntity, new ParentGameplayEffectEntity(cooldownEntity1));

            new TemporaryAttributeModifierTag() { ParentGameplayEffectEntity = cooldownEntity1 }.CreateAttributeModifier<ManaAttributeComponent, Components.Operators.Add>(dstManager, actorEntity, -5);
        }

        public void CreateSourceAttributeModifiers(EntityManager dstManager, Entity actorEntity) {
            var entity = new PermanentAttributeModifierTag()
                    .CreateAttributeModifier<ManaAttributeComponent, Components.Operators.Add>(dstManager, actorEntity, -1);
        }

        public void CommitAbility(EntityManager dstManager, Entity actorEntity) {
            CreateCooldownEntities(dstManager, actorEntity);
            CreateSourceAttributeModifiers(dstManager, actorEntity);
        }
        public void CreateTargetAttributeModifiers(EntityManager dstManager, Entity actorEntity) {
            var attributeEntity = new PermanentAttributeModifierTag()
                                .CreateAttributeModifier<HealthAttributeComponent, Components.Operators.Add>(dstManager, actorEntity, -5f);
            // Create a "poison" effect
            Entity poisonEffectEntity = new PoisonGameplayEffectComponent().Instantiate(dstManager, actorEntity, 25f);

            var tickEntity = new PeriodicTickActionComponent<PeriodicTickDelegate>()
                                .SetTickFunction(
                                    ((index, Ecb, entity, parentGameplayEffectEntity) => {
                                        new PoisonTickGameplayEffectComponent() { Damage = -1f }.Instantiate(index, Ecb, entity, -1f);
                                    })
                                )
                                .CreateEntity(dstManager);

            dstManager.SetComponentData<PeriodicTickComponent>(tickEntity, new PeriodicTickComponent()
            {
                TickPeriod = 0.9f,
                TickDurationLeft = 1
            });
            dstManager.SetComponentData<PeriodicTickTargetComponent>(tickEntity, actorEntity);
            dstManager.SetComponentData<ParentGameplayEffectEntity>(tickEntity, new ParentGameplayEffectEntity(poisonEffectEntity));
        }

        public void BeginActivateAbility(EntityManager dstManager, Entity grantedAbilityEntity) {
            // Check if entity already has the "Active" component - return if existing
            if (dstManager.HasComponent<DefaultAttackAbilityActive>(grantedAbilityEntity)) return;

            // Add component to entity
            dstManager.AddComponentData<DefaultAttackAbilityActive>(grantedAbilityEntity, new DefaultAttackAbilityActive());
        }

        public void EndActivateAbility(EntityManager dstManager, Entity grantedAbilityEntity) {
            // Check if entity already has the "Active" component - return if not existing
            if (!dstManager.HasComponent<DefaultAttackAbilityActive>(grantedAbilityEntity)) return;

            // Remove component from entity
            dstManager.RemoveComponent<DefaultAttackAbilityActive>(grantedAbilityEntity);
        }

        public IEnumerator CheckAbilityHit(EntityManager EntityManager, Entity sourceEntity, Entity targetEntity) {
            yield return null;
        }

        public IEnumerator DoAbility(object Payload) {
            if (Payload is BasicMeleeAbilityPayload payload) {
                payload.ActorAbilitySystem.StartCoroutine(AbilityActionLogic(payload));
            } else {
                Debug.LogWarningFormat("The payload passed to {0} does not match the expected payload format.", this.GetType());
            }

            yield return null;
        }

        private IEnumerator AbilityActionLogic(BasicMeleeAbilityPayload payload) {
            var entityManager = payload.EntityManager;
            var transform = payload.ActorTransform;
            var actorAbilitySystem = payload.ActorAbilitySystem;
            var grantedAbilityEntity = payload.GrantedAbilityEntity;
            // Check ability state
            var abilityStateComponent = entityManager.GetComponentData<AbilityStateComponent>(grantedAbilityEntity);

            if (abilityStateComponent.Value != 0) yield break;
            var animator = actorAbilitySystem.GetComponent<Animator>();
            var animatorLayerName = "Weapon";
            var animatorStateName = animatorLayerName + ".Swing";
            var animatorStateFullHash = Animator.StringToHash(animatorStateName);
            var animatorLayerIndex = animator.GetLayerIndex(animatorLayerName);

            // If we aren't in idle, then do nothing
            if (!animator.GetCurrentAnimatorStateInfo(animatorLayerIndex).IsName("Idle")) yield break;
            BeginActivateAbility(entityManager, grantedAbilityEntity);
            CreateSourceAttributeModifiers(entityManager, actorAbilitySystem.AbilityOwnerEntity);
            // Get animator state info
            var weaponLayerAnimatorStateInfo = GetAnimatorStateInfo(animator, animatorLayerIndex, swingStateName);

            animator.SetTrigger(AnimationStartTriggerName);
            // Wait to reach the "Swing" state
            while (!IsInOrEnteringAnimatorState(animator, animatorLayerIndex, swingStateName)) {
                yield return null;
            }

            // In the swing state, check for a collision between this hitbox and any hurtbox
            // Get reference to all hitboxes on source
            var hitboxes = payload.ActorAbilitySystem.GetComponentsInChildren<HitboxMonoComponent>(false);
            ActorAbilitySystem hitTarget = null;



            // Once we're about 35% through the animation, allow hit to trigger
            while (GetAnimatorStateInfo(animator, animatorLayerIndex, swingStateName).normalizedTime < 0.35f) {
                yield return null;
            }

            void HitTriggered(object sender, ColliderEventArgs e) {
                hitTarget = e.other.gameObject.GetComponent<HurtboxMonoComponent>().ActorAbilitySystem;
            }

            // Subscribe each hitbox on actor to the HitTriggered method

            for (int i = 0; i < hitboxes.Length; i++) {
                hitboxes[i].TriggerEnterEvent += HitTriggered;
            }

            // Wait for hitTriggered to become true, or animation to complete
            while (IsInOrEnteringAnimatorState(animator, animatorLayerIndex, swingStateName) && hitTarget == null) {
                yield return new WaitForFixedUpdate();
            }

            // If we get to here trigger return to idle state
            if (hitTarget != null) {
                CreateTargetAttributeModifiers(entityManager, hitTarget.AbilityOwnerEntity);
                animator.SetTrigger("ReturnWeaponToIdle");
                // Once we are no longer in the swing animation, commit the ability
                CreateCooldownEntities(entityManager, actorAbilitySystem.AbilityOwnerEntity);
                Entity cooldownEntity2 = new AttackCombo1GameplayEffectComponent().Instantiate(entityManager, actorAbilitySystem.AbilityOwnerEntity, 1.5f);

            }
            // Get target entity
            while (IsInOrEnteringAnimatorState(animator, animatorLayerIndex, swingStateName)) {
                yield return new WaitForFixedUpdate();
            }

            animator.ResetTrigger("ReturnWeaponToIdle");
            EndActivateAbility(entityManager, grantedAbilityEntity);

            // If we didn't anything, create cooldown entities
            if (hitTarget == null) {
                CreateCooldownEntities(entityManager, actorAbilitySystem.AbilityOwnerEntity);
            }

            yield return null;
        }

        AnimatorStateInfo GetAnimatorStateInfo(Animator animator, int layerIndex, string animatorStateName) {
            var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var animatorStateInfoNext = animator.GetNextAnimatorStateInfo(layerIndex);
            if (animatorStateInfoNext.IsName(animatorStateName)) animatorStateInfo = animatorStateInfoNext;
            return animatorStateInfo;
        }

        bool IsInOrEnteringAnimatorState(Animator animator, int layerIndex, string animatorStateName) {
            return GetAnimatorStateInfo(animator, layerIndex, animatorStateName).IsName(animatorStateName);
        }

    }

}