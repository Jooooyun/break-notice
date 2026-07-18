# Production Image Prompt Pack

아래 프롬프트는 탐색용이 아니라 승인된 디자인을 유지하는 제작 시안용이다. 한 번에 한 장만 생성한다.

## 공통 Prefix

```text
PROJECT: BREAK NOTICE

Create one finalized production-target visual for an original solo-developed Unity 6 third-person physics-action roguelite set inside a modern corporate office.

This is not a style exploration and not a collection of alternatives. Produce one coherent, production-oriented visual that can serve as a strict reference for the actual game.

FINAL ART DIRECTION: Grounded Corporate Action Realism.
Use realistic adult anatomy, restrained expressions, practical workplace clothing, worn PBR materials, readable silhouettes, modular office architecture, controlled debris, and a limited number of visible NPCs.
The humor comes from serious ordinary employees and absurd but physically understandable office-object chain reactions.
There is no magic and no telekinesis. Every object is physically thrown, rolled, pushed, caught, or redirected through visible contact.

The dark red necktie is ordinary clothing only. It never functions as a rope, tether, cable, weapon, or object connector.
All object control uses a separate black-and-yellow industrial cargo strap from the waist-mounted reel and a visible silver carabiner.
```

## 공통 Negative

```text
No anime, manga, manhwa, cel shading, comic-book rendering, Pixar or Disney style.
No oversized eyes, exaggerated heroic jaw, bodybuilder anatomy, rubber poses, slapstick faces, toy plastic materials, superhero costume, military sci-fi armor, magic glow, telekinesis, energy aura, glowing trails, supernatural cable, necktie attached to furniture, firearms, giant explosion, excessive smoke, crowd, random equipment, Dutch angle, extreme depth of field, unreadable motion blur, text, logo, UI, split panel, or collage unless requested as a model sheet.
```

## GEAR_01 — Cargo Strapping System

Status: `LOCKED` as `references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png`. 아래 프롬프트는 승인된 Revision 작업이 아닌 경우 재사용하지 않는다.

```text
Use the approved CHR_00 player image as an exact continuity reference. Create a practical industrial equipment design sheet, not a character redesign.
Show the waist-mounted retractable reel, black-and-yellow woven cargo strap, silver carabiner, replaceable asset clamp, forearm brace tension lock, safety release, load warning labels, and how the system connects to a wheeled office-chair frame.
Include neutral stored state, attached state, tensioned redirect state, and automatic safety-release state.
The red necktie remains completely separate. No floating object and no rope-swing movement.
```

## CHR_01 — Ally Identity

Status: `LOCKED` as `references/approved/characters/CHR_01_First_Ally_Identity_v001_FINAL.png`. 아래 과거 프롬프트는 승인된 Revision 작업이 아닌 경우 재사용하지 않는다. Canonical Elements는 `ALLY_VISUAL_SPEC.md`와 ADR-0009를 따른다.

```text
Use the approved CHR_00 realism, material, and office-world quality. Create one full-body three-quarter design for the Facilities Mover ally on a neutral gray background.
The ally is a believable facilities/logistics employee in worn dark-blue workwear, employee badge, gloves, safety shoes, and a practical tool belt. No powered brace, no red necktie, no armor, no firearm.
Show a physically capable worker who can throw light office supplies and roll chairs or carts, but not a combat hero.
```

## PGT_01 — Gameplay Camera Lock

Status: `LOCKED` as `references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png`. 아래 과거 프롬프트는 승인된 Revision 작업이 아닌 경우 재사용하지 않는다. Canonical Elements는 `CAMERA_SPEC.md`와 ADR-0010을 따른다.

```text
Create an actual in-game 16:9 third-person gameplay view, not key art. Use the approved player and ally exactly.
Camera approximately 4.5 meters behind the player, 1.2 to 1.5 meters above head level, around 68-degree FOV, slightly elevated OTS, enough visible floor for object trajectories, no cinematic depth of field.
Show one modular open office room, one glass meeting room, low partitions, desks, chairs, one copier, one mobile whiteboard, one filing cabinet, and one conference table.
Characters: one player, one Facilities Mover at a safe relay position, three security enemies maximum.
Clearly show 2-3 Light objects, two Medium objects, one Heavy candidate, a breakable glass partition, collision surfaces, and multiple movement paths. No active destruction yet.
```

## PGT_02 — Chair Redirect

Status: `LOCKED` as `references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png`. 아래 과거 프롬프트는 승인된 Revision 작업이 아닌 경우 재사용하지 않는다. Canonical Elements는 `CHAIR_REDIRECT_VISUAL_SPEC.md`와 ADR-0011을 따른다.

```text
Edit the approved PGT_01 image while preserving the same room, camera, characters, scale, lighting, and layout.
The Facilities Mover physically pushes and releases a wheeled office chair from behind-left. The chair stays on the floor and rolls toward the player.
The player connects the separate black-and-yellow strap from the waist reel to a metal point on the chair with a visible silver carabiner, plants one foot, rotates hips and torso, braces with the forearm device, and redirects the moving chair toward one security enemy.
Read clearly as coworker push -> chair rolls -> mechanical connection -> player pivots -> chair changes direction. The red necktie is not connected.
```

## PGT_03 — Chain Aftermath

Status: `LOCKED` as `references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png`. 아래 과거 프롬프트는 승인된 Revision 작업이 아닌 경우 재사용하지 않는다. Canonical Elements는 `CHAIN_REACTION_AFTERMATH_VISUAL_SPEC.md`와 ADR-0012를 따른다.

```text
Edit the approved PGT_02 image, preserving camera and layout. Show the immediate causal result:
chair hits security employee -> employee breaks one glass panel -> frame or body strikes a mobile filing unit -> filing unit pushes the copier -> copier tips and releases localized paper and toner -> another enemy loses balance.
Use 8-15 large physical debris pieces maximum, one broken glass panel, one tipped copier, one displaced filing unit, particles for small paper and glass, no fireball, no ceiling collapse, no crowd, no room-wide smoke.
Make the newly opened route and changed room state readable like actual gameplay.
```

## LGT_01 — Night Archive

Status: `LOCKED` as `references/approved/lighting/LGT_01_Night_Archive_Lighting_v001_FINAL.png`. 아래 과거 프롬프트는 승인된 Revision 작업이 아닌 경우 재사용하지 않는다. Canonical Elements는 `NIGHT_ARCHIVE_LIGHTING_VISUAL_SPEC.md`와 ADR-0013을 따른다.

```text
Preserve approved gameplay camera and characters. Use the exploration night archive reference only for cold fluorescent lighting, narrow filing aisles, dark green-blue ambient tone, limited red emergency indicators, reflective floor highlights, and document-storage atmosphere.
Keep player, ally, enemies, strap, usable objects, and routes clearly readable. Not horror, not supernatural, not black-crushed, and not monochrome.
```

## FIN_01 — Table Finisher

Status: `NEXT / PENDING PREFLIGHT`. 이미지 생성 전에 별도 Canon preflight와 owner 승인이 필요하다. LGT_01의 정확한 방·선반·통로·배우·소품·조명은 incidental Non-Canon이며 그대로 고정하지 않는다. 아래 과거 프롬프트는 권한 있는 생성 브리프가 아니다.

```text
Create a rare scripted in-engine finisher using the approved player and two allies maximum. All three physically contact and push a heavy conference table on reinforced caster wheels toward two or three security enemies near a glass partition.
Show body weight, foot traction, wheel motion, limited glass breakage, and controlled debris. No remote control, telekinesis, necktie connection, superhero pose, giant explosion, or building collapse.
```
