# Implementation Plan — Final Boss Evil Inara

## Overview

Final Boss pada game **The Day After** akan dibuat sebagai **playable boss encounter** dengan Evil Inara sebagai boss raksasa yang diam di tengah arena side-scroller. Pemain bergerak secara horizontal, menggunakan cover untuk bertahan dari serangan, menunggu boss memasuki kondisi **Exhausted / Vulnerable**, kemudian menggunakan Lentera untuk memurnikannya.

Konsep utama yang dipertahankan:

- Evil Inara adalah representasi sisi gelap/emosi negatif Inara.
- Boss berukuran besar dan relatif stationary di tengah arena.
- Pemain tidak dapat memberikan damage secara frontal.
- Pemain harus bertahan, menggunakan cover, lalu menyerang ketika boss kelelahan.
- Boss memiliki 6 HP sebagai progression encounter.
- Setiap penurunan HP dapat meningkatkan tekanan dengan memunculkan minion dari chapter sebelumnya.
- Final phase menggunakan seluruh jenis minion sebagai klimaks.
- Setelah boss dikalahkan, Evil Inara berubah menjadi Inara kecil dan encounter bertransisi menuju resolusi cerita.

Implementasi harus memanfaatkan sistem existing sebanyak mungkin, terutama:

- `PlayerMovement`
- `PlayerCrouch`
- `PlayerLanternAttack`
- `PlayerCloakOfInvisibility`
- `SanityController`
- `PlayerLife`
- `PlayerDeathHandler`
- Enemy system existing untuk minion
- `MemoryInteractable`
- `DialogueManager`
- `StoryManager`
- `InventoryManager`
- `TokenManager`

Evil Inara **tidak disarankan dimasukkan ke `EnemyArchetype` atau dipaksa menggunakan `EnemyBehaviourController`** karena lifecycle dan behavior boss berbeda jauh dengan enemy normal.

---

# Target Architecture

Struktur folder yang direncanakan:

```text
Assets/
└── Game Folders/
    └── Scripts/
        └── Gameplay/
            └── Boss/
                └── EvilInara/
                    ├── EvilInaraBoss.cs
                    ├── EvilInaraHealth.cs
                    ├── EvilInaraStateController.cs
                    ├── EvilInaraAttackController.cs
                    ├── EvilInaraAttack.cs
                    ├── EvilInaraDamageReceiver.cs
                    ├── EvilInaraPhaseController.cs
                    ├── EvilInaraMinionSpawner.cs
                    └── EvilInaraArena.cs
```

Tidak semua script harus dibuat sekaligus. Script dibuat hanya ketika benar-benar dibutuhkan oleh milestone yang sedang dikerjakan.

---

# Milestone 0 — Boss Arena & Test Scene

## Tujuan

Membuat arena greybox sederhana sebagai tempat pengujian seluruh boss mechanic.

## Struktur awal

```text
BossTestArena
├── Ground
├── LeftBoundary
├── RightBoundary
├── Cover_A
├── Cover_B
├── Cover_C
├── Player
├── EvilInara
└── MainCamera
```

Arena harus mendukung:

- Run kiri/kanan
- Jump
- Crouch
- Cloak
- Lantern Attack

Evil Inara cukup berada di tengah arena dan belum memiliki behavior.

## Fokus

Jangan langsung menggunakan environment final. Gunakan bentuk sederhana agar gameplay dapat diuji dengan cepat.

## Definition of Done

- Player dapat bergerak normal di arena.
- Player dapat menggunakan seluruh kemampuan existing.
- Evil Inara dapat ditempatkan di tengah arena.
- Kamera dapat melihat player dan boss dengan jelas.
- Posisi cover dapat diuji.

---

# Milestone 1 — Evil Inara Boss Core

## Tujuan

Membuat Evil Inara menjadi entity boss yang memiliki lifecycle dan HP sendiri.

## Script

```text
EvilInaraBoss.cs
EvilInaraHealth.cs
EvilInaraStateController.cs
EvilInaraDamageReceiver.cs
```

## State awal

```text
Intro
Idle
Vulnerable
Dead
```

Untuk tahap ini belum perlu membuat seluruh phase dan attack.

## HP

```text
Max HP = 6
Current HP = 6
```

## Damage Flow

```text
PlayerLanternAttack
        ↓
EvilInaraDamageReceiver
        ↓
EvilInaraBoss
        ↓
EvilInaraHealth
        ↓
Current HP--
```

