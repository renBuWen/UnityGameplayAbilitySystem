using System.Collections.Generic;
using GameplayAbilitySystem.Events;
using GameplayAbilitySystem.Interfaces;
using GameplayAbilitySystem.GameplayEffects;
using UnityEngine;
using GameplayAbilitySystem.Abilities.AbilityActivations;
using System.Linq;

namespace GameplayAbilitySystem.Abilities {
    /// <inheritdoc />
    [AddComponentMenu("Ability System/Ability")]
    [CreateAssetMenu(fileName = "Ability", menuName = "Ability System/Ability")]
    public class GameplayAbility : ScriptableObject, IGameplayAbility {

        [SerializeField]
        private GameplayAbilityTags _tags = new GameplayAbilityTags();

        [SerializeField]
        private GameplayCost _gameplayCost = new GameplayCost();

        [SerializeField]
        private List<GameplayEffect> _cooldownsToApply = new List<GameplayEffect>();

        [SerializeField]
        private List<GameplayEffect> _effectsToApplyOnExecution = new List<GameplayEffect>();


        [SerializeField]
        private GenericAbilityEvent _onGameplayAbilityCommitted = new GenericAbilityEvent();


        [SerializeField]
        private GenericAbilityEvent _onGameplayAbilityCancelled = new GenericAbilityEvent();


        [SerializeField]
        private GenericAbilityEvent _onGameplayAbilityEnded = new GenericAbilityEvent();

        [SerializeField]
        private AbstractAbilityActivation _abilityLogic = null;



        /// <inheritdoc />
        public IAbilityTags Tags => _tags;
        /// <inheritdoc />
        public IGameplayCost GameplayCost => _gameplayCost;
        /// <inheritdoc />
        public List<GameplayEffect> CooldownsToApply => _cooldownsToApply;
        public List<GameplayEffect> EffectsToApplyOnExecution => _effectsToApplyOnExecution;
        /// <inheritdoc />
        public GenericAbilityEvent OnGameplayAbilityCommitted => _onGameplayAbilityCommitted;
        /// <inheritdoc />
        public GenericAbilityEvent OnGameplayAbilityCancelled => _onGameplayAbilityCancelled;
        /// <inheritdoc />
        public GenericAbilityEvent OnGameplayAbilityEnded => _onGameplayAbilityEnded;
        /// <inheritdoc />
        public AbstractAbilityActivation AbilityLogic => _abilityLogic;



        void Start() {
        }



        protected void ApplyGameplayEffectToTarget(GameplayEffect effect, AbilitySystemComponent target) {
            //
        }

        /// <inheritdoc />
        public virtual void ActivateAbility(IGameplayAbilitySystem AbilitySystem) {
            _abilityLogic.ActivateAbility(AbilitySystem, this);
            ApplyCooldown(AbilitySystem);
        }

        /// <inheritdoc />
        public virtual bool IsAbilityActivatable(IGameplayAbilitySystem AbilitySystem) {
            return PlayerHasResourceToCast(AbilitySystem) && AbilityOffCooldown(AbilitySystem);
        }

        /// <summary>
        /// Applies the ability cost, decreasing the specified cost resource from the player.
        /// If player doesn't have the required resource, the resource goes to negative (or clamps to 0)
        /// </summary>
        protected void ApplyCost(IGameplayAbilitySystem AbilitySystem) {
            var modifiers = this.GameplayCost.CostGameplayEffect.CalculateModifierEffect();
            var attributeModification = this.GameplayCost.CostGameplayEffect.CalculateAttributeModification(AbilitySystem, modifiers);
            this.GameplayCost.CostGameplayEffect.ApplyInstantEffect(AbilitySystem);
        }


        /// <summary>
        /// Applies cooldown.  Cooldown is applied even if the  ability is already
        /// on cooldown
        /// </summary>
        protected void ApplyCooldown(IGameplayAbilitySystem abilitySystem) {
            foreach (var cooldownEffect in this.CooldownsToApply) {
                abilitySystem.ApplyGameEffectToTarget(cooldownEffect, abilitySystem);
            }
        }

        /// <inheritdoc />
        public void EndAbility(IGameplayAbilitySystem AbilitySystem) {
            _onGameplayAbilityEnded.Invoke(this);

            // Ability finished.  Remove all listeners.
            _onGameplayAbilityEnded.RemoveAllListeners();

            // TODO: Remove tags added by this ability

            // TODO: Cancel all tasks?

            // TODO: Remove gameplay cues

            // TODO: Cancel ability

            // TODO: Remove blocking/cancelling Gameplay Tags

            // Tell ability system ability has ended
            AbilitySystem.NotifyAbilityEnded(this);
        }


        /// <inheritdoc />
        public bool PlayerHasResourceToCast(IGameplayAbilitySystem AbilitySystem) {
            // Check the modifiers on the ability cost GameEffect
            var modifiers = this.GameplayCost.CostGameplayEffect.CalculateModifierEffect();
            var attributeModification = this.GameplayCost.CostGameplayEffect.CalculateAttributeModification(AbilitySystem, modifiers, operateOnCurrentValue: true);

            foreach (var attribute in attributeModification) {
                if (attribute.Value.NewAttribueValue < 0) return false;
            }
            return true;
        }

        /// <inheritdoc />
        public bool CommitAbility(IGameplayAbilitySystem AbilitySystem) {
            ActivateAbility(AbilitySystem);
            AbilitySystem.OnGameplayAbilityActivated.Invoke(this);
            ApplyCost(AbilitySystem);
            return true;
        }

        /// <inheritdoc />
        public bool AbilityOffCooldown(IGameplayAbilitySystem AbilitySystem) {
            (var elapsed, var total) = this.CalculateCooldown(AbilitySystem);
            return total == 0f;
        }

        /// <inheritdoc />
        public List<GameplayTag> GetAbilityCooldownTags() {
            return this._tags.CooldownTags.Added;
        }

        public (float CooldownElapsed, float CooldownTotal) CalculateCooldown(IGameplayAbilitySystem AbilitySystem) {
            var cooldownTags = this.GetAbilityCooldownTags();

            // Iterate through all gameplay effects on the ability system and find all effects which grant these cooldown tags
            var dominantCooldownEffect = AbilitySystem.ActiveGameplayEffectsContainer
                                    .ActiveEffectAttributeAggregator
                                    .GetActiveEffects()
                                    .Where(x => x.Effect.GetGrantedTags().Intersect(cooldownTags).Any())
                                    .DefaultIfEmpty()
                                    .OrderByDescending(x => x?.CooldownTimeRemaining)
                                    .FirstOrDefault();

            if (dominantCooldownEffect == null) {
                return (0f, 0f);
            }

            return (dominantCooldownEffect.CooldownTimeElapsed, dominantCooldownEffect.CooldownTimeTotal);
        }

    }
}
