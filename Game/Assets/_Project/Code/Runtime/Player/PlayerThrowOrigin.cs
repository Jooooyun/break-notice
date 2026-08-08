namespace BreakNotice.Gameplay.Interaction
{
    public readonly struct PlayerThrowOrigin
    {
        public PlayerThrowOrigin(
            uint sequence,
            string contentDefinitionId,
            string runtimeInstanceKey,
            PrototypeWeightClass weightClass)
        {
            Sequence = sequence;
            ContentDefinitionId = contentDefinitionId;
            RuntimeInstanceKey = runtimeInstanceKey;
            WeightClass = weightClass;
        }

        public uint Sequence { get; }

        public string ContentDefinitionId { get; }

        public string RuntimeInstanceKey { get; }

        public PrototypeWeightClass WeightClass { get; }

        public bool IsPlayerOwned => Sequence != 0 && !string.IsNullOrWhiteSpace(RuntimeInstanceKey);
    }
}
