# Save and Versioning

## Prototype

- 저장 없음
- Reset은 초기 스냅샷 또는 재생성으로 수행
- 옵션 설정은 로컬 기본값 사용 가능

## Vertical Slice

저장 후보:

- 입력/오디오/카메라 접근성 설정
- 현재 Run Seed와 방 인덱스(필요 시)
- 선택한 업그레이드 ID
- 빌드 버전과 Save Schema Version

## 금지

- 모든 Rigidbody 위치·속도 저장
- 임의 객체 참조 직렬화
- 버전 필드 없는 저장

## Versioning

- 문서 기준: SemVer-like `0.x.y`
- 저장 스키마는 독립 정수 버전
- Canon 변경은 CHANGELOG와 ADR에 기록