Boss hanya menerima damage ketika state yang sesuai mengizinkan.

## Definition of Done

Prototype sederhana sudah dapat:

```text
F
↓
HP 6 → 5
↓
F
↓
HP 5 → 4
...
```

Boss belum perlu menyerang player.

---

# Milestone 2 — Boss State Machine

## Tujuan

Membuat lifecycle boss yang menjadi fondasi seluruh encounter.

## Target State

```text
Intro
Idle
Telegraph
Attacking
Recovery
Exhausted
Vulnerable
PhaseTransition
Dead
```

## Flow utama

```text
Idle
 ↓
Telegraph
 ↓
Attacking
 ↓
Recovery
 ↓
Exhausted
 ↓
Vulnerable
 ↓
PhaseTransition
 ↓
Idle
```

## Aturan Damage

Boss hanya boleh terkena Lentera ketika:

```text
Vulnerable
```

State lain harus menolak damage.

## Definition of Done

- State berubah dengan benar.
- Tidak ada damage ketika boss tidak vulnerable.
- State dapat di-debug dengan jelas melalui log/inspector.
- State machine tidak bercampur dengan movement atau attack logic.

---

# Milestone 3 — Boss Attack System

## Tujuan

Membuat Evil Inara benar-benar menyerang player.

Untuk playable prototype cukup tiga attack pattern.

## Attack A — Ground Slam

```text
Raise Hand
 ↓
Telegraph
 ↓
Slam
 ↓
Ground Hazard / Shockwave
```

Player harus belajar membaca telegraph dan menghindar.

## Attack B — Dark Projectile

```text
Boss
 ↓
Spawn Projectile
 ↓
Projectile bergerak ke arah player
```

Digunakan untuk menguji positioning dan penggunaan cover.

## Attack C — Horizontal Sweep

```text
Boss prepares
 ↓
Large horizontal attack
 ↓
Player menghindar / crouch / cover
```

Attack ini dapat memanfaatkan kemampuan crouch dan positioning side-scroller.

## Architecture

```text
EvilInaraBoss
      ↓
EvilInaraAttackController
      ↓
EvilInaraAttack
```

`EvilInaraBoss` tidak menampung semua logic attack.

## Definition of Done

- Boss dapat memilih attack.
- Setiap attack memiliki telegraph.
- Attack mempunyai durasi dan recovery.
- Attack dapat memberikan damage kepada player.
- Attack tidak berjalan bersamaan secara tidak terkontrol.

---

# Milestone 4 — Cover & Survival System

## Tujuan

Mengimplementasikan mekanik Cover & Strike yang menjadi inti boss fight.

GDD mengharuskan player berlindung di balik dinding/pilar ketika Evil Inara menyerang. Beberapa cover juga dapat dihancurkan.

## Cover

Setiap cover minimal memiliki:

```text
Health
IsDestroyed
Respawn
```

Flow:

```text
Boss Attack
 ↓
Cover terkena serangan
 ↓
Cover Damage
 ↓
Cover Destroyed
 ↓
Player mencari cover lain
```

## Respawn

Prototype:

```text
Destroyed
 ↓
Wait
 ↓
Respawn
```

Tidak perlu animasi kompleks dahulu.

## Definition of Done

- Player dapat berlindung di cover.
- Attack boss dapat mengenai cover.
- Cover dapat hancur.
- Cover dapat respawn.
- Arena tidak dapat menjadi permanently unwinnable hanya karena seluruh cover hancur.

---

# Milestone 5 — Exhausted & Vulnerable Window

## Tujuan

Membuat gameplay inti boss menjadi benar-benar playable.

Ini adalah milestone paling penting.

## Flow

```text
Boss
 ↓
Attack
 ↓
Attack
 ↓
Attack
 ↓
Exhausted
 ↓
Vulnerable
 ↓
Player keluar dari cover
 ↓
Mendekati boss
 ↓
F
 ↓
Boss HP -1
 ↓
Boss kembali menyerang
```

## Rules

Ketika `Exhausted`:

- Boss berhenti menyerang.
- Boss membuka vulnerability window.
- Player dapat memberikan damage.
- Setelah hit berhasil, boss kembali ke combat cycle.

## Definition of Done

Siklus berikut sudah terasa playable:

```text
ATTACK
 ↓
SURVIVE
 ↓
EXHAUSTED
 ↓
ATTACK BOSS
 ↓
HP -1
 ↓
REPEAT
```

Jika gameplay belum terasa menyenangkan pada tahap ini, jangan lanjut ke sistem minion.

