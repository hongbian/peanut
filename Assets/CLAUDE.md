# CLAUDE.md — Project Handoff & Design Doc

## What this game is

A 3D atmospheric puzzle-adventure in Unity 6 (project name "CAM", working greybox phase). Inspired by **Little Nightmares** in camera language, movement feel, and level-monster-theme structure — but deliberately NOT its tone or content. This game is **bright and colorful**, and its "monsters" are not creatures of grime and hunger: each stage's antagonist is an **evil idea that kills the heart**, embodied as a character. Beauty is the warning sign in this world, not ugliness.

**Core concept:** The player character travels with a companion that is literally his **heart**. The character himself is expendable — he can fall, get hurt, take risks. But if the companion/heart is destroyed, the player dies. Protecting the heart IS the game.

**Stage 1 theme:** Self-appearance / vanity. The boss is "the Beauty" — a gorgeous figure (with a hidden face, never fully shown) living in a bright ornate house, who lures hearts with beauty and keeps a **collection of captured hearts** in cages. The message (vanity tears your heart apart) must stay **implicit** — shown through mechanics and environment, never stated. The game should feel like a game, not a lesson.

## The design rule that everything hangs on

**The heart is ATTRACTED to beautiful things.** Threats in this game don't chase — they **lure**. Mirrors, trinkets, portraits, and ultimately the Beauty herself pull the companion toward them. This makes the companion's vulnerability active, not passive, and makes the player's verbs (carry, sit/stay, rescue) meaningful choices rather than traversal buttons.

Design test for any new hazard/puzzle idea: *does it ever make the player hesitate before picking the heart up, or dread setting it down?* If carrying trivially solves it, redesign it. Prefer **environmental rules** (apply everywhere, create route/timing decisions) over one-off obstacles.

## Stage 1 arc (agreed structure)

1. **Act 1 — the house, no threat.** Wordless teaching of follow/carry/sit. First gentle lure (a trinket; heart drifts, player corrects it, no consequence). Companion charm/personality carries this act.
2. **Act 2 — consequences, source unknown.** Lures gain real cost (enchantment drain). Environmental storytelling: scratched-out portraits, vanity tables, ornate cages — one holds a dim faded heart. Distant sounds of the boss. Player learns WHAT happens here before WHO does it.
3. **Act 3 — the Beauty exists.** First sighting: **scripted climax where the heart is mesmerized and walks toward the Beauty, and is captured** — playable but unwinnable (player chases, a door closes / capture happens half-off-screen; player must feel "I couldn't save it," never "cutscene took control"). Boss face stays hidden via camera framing.
4. **Act 4 — alone, then rescue, then escape.** Player crosses the house without the heart (world feels dimmer/colder — the heart was the warmth). Rescue destination is the **collection room** seen in Act 2. Escape together back through patrolled territory = all mechanics stacked under pressure.

Boss behaves LN-style: an environment with behavior (predictable grooming/patrol routine = stealth windows), not a combat encounter. The boss senses **hearts** — the companion glows/hums louder when enchanted, so a lured heart literally calls the boss. Ending should turn the boss's nature against itself (reflection / hidden face / the collection — direction not yet chosen).

## Current state — what exists and works

Unity 6 (6000.0.43f1), Cinemachine 3 (CM3 component names!), greybox scene "CameraTest" plus main scene. Player character "finally" (Blender FBX, idle + walk animations, Animator with a `Speed` float parameter).

