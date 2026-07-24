# Save and Versioning

> **OWNER-APPROVED G3 RESET BOUNDARY — PERSISTENT SAVE DEFERRED**
>
> Authority: `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
> `## Reset, Save, and Versioning`

## Prototype Reset

authored baseline snapshot/restore가 primary Reset path다. complete Scene
reload는 restore 누락이나 unsafe state를 위한 audited fallback이다.

반복 Reset마다 다음을 확인한다.

- authored baseline과의 state diff;
- subscription count와 duplicate handler;
- active ownership/reservation;
- fault와 quarantine residue;
- later resource가 활성화된 경우 encounter-entry baseline.

## Reset Ownership Inventory

- CHR_00, CHR_01과 Compliance Runner 두 명;
- exact object pools;
- transform과 physical state;
- Carry와 Strap;
- Relay reservation/phase와 Ally state;
- Causal Origin/Result state;
- room-state flag;
- fault/quarantine state;
- subscription과 duplicate-handler state.

## Vertical Slice

### Persistent Save Boundary

Prototype에는 persistent gameplay save가 없고 full Rigidbody-world
persistence를 사용하지 않는다. local option default는 later 별도
승인할 수 있지만 gameplay Reset owner와 혼합하지 않는다.

Vertical Slice의 Run Seed, room index, upgrade ID 또는 accessibility
setting save는 후보일 뿐 승인되지 않았다.

## Versioning

- repository/design version은 Gate status가 아니다.
- persistent save가 later 승인될 때만 independent save-schema version을
  정의한다.
- final serialization format, arbitrary object reference 저장과
  versionless save는 승인되지 않았다.
- Canon change는 Change Control, CHANGELOG와 필요한 ADR을 따른다.
