# ADR-0001: Unity 6, URP, GameObject PhysX

- Status: Accepted
- Date: 2026-07-16

## Context

1인 개발, 기존 Unity 경험, 다수 상호작용 물체, 제한된 사전 파손 구조가 필요하다.

## Decision

Unity 6, URP, GameObject 기반 Built-in 3D PhysX를 사용한다. DOTS/ECS와 HDRP는 최초 버티컬 슬라이스 범위에서 제외한다.

## Consequences

- 빠른 C# 반복과 기존 경험 활용
- 완전 파괴보다 authored breakable 중심
- Rigidbody·Collider·Pooling 최적화 필요
- 정확한 Unity 패치와 패키지는 Bootstrap ADR에서 추가 잠금
