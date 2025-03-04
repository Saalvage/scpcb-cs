﻿using System.Diagnostics;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using SCPCB.Entities;
using SCPCB.Graphics.Primitives;
using SCPCB.Utility;
using Veldrid;

namespace SCPCB.Graphics.Textures; 

public class Video : Disposable, IUpdatable {
    private readonly MediaFile _media;
    private readonly VideoTexture _texture;
    private readonly byte[] _buffer;

    private float _acc;
    private readonly float _timePerFrame;

    public ICBTexture Texture => _texture;

    public float Speed { get; set; } = 1f;

    public bool Loop { get; set; } = false;

    public bool Paused { get; set; } = false;

    public TimeSpan Position { get => _media.Video.Position; set => ResetTo(value); }

    public event Action? Finished;

    // TODO: Do we really need this to be its own class?
    // Modify CBTexture to allow for the same behavior?
    private class VideoTexture : Disposable, ICBTexture {
        private readonly GraphicsDevice _gfx;

        private readonly Texture _texture;
        public TextureView View { get; }

        public uint Width { get; }
        public uint Height { get; }

        public VideoTexture(GraphicsResources gfxRes, uint width, uint height) {
            _gfx = gfxRes.GraphicsDevice;

            Width = width;
            Height = height;

            _texture = gfxRes.GraphicsDevice.ResourceFactory.CreateTexture(new(width, height, 1, 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));

            View = gfxRes.GraphicsDevice.ResourceFactory.CreateTextureView(_texture);
        }

        public void UpdateData(Span<byte> data) {
            _gfx.UpdateTexture(_texture, data, 0, 0, 0, Width, Height, 1, 0, 0);
        }

        protected override void DisposeImpl() {
            View.Dispose();
            _texture.Dispose();
        }
    }

    public Video(GraphicsResources gfxRes, string file) {
        _media = MediaFile.Open(file, new() { VideoPixelFormat = ImagePixelFormat.Rgba32 });
        Debug.Assert(_media.HasVideo);
        Debug.Assert(!_media.Video.Info.IsVariableFrameRate);
        var width = _media.Video.Info.FrameSize.Width;
        var height = _media.Video.Info.FrameSize.Height;
        _buffer = new byte[width * height * 4];
        _texture = new(gfxRes, (uint)width, (uint)height);
        _timePerFrame = 1f / (float)_media.Video.Info.AvgFrameRate;
        AdvanceFrame();
    }

    public void AdvanceFrame() {
        if (_media.Video.TryGetNextFrame(_buffer)) {
            _texture.UpdateData(_buffer);
        } else if (Loop) {
            ResetTo(TimeSpan.Zero);
        } else {
            Finished?.Invoke();
        }
    }

    public void Update(float delta) {
        if (Paused) {
            return;
        }

        _acc += delta * Speed;
        if (_acc >= 2 * _timePerFrame) {
            var time = _media.Video.Position + TimeSpan.FromSeconds(_acc);
            time = TimeSpan.FromTicks(time.Ticks % _media.Video.Info.Duration.Ticks);
            ResetTo(time);
        } else if (_acc >= _timePerFrame) {
            AdvanceFrame();
        }
        _acc %= _timePerFrame;
    }

    private void ResetTo(TimeSpan time) {
        var data = _media.Video.GetFrame(time);
        _texture.UpdateData(data.Data);
    }

    protected override void DisposeImpl() {
        _media.Dispose();
        Texture.Dispose();
    }
}
