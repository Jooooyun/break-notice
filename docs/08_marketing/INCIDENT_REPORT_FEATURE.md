# Incident Report Feature

> **P2 / SHOULD / DEFERRED — NON-BLOCKING**
>
> PR-F-020은 현재 활성 Prototype 또는 Vertical Slice blocking scope가
> 아니며 Acceptance Test도 없다. 아래 흐름과 예시는
> `DRAFT FEATURE CANDIDATE`일 뿐 구현, 저장·공유, UI, replay 또는
> production content를 승인하지 않는다.

## 목적

플레이어가 만든 우연한 명장면을 짧은 공유 가능한 결과로 재구성한다.

## 흐름

1. 방에서 가장 깊은 Chain 또는 가장 큰 공간 변화를 선택
2. 3~5초 리플레이
3. 최초 원인, 손상 자산, 재배치된 직원, 열린 경로 요약
4. 저장·공유는 본 제작에서 검토

## 예시

```text
INCIDENT REPORT #00421
Initial Cause: Office Chair
Chain Depth: 4
Assets Damaged: 9
Employees Reassigned: 3
Route Opened: Meeting Room East
```

## 기술 범위

PR-F-020은 `P2 / SHOULD / DEFERRED`다. G1, Prototype Core와 현재
Vertical Slice의 비차단 범위이며, 별도 Owner 승격·Acceptance Test·
production scope 승인 전에는 구현하지 않는다.