### Camera system (done, tuned, working)
- **CinemachineCamera** per room/zone. Position Control = **Spline Dolly** on a Spline (knots define the rail; camera slides to nearest point to target, damping ~1.5/2/1.5). Rotation Control = **Rotation Composer** tuned very subtle: Screen Position Y +0.15 (character in lower third, LN headroom), Dead Zone X 0.3 (horizontal framing is the DOLLY's job — composer must NOT pan sideways), Dead Zone Y 0.05 (composer owns only a whisper of vertical tilt), damping ~1/1, Lookahead OFF. Noise = Basic Multi Channel Perlin, Handheld_normal_mild, Amplitude 0.25.
- Lens FOV ~40 (or 35), camera pulled far back — far camera + narrow lens = the LN diorama look. Character ≈ 1/5–1/6 of screen height.
- **Camera zones:** one CinemachineCamera + rail per room; `CameraZone` trigger script at doorways enables the zone camera and disables the default; CinemachineBrain Default Blend = Ease In Out ~1.5s. Rails of adjacent rooms should overlap near doorways for seamless handoffs.
- Rails are a storytelling tool: frame the danger/lure so the player SEES it from the doorway; rails also control what angles of the boss's face are ever shown.

### Movement (done)
- `Player.cs`: world-relative movement (NOT camera/character-relative). Input = world vector (Horizontal→X, Vertical→Z), CharacterController.Move with gravity, cosmetic body rotation via Quaternion.Slerp (turnSpeed ~10 = smoothing rate), `animator.SetFloat("Speed", move.magnitude)`. ClampMagnitude on input so diagonals aren't faster.
- Player has: CharacterController (collision+gravity), kinematic Rigidbody (needed for triggers to fire), Tag = "Player".

### Companion systems (done through prototype level)
- Companion is currently a placeholder **sphere** (~0.4 scale, rests at Y matching floor top ≈ 0.7). DO NOT model it in Blender yet — mechanics decide the model (size, carry pose, locomotion), not vice versa. Same rule for environments: greybox in Unity first, Blender art pass only after camera-validated dimensions.
- `CompanionFollow.cs`: leash-follow with hysteresis (wakeDistance ~2.5 start, followDistance ~1.5 stop), SmoothDamp acceleration, speed slightly BELOW player speed (deliberate: player can outrun it → separation anxiety). Grounded on flat floor (Y handled by constants for now; raycast later).
- `CompanionCarry.cs` (on player): **E** toggles pick up / put down. Pickup parents companion to a `CarryAnchor` child (chest height), fully disables `CompanionFollow` (inert + collider off — carrying = shielded from lures and hazards). Put-down re-enables `CompanionFollow` and calls `SetSitting(true)`. Exposes `IsCarrying`.
- **F** toggles sit/stay: calls `CompanionFollow.SetSitting()`. Sitting suppresses the leash-chase toward the player, but does NOT disable the script — lures still act on a sitting companion (see below). This is deliberate: "stay" is a command to the companion, not immunity to being lured.
- `CompanionHealth.cs`: health (3), invulnerability window (1s), death → `PlayerDies()` → scene reload for now. `HazardZone.cs`: trigger using OnTriggerStay calls TakeDamage (red-cube placeholder hazards — being replaced by the lure paradigm).
- Damage feedback: material flash coroutine + Cinemachine Impulse (Impulse Listener extension on every CinemachineCamera, Impulse Source on companion, low amplitude ~0.5 — subtle, never earthquake).

### The lure mechanic (built, not yet feel-tuned)
- `HeartLure.cs` on "beautiful object" placeholders (gold cube, `GoldTrinketMat`): lureRadius 4 (drift starts), captureRadius 0.8 (heart stands mesmerized), lureStrength 1.2, pull scales 0→full from edge→center, `OnDrawGizmosSelected` draws both radii. Two test trinkets (`LureTrinket_A`, `LureTrinket_B`) live in the Room 1 greybox, replacing the old hazard-strip test there.
- `CompanionFollow` has `willpower` (0–1, currently 0.75) — follow-resistance to lures; scans `HeartLure` objects every `Update` via `FindObjectsByType` (fine for prototype scale), strongest pull wins. When sitting, the leash desire is zero, so a nearby lure pulls at effectively full strength — sitting is not safety. `combinedDesired = desiredVelocity * willpower + lurePull`; inside captureRadius → `IsMesmerized = true`, velocity zeroed, companion holds still until rescued.
- Breaking the spell: picking up (E) snaps it out (carry fully disables `CompanionFollow`; `OnEnable` resets `isFollowing`/`IsMesmerized`/velocity to fresh state next time it's re-enabled via put-down). E is the rescue verb — thematically intentional.
- Verified in Play mode: pull drift toward a trinket, capture-freeze at the boundary, pickup breaking the freeze, sitting-far-from-a-lure staying perfectly put, AND sitting-near-a-lure still drifting in and getting captured — all confirmed end-to-end. NOT yet play-tested by hand for feel (drift speed, correction window, whether 0.75 willpower is right).
- Open design questions still to play-test: right willpower value (raised once already, 0.6→0.75); whether the BOSS's pull should override even more decisively than a regular lure (not yet implemented — regular lures already override "sit"). `willpower`/radius/strength are the Act 1→3 difficulty knobs; the scripted capture is this mechanic at max intensity.
- Act 1 version has NO damage/drain yet — feel first (creepy, noticeable, correctable), consequences layered later (roadmap item 2).

### Companion gaze (built, cosmetic only)
- Two black sphere "eyes" (`Eye_L`/`Eye_R`, `EyeMat` unlit black) parented to the companion on its +Z face, comically oversized on purpose (local scale 0.35 against a 0.4-scale parent) so they read clearly at gameplay camera distance.
- `CompanionGaze.cs` on the companion picks a look target by priority each frame: **1)** `CompanionFollow.CurrentLure` if non-null (a lure currently pulling, or the one that captured it) — this is the early-warning channel, gaze locks on before meaningful drift; **2)** `CompanionFollow.CurrentVelocity` direction if moving; **3)** the player, otherwise (idle default, also used the whole time it's carried — simplest fallback, not important yet). Y-axis rotation only, `Quaternion.Slerp` at turnSpeed 3.5 (deliberately slower/dreamier than the player's ~10).
- `CompanionFollow` now exposes `CurrentLure` (the strongest-pulling or capturing `HeartLure`, null if none) and `CurrentVelocity` — added specifically so `CompanionGaze` reads state instead of duplicating the lure-scan.
- Verified in Play mode: idle gaze settles on the player (0° after 60 ticks); placed at the edge of a lure's radius, gaze snapped to within 0.28° of the lure after just 3 ticks while the companion had barely moved (0.024 units) — gaze genuinely leads the drift as intended.
- **Facing is cosmetic only right now — it does NOT affect lure strength.** Open design question for later: should facing be mechanical (a lure behind the heart pulls weaker; sitting facing away from a lure as a deliberate protective placement)? Not built, worth a future experiment.
- Future model requirement to remember: the eventual companion model needs a readable front (eyes = direction) and a glow/state channel (state = enchanted/mesmerized/normal) built in — no HUD is planned, so these cosmetic readability channels ARE the UI.