---

# Milestone 6 — Boss Damage terhadap Player

## Tujuan

Menghubungkan attack boss dengan sistem player yang sudah ada.

Existing system yang dapat digunakan:

```text
PlayerDeathHandler
PlayerLife
SanityController
PlayerRespawn
```

Player saat ini sudah memiliki sistem death, attempt, respawn, dan Game Over.

## Rekomendasi prototype

Bedakan damage boss dan damage minion.

```text
Boss direct hit
→ kehilangan 1 Chance / Attempt

Minion touch
→ damage biasa / Sanity damage
```

Tujuannya agar player tidak mati hanya karena satu minion kebetulan menyentuhnya.

## Definition of Done

- Boss attack dapat mengenai player.
- Player dapat kehilangan chance/attempt.
- Player dapat respawn dengan benar.
- Game Over tetap menggunakan sistem existing.
- Tidak ada conflict dengan death animation existing.

---

# Milestone 7 — Boss Phase Controller

## Tujuan

Menghubungkan HP boss dengan escalation encounter.

Script:

```text
EvilInaraPhaseController.cs
```

## Progression

```text
HP 6
 ↓
Phase 1
Boss only

HP 5
 ↓
Phase 2
Boss + Red

HP 4
 ↓
Phase 3
Boss + Blue

HP 3
 ↓
Phase 4
Boss + Cyan

HP 2
 ↓
Phase 5
Boss + Purple

HP 1
 ↓
Final Phase
All enemy types
```

HP digunakan sebagai progression trigger, bukan berarti setiap HP harus mempunyai attack pattern baru.

Contoh:

```text
Phase 1
Attack Set A

Phase 2
Attack Set A + Red

Phase 3
Attack Set A + Red + Blue
```

Dengan struktur ini attack system dapat berkembang tanpa membuat phase controller terlalu kompleks.

---

# Milestone 8 — Minion Integration

## Tujuan

Memanfaatkan enemy system existing untuk minion.

Minion tetap menggunakan:

```text
EnemyController
EnemyBehaviourController
EnemyHealth
EnemyLanternTarget
EnemyPurification
EnemyReward
```

Boss tidak mengambil alih AI minion.

Boss hanya memberi instruksi:

```text
Spawn Red
Spawn Blue
Spawn Cyan
Spawn Purple
Spawn All
```

## Script

```text
EvilInaraMinionSpawner.cs
```

## Prinsip

```text
EvilInara
   ↓
MinionSpawner
   ↓
Instantiate / Activate Minion
   ↓
Existing Enemy AI bekerja sendiri
```

## Definition of Done

- Red dapat di-spawn saat HP 5.
- Blue dapat di-spawn saat HP 4.
- Cyan dapat di-spawn saat HP 3.
- Purple dapat di-spawn saat HP 2.
- Semua jenis dapat di-spawn pada final phase.
- Minion tidak mengubah state machine boss secara langsung.

---

# Milestone 9 — Minion sebagai Resource Recovery

## Tujuan

Membuat minion menjadi resource untuk survival.

Existing `EnemyReward` sudah memiliki hubungan dengan:

```text
SanityController
TokenManager
```

Untuk boss fight kita dapat memanfaatkan konsep tersebut sebagai recovery.

## Contoh

```text
Purify Red
→ Recover Sanity

Purify Blue
→ Recover more Sanity
```

Dengan begitu:

```text
Boss Attack
 ↓
Player loses resource
 ↓
Minions Spawn
 ↓
Player Purifies Minions
 ↓
Player Recover
 ↓
Player survives
```

## Design Goal

Minion bukan sekadar gangguan, tetapi bagian dari resource loop boss fight.

---

# Milestone 10 — Cloak Integration

## Tujuan

Memastikan Cloak of Invisibility memiliki fungsi nyata dalam boss encounter.

Existing cloak sudah memiliki:

```text
Activate
Duration
Cooldown
Deactivate
ForceDeactivate
```

## Final Phase

Cloak dapat menjadi alat survival:

```text
All Minions Active
 ↓
Player uses Cloak
 ↓
Avoid / reposition
 ↓
Boss becomes Exhausted
 ↓
Player disables Cloak
 ↓
Lantern Attack
```

`PlayerLanternAttack` sudah dapat memaksa cloak nonaktif ketika menyerang sehingga interaction antara kedua sistem dapat digunakan tanpa membuat senjata baru.

## Definition of Done

