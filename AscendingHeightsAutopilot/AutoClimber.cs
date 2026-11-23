using Il2Cpp;
using UnityEngine;

namespace AutoAscendingHeights;

/// <summary>
/// Ascending Heights: you lose when you fall 50 units below the highest point reached
/// (AscendingHeightsController.FixedUpdate). Platforms bounce you with PlatformJump(force).
/// This component calls the game's own PlatformJump every time the player starts falling while
/// the run is in the Playing state, so the player keeps climbing until the finish line
/// (AscendingHeightsFinishLineDetector), which ends the run as a win the normal way.
/// </summary>
public class AutoClimber : MonoBehaviour
{
    private const float FallingVelocity = -0.1f;

    private bool _wasPlaying;
    private int _jumps;
    private bool _finishing;
    private bool _errorLogged;

    private void Update()
    {
        if (Input.GetKeyDown(Plugin.Settings.ToggleKey.Value))
        {
            Plugin.Settings.Enabled.Value = !Plugin.Settings.Enabled.Value;
            string state = Plugin.Settings.Enabled.Value ? "on" : "off";
            Plugin.Logger.Msg($"Ascending Heights Autopilot {(Plugin.Settings.Enabled.Value ? "enabled" : "disabled")}");
            try { Plugin.ModHelperInstance?.ShowNotification($"Ascending Heights Autopilot {state}", false); }
            catch { /* optional */ }
        }

        try { Tick(); }
        catch (Exception e)
        {
            if (!_errorLogged) { _errorLogged = true; Plugin.Logger.Error($"Auto climb failed: {e}"); }
        }
    }

    private void Tick()
    {
        var ctrl = AscendingHeightsController.instance;
        bool playing = ctrl != null
                       && ctrl.currentState == AscendingHeightsController.AscendingHeightsState.Playing;

        if (playing != _wasPlaying)
        {
            _wasPlaying = playing;
            if (playing)
            {
                _jumps = 0;
                _finishing = false;
                _walkLogged = false;
                _bridge = null;
                _finishLine = null;
                _finishTrigger = null;
                _arrivedAt = -1f;
                _teleportLogged = false;
                Plugin.Logger.Msg($"Ascending Heights started (auto climb {(Plugin.Settings.Enabled.Value ? "on" : "off")})");
            }
            else
            {
                Plugin.Logger.Msg($"Ascending Heights ended (auto jumps: {_jumps})");
                _bridge = null;
                _finishLine = null;
                _finishTrigger = null;
            }
        }

        if (!playing || !Plugin.Settings.Enabled.Value) return;

        var player = PlayerMovement.instance;
        if (player == null || player.rigidBody == null) return;

        // Once the player got above the finish bridge, stay in "finishing" mode for the rest of the run:
        // after landing, the player's position is level with the bridge (not above it), so re-checking
        // the height would stop the walk halfway.
        if (Plugin.Settings.LandOnFinish.Value && ctrl.finishMapSpawned && (_finishing || IsAboveFinish(player)))
        {
            if (!_finishing)
            {
                _finishing = true;
                Plugin.Logger.Msg($"Above the finish bridge (player y={player.transform.position.y:0.0}, bridge y={_bridge.transform.position.y:0.0}): landing and walking to the finish.");
            }

            // Fell well below the bridge (walked off it): jump back up.
            if (_bridge != null && player.transform.position.y < _bridge.transform.position.y - 3f
                && player.rigidBody.linearVelocity.y < FallingVelocity)
            {
                ctrl.PlatformJump(ctrl.normalPlatformForce);
                _jumps++;
                return;
            }

            if (Plugin.Settings.WalkToFinish.Value) WalkToFinish(player);
            return;
        }

        if (player.rigidBody.linearVelocity.y >= FallingVelocity) return;

        float force = Plugin.Settings.UseBoostForce.Value ? ctrl.boostPlatformForce : ctrl.normalPlatformForce;
        ctrl.PlatformJump(force);
        _jumps++;
    }

    private AscendingHeightsFinishBridge _bridge;
    private AscendingHeightsFinishLineDetector _finishLine;
    private bool _walkLogged;

    private bool IsAboveFinish(PlayerMovement player)
    {
        if (_bridge == null) _bridge = FindObjectOfType<AscendingHeightsFinishBridge>();
        if (_bridge == null) return false;
        return player.transform.position.y > _bridge.transform.position.y;
    }

    private Collider2D _finishTrigger;
    private float _arrivedAt = -1f;
    private bool _teleportLogged;

    /// <summary>
    /// The reward/finish trigger (AscendingHeightsFinishLineDetector) is to the side of the bridge.
    /// Slide the player toward the trigger collider's center (not the object's position: the collider is offset).
    /// When the player touches it the game ends the run (or opens the chest) exactly as when walking there by hand.
    /// If the player is over it for a while and the run has not ended (e.g. landed beside it), put the player inside it.
    /// </summary>
    private void WalkToFinish(PlayerMovement player)
    {
        if (_finishLine == null) _finishLine = FindObjectOfType<AscendingHeightsFinishLineDetector>();
        if (_finishLine != null && _finishTrigger == null) _finishTrigger = FindTrigger(_finishLine);

        var rb = player.rigidBody;
        Vector2 pos = rb.position;
        float targetX = pos.x + 1000f; // default: right
        if (_finishTrigger != null) targetX = _finishTrigger.bounds.center.x;
        else if (_finishLine != null) targetX = _finishLine.transform.position.x;

        if (!_walkLogged)
        {
            _walkLogged = true;
            string target = _finishTrigger != null
                ? $"finish trigger x={_finishTrigger.bounds.min.x:0.0}..{_finishTrigger.bounds.max.x:0.0}, y={_finishTrigger.bounds.min.y:0.0}..{_finishTrigger.bounds.max.y:0.0}"
                : _finishLine != null ? $"finish object at x={targetX:0.0} (no trigger found)" : "no finish found, going right";
            Plugin.Logger.Msg($"Walking from x={pos.x:0.0} y={pos.y:0.0} to {target}");
        }

        float dx = targetX - pos.x;
        if (Mathf.Abs(dx) > 0.05f)
        {
            _arrivedAt = -1f;
            float step = Mathf.Min(Mathf.Max(0.1f, Plugin.Settings.WalkSpeed.Value) * Time.deltaTime, Mathf.Abs(dx));
            rb.position = new Vector2(pos.x + Mathf.Sign(dx) * step, pos.y);
            return;
        }

        // Over the finish: let the player fall into it; if nothing happens, put the player inside the trigger.
        if (_arrivedAt < 0f)
        {
            _arrivedAt = Time.unscaledTime;
            Plugin.Logger.Msg($"Reached the finish x={pos.x:0.0} (player y={pos.y:0.0}).");
        }
        if (_finishTrigger != null && Time.unscaledTime - _arrivedAt > 2f)
        {
            if (!_teleportLogged)
            {
                _teleportLogged = true;
                Plugin.Logger.Msg($"Run did not end after reaching the finish (player y={pos.y:0.0}); moving the player into the finish trigger.");
            }
            rb.position = _finishTrigger.bounds.center;
            _arrivedAt = Time.unscaledTime;
        }
    }

    private static Collider2D FindTrigger(AscendingHeightsFinishLineDetector detector)
    {
        Collider2D any = null;
        foreach (var c in detector.GetComponentsInChildren<Collider2D>())
        {
            if (c == null) continue;
            if (c.isTrigger) return c;
            any ??= c;
        }
        return any;
    }
}