### Corruption (built) — replaces damage-drain for lure threats
- `CompanionCorruption.cs` on the companion: single `Corruption` float 0→1, no HUD — read entirely off the companion's own body.
- **Gain:** `gainRate` 0.04/sec, but ONLY while `CompanionFollow.IsMesmerized` — mere drifting/being pulled costs nothing, only an actual capture does. Tuned so one unnoticed capture + a realistic few-second rescue costs ~0.2–0.3 (verified: 300 simulated ticks of continuous mesmerize → 0.24).
- **Recovery ("closeness heals," built as default per spec):** ticks down at `recoveryRate` 0.01/sec (4x slower than gain — scars linger, verified exactly: same tick count that caused +0.24 gain caused -0.06 recovery) while carried, OR while within `CompanionFollow.FollowDistance` of the player. Carry state is checked first each frame, so a stale `IsMesmerized` flag from the instant before pickup can never cause a gain tick while shielded in arms.
- **Visual tells, all verified in Play mode:** body material color lerps from its original color toward `porcelainColor` (pale porcelain gray); `Eye_L`/`Eye_R` local scale lerps down to 35% of their base size (dulling); `CompanionGaze`'s effective turn speed lerps toward 0 (dreamier, less responsive, and at Corruption=1 it stops turning — and therefore stops looking at the player — entirely).
- **Mechanical effects, deliberately NOT touching `willpower`/lure math:** `CompanionFollow`'s leash speed scales down with corruption (lerp to 0 at full corruption — a fully corrupted heart stops responding to the leash entirely, verified via `CompanionFollow.Start()` caching a `CompanionCorruption` reference). `CompanionCarry.GetCarrySpeedMultiplier()` (lerp to 0.5 at full corruption) is read by `Player.cs` each frame to make carrying a corrupted heart heavier — Player now caches a `CompanionCarry` reference too.
- **At Corruption ≥ 1.0:** follow speed and gaze turn rate are already at zero via the lerps above (heart "stops responding entirely" falls out of the existing scaling, no extra special-casing needed), and `CompanionCorruption` calls `CompanionHealth.PlayerDies()` once (guarded by a `hasDied` flag) — `PlayerDies()` was changed from private to public specifically so both systems can trigger the same death/reload path instead of duplicating it. Verified end-to-end in Play mode.
- Open questions: **corruption→willpower downward spiral** (corrupted hearts resisting lures even less, compounding) — deliberately deferred, add only if corruption ends up lacking teeth once played by hand. **Permanence model may change after playtesting** — closeness-healing was built as the default per instruction, but full-reset-on-rescue or a rest-point/checkpoint model are both still on the table. **Mesmerized glow ramp (emission) — not built, needed before the boss prototype.** The boss section above says the companion "glows/hums louder when enchanted... a lured heart literally calls the boss" — that's the boss's detection input, and corruption's visual tells (desaturation, eye-dulling) cover the slow lingering damage but NOT this acute in-the-moment signal. Channel plan is still eyes = direction, glow = state; the glow half is the missing piece.