- Cloak dapat dipakai saat boss fight.
- Cloak tidak membuat boss fight otomatis trivial.
- Pemakaian cloak membutuhkan keputusan timing.
- Lantern tetap mematikan cloak ketika player menyerang.

---

# Milestone 11 — Final Phase

## Tujuan

Membuat phase terakhir sebagai climax encounter.

Trigger:

```text
Boss HP = 1
```

Kemudian:

```text
Boss
+ Red
+ Blue
+ Cyan
+ Purple
```

Untuk prototype jangan langsung melakukan unlimited spawn.

Gunakan jumlah terkontrol, misalnya:

```text
Red × 2
Blue × 1
Cyan × 1
Purple × 1
```

Boss tetap menjalankan:

```text
Attack
 ↓
Exhausted
 ↓
Vulnerable
```

Player harus bertahan sampai mendapatkan opening terakhir.

## Final Blow

```text
F
 ↓
Boss HP 0
```

---

# Milestone 12 — Boss Death & Purification

## Tujuan

Mengakhiri encounter secara khusus, bukan seperti enemy biasa.

Flow:

```text
Boss HP 0
 ↓
Stop Attacking
 ↓
Stop Minion Spawn
 ↓
Disable Damage
 ↓
Death / Purification Animation
 ↓
Light / VFX
 ↓
Evil Inara → Inara Kecil
```

Jangan langsung `Destroy(gameObject)` sebelum animasi selesai.

Boss harus menyelesaikan sequence terlebih dahulu.

## Story transition

Setelah purification:

```text
Boss Death
 ↓
Story Event
 ↓
Scene 20 — Penerimaan
```

---

# Milestone 13 — Story Integration

## Tujuan

Menghubungkan gameplay boss dengan cerita final chapter.

Existing `MemoryInteractable` sudah dapat menjadi integration layer antara object memory, dialogue dan `StoryManager`.

## Flow

```text
Album Foto
 ↓
MemoryInteractable
 ↓
Dialogue / Cutscene
 ↓
Boss Intro
 ↓
Boss Battle
 ↓
Boss Death
 ↓
Story Flag
 ↓
Scene Penerimaan
```

Story completion harus menggunakan `StoryManager` sebagai source of truth, bukan membuat state story baru di boss.

## Definition of Done

- Album foto memicu boss sequence.
- Boss hanya muncul setelah trigger yang benar.
- Setelah boss mati, story flag dapat di-set.
- Ending sequence dapat berjalan.
- Tidak terjadi duplicate trigger ketika scene/state berubah.

---

# Milestone 14 — Arena Transformation

## Tujuan

Merealisasikan perubahan visual dari calm memory room menjadi combat arena.

## Flow

```text
Calm Memory Room
        ↓
Album Photo
        ↓
Boss Appears
        ↓
Arena Break
        ↓
Ruins
        ↓
Dark Sky / Storm
        ↓
Boss Battle
```

Initial arena:

```text
White / Ivory
Calm
Water
Memory Objects
No Monsters
```

Combat arena:

```text
Ruins
Dark Sky
Storm
Cover
Boss
Minions
```

Untuk prototype, transformasi dapat dibuat menggunakan:

```text
SetActive
Animator
SpriteRenderer
ParticleSystem
Camera Shake
```

Tanpa harus langsung membuat VFX final.

---

# Milestone 15 — Balancing & Playtesting

## Tujuan

Menentukan angka final berdasarkan hasil permainan nyata.

Parameter yang perlu diuji:

```text
Boss HP
Attack Cooldown
Attack Telegraph Duration
Attack Duration
Recovery Duration
Exhaust Duration
Vulnerable Duration
Boss Damage
Cover HP
Cover Respawn Time
Minion Count
Minion Spawn Cooldown
Cloak Cooldown
Lantern Cooldown
Sanity Recovery
```

## Prinsip balancing

Kita perlu membedakan:

```text
Mati karena salah mengambil keputusan
```

dengan:

```text
Mati karena sistem terlalu sulit / tidak jelas
```

Fase HP 1 harus menjadi climax, bukan frustration spike.

## Test target

Lakukan testing berulang dan periksa:

- Apakah telegraph mudah dibaca?
- Apakah player punya waktu cukup untuk mencari cover?
- Apakah cover terlalu kuat atau terlalu mudah hancur?
- Apakah player masih memiliki opsi saat semua minion muncul?
- Apakah cloak terlalu kuat?
- Apakah recovery dari minion cukup?
- Apakah boss terasa terlalu lama karena 6 HP?
- Apakah vulnerable window terasa rewarding?

