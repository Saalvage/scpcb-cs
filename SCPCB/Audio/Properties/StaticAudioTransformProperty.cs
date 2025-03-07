using System.Numerics;
using OpenTK.Audio.OpenAL;

namespace SCPCB.Audio.Properties;

public record StaticAudioTransformProperty(Vector3 Position, Vector3? Direction = null, Vector3? Velocity = null)
    : IAudioProperty {
    public void Apply(Source playback) {
        AL.Source(playback.Handle, ALSourceb.SourceRelative, false);
        AL.Source(playback.Handle, ALSource3f.Position, Position.X, Position.Y, Position.Z);
        if (Direction is { } dir) {
            AL.Source(playback.Handle, ALSource3f.Direction, dir.X, dir.Y, dir.Z);
        }
        if (Velocity is { } vel) {
            AL.Source(playback.Handle, ALSource3f.Velocity, vel.X, vel.Y, vel.Z);
        }
    }
}