### HazardZone (kept, scoped down) — open question: fold into corruption later?
- `HazardZone.cs` / `Switch.cs` / `LevelGoal.cs` still exist and are used in a validation-prototype 3-room gauntlet (Room 1 hazard-strip-turned-lure-test → Room 2 rest/switch/gate → Room 3 dual-route) built to sanity-check carry/sit/stakes before lures existed. Rooms 2–3 still use plain damage-on-touch hazards; **that's intentional for now, not an oversight** — HazardZone is reserved for genuinely physical dangers (falls, crushers), not thematic/boss threats, which should be lures. Rooms 2–3 will be rebuilt as proper Act 1 house rooms once lure feel is tuned; don't extend this gauntlet further.
- `CompanionHealth` (3 HP, damage-based death) still exists in parallel with `CompanionCorruption` (0→1, capture-based death) — two separate death paths converging on the same `PlayerDies()`. Open question: should physical-hazard damage eventually feed into corruption too (one unified "condition" stat), or do they stay permanently separate (health = body, corruption = spirit)? Not decided — flagging now before it's forgotten.

## Roadmap (agreed order)

1. **Hand-playtest lure + corruption feel — current, and the real gate before anything else.** Everything since the lure landed (willpower 0.75, corruption gain/recovery rates, the 0.5x carry-heaviness penalty) is build-verified in Play mode but NOT yet felt by a human. See the playtest checklist below — don't start item 3 until this pass happens.
2. ~~Enchantment drain~~ — **DONE, replaced by the corruption system.** Act 2's "mesmerized heart suffers a real cost" is built (capture-based, not damage-based). The one piece of the original spec still missing is the "glows louder" half — see the mesmerized-glow open question in the Corruption section above.
3. Greybox Act 1 rooms of the house with rails + zones; corridor-of-mirrors as first real level-design test.
4. Companion charm pass (bob, trembles near lures, happy bob when picked up) — critical because Act 1's content IS the companion's personality.
5. Player-death (hazards that hurt the player, checkpoints) when needed.
6. Boss prototype: patrol routine + heart-sensing (needs the mesmerized glow built first — that's its detection input).
7. Only then: Blender art pass of proven rooms; companion modeled to the carry pose the prototype discovered.

## Playtest checklist (do before greyboxing Act 1 — item 1 above)

One honest ~15-minute hand-played session, not automated verification. Specifically feel out:
- Willpower 0.75 — does resisting a lure feel right, or does it drift too eagerly / too stubbornly?
- Corruption gain/recovery rates (0.04 gain, 0.01 recovery) — does one missed capture feel like a real cost? Does recovery feel like "scars linger" or just tedious?
- **After your first rescue, does the pale, slower heart make you want to hold it for a while?** — the visual tells should create a pull toward caring for it, not just inform you of a stat.
- **Does carrying a corrupted heart feel heavy, or just annoying?** These are different feelings, and the flat 0.5x speed multiplier could land as either — if it reads as "annoying," the penalty likely needs to be reshaped (e.g. non-linear curve, momentum/acceleration drag instead of a flat cap), not just retuned numerically.

## Hard-won gotchas (do not re-lose these)

- **Hierarchy vs Project references:** always assign SCENE instances (Hierarchy), never Project assets — a Tracking Target pointed at the prefab asset (frozen at origin) once froze the whole camera rig. Double-click any object field to verify what it points to.
- Blender FBX exports include Blender's camera/light: **Import Cameras / Import Lights are unchecked** on the character model — keep it that way (a hidden imported camera once hijacked rendering and rotated with the character).
- CM3 naming: "CinemachineCamera", Position/Rotation Control dropdowns, Cinemachine Follow / Spline Dolly / Rotation Composer components. No "Virtual Camera"/"Body"/"Aim" (that's CM2) — tutorials and docs must match CM3.
- Spline container Transform must stay at (0,0,0); knot coordinates are local.
- Cinemachine **Save During Play = ON** for tuning; noise profiles live in the package (search scope "All"/Packages in the picker).
- Triggers need a Rigidbody (kinematic) on the player/companion; CharacterController replaced the player's capsule collider.
- Keep sacred: composer never pans horizontally (dolly owns X), damping values are the LN feel — change deliberately, not casually.
- **`SceneManager.LoadScene` on the currently-open scene, called WHILE in Play mode (e.g. testing a death/reload trigger), can confuse the Editor's "revert to pre-Play state" safety net on Stop.** Twice now, a Transform value forced during that same test session (player position, once at Corruption=1 the eyes' scale) survived Stop and got written into the actual save instead of reverting. Material-instance color changes (`renderer.material.color`) are NOT affected — Unity discards those material instances on exiting Play regardless. After any Play-mode test that calls a death/reload path, double-check player/companion Transform values (position, scale) before saving — don't assume Stop cleaned up after itself.
