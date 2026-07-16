# Incident Report Feature

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

버티컬 슬라이스 P2 기능이다. 핵심 루프와 성능이 검증되기 전 구현하지 않는다.
