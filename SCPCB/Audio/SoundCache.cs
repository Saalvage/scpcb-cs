﻿using SCPCB.Graphics.Caches;

namespace SCPCB.Audio;

public class SoundCache : BaseCache<(string, Channels?), AudioFile> {
    public AudioFile GetSound(string path, Channels? convertChannels = null) {
        if (!_dic.TryGetValue((path, convertChannels), out var sound)) {
            Log.Information("Loading sound {SoundPath} (Channels: {Channels})", path, convertChannels);
            sound = new(path, convertChannels);
            _dic.Add((path, convertChannels), sound);
        }
        return sound;
    }
}