---

# Milestone 16 — Final Polish

## Tujuan

Mengubah playable prototype menjadi final presentation.

Tambahkan:

```text
Boss Animation
Boss VFX
Boss Crack Effect
Hit Flash
Lantern Hit VFX
Particle Effects
Screen Shake
Lightning
Storm
Boss Music
Attack Audio
Hit Audio
Phase Transition Audio
Boss HP UI
Intro Cinematic
Death Cinematic
Arena Transition
```

Visual Evil Inara:

```text
Giant Inara
Black Cracks
Red Fragments — Hatred
Blue Fragments — Regret
Cyan Fragments — Loneliness
Purple Fragments — Fear
Dark Blindfold / Eye Symbolism
```

---

# Milestone Dependency

```text
M0 Arena
 ↓
M1 Boss Core
 ↓
M2 State Machine
 ↓
M3 Attack
 ↓
M4 Cover
 ↓
M5 Exhausted / Vulnerable
 ↓
M6 Player Damage
 ↓
M7 Phase Controller
 ↓
M8 Minion
 ↓
M9 Recovery
 ↓
M10 Cloak
 ↓
M11 Final Phase
 ↓
M12 Boss Death
 ↓
M13 Story
 ↓
M14 Arena Transformation
 ↓
M15 Balancing
 ↓
M16 Polish
```

---

# Playable Prototype Definition

Playable prototype dianggap berhasil ketika setidaknya milestone berikut telah selesai:

```text
M0
M1
M2
M3
M4
M5
M6
M7
M8
```

Pada titik ini encounter sudah dapat dimainkan dari awal sampai boss dikalahkan.

Core gameplay:

```text
                EVIL INARA
                    │
                ATTACK
                    ↓
                PLAYER
                    ↓
                  COVER
                    ↓
           Survive Boss Attack
                    ↓
               EXHAUSTED
                    ↓
              BOSS VULNERABLE
                    ↓
                 LANTERN
                    ↓
               BOSS -1 HP
                    ↓
             PHASE TRANSITION
                    ↓
              SPAWN MINIONS
                    ↓
                  LOOP
```

Setelah itu:

```text
M9-M12
```

melengkapi gameplay encounter sampai final phase dan boss death.

Kemudian:

```text
M13-M16
```

menyelesaikan integrasi story, transformation, balancing, audio, VFX, dan presentation.

---

# Prinsip Architecture yang Harus Dipertahankan

## 1. Evil Inara bukan Enemy Archetype baru

Jangan membuat:

```text
EnemyArchetype.EvilInara
```

dan memasukkannya ke semua logic enemy biasa.

Boss memiliki lifecycle sendiri.

## 2. Boss tidak mengambil alih AI minion

Boss hanya memanggil:

```text
EvilInaraMinionSpawner
```

Minion tetap menggunakan AI existing.

## 3. Player system tidak dibuat ulang

Gunakan system existing:

```text
PlayerMovement
PlayerCrouch
PlayerLanternAttack
PlayerCloakOfInvisibility
SanityController
PlayerDeathHandler
PlayerLife
```

## 4. Story state tetap melalui StoryManager

Boss tidak menjadi source of truth untuk story.

Gunakan:

```text
StoryManager.SetFlag(...)
```

## 5. Jangan membuat semua script di awal

Bangun sistem sesuai kebutuhan milestone.

Semakin sedikit logic yang dibuat sebelum gameplay terbukti, semakin mudah debugging dan maintenance.

---

# Final Recommended Development Strategy

Prioritas utama adalah membuktikan bahwa loop berikut menyenangkan:

```text
Boss Attack
 ↓
Player Reads Telegraph
 ↓
Player Finds Cover
 ↓
Cover Survives / Breaks
 ↓
Boss Exhausted
 ↓
Player Takes Risk
 ↓
Lantern Hit
 ↓
Boss HP -1
```

Jika loop tersebut sudah terasa bagus, barulah tambahkan:

```text
Minions
 ↓
Resource Recovery
 ↓
Cloak
 ↓
Final Phase
```

Kemudian baru:

```text
Story
 ↓
Transformation
 ↓
VFX
 ↓
Audio
 ↓
Polish
```

Target akhir bukan sekadar boss yang "bisa dikalahkan", tetapi boss yang terasa sebagai **ujian terakhir terhadap seluruh mekanik yang sudah dipelajari pemain sepanjang The Day After**.
