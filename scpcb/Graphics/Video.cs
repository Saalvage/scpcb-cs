using System.Diagnostics;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics; 

public class Video : Disposable, IUpdatable {
    private readonly MediaFile _media;
    private readonly VideoTexture _texture;

    private float _acc;
    private readonly float _timePerFrame;

    public ICBTexture Texture => _texture;

    public float Speed { get; set; } = 1f;

    public bool Loop { get; set; } = true;

    public bool Paused { get; set; } = false;

    public TimeSpan Position { get => _media.Video.Position; set => ResetTo(value); }

    public event Action Finished;

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

        public void UpdateData(ImageData data) {
            _gfx.UpdateTexture(_texture, data.Data, 0, 0, 0, Width, Height, 1, 0, 0);
        }

        public void GenerateMipmaps(CommandList commands) {
            throw new NotImplementedException();
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
        _texture = new(gfxRes, (uint)_media.Video.Info.FrameSize.Width, (uint)_media.Video.Info.FrameSize.Height);
        _timePerFrame = 1f / (float)_media.Video.Info.AvgFrameRate;
        AdvanceFrame();
    }

    public void AdvanceFrame() {
        if (_media.Video.TryGetNextFrame(out var data)) {
            _texture.UpdateData(data);
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
        while (_acc >= _timePerFrame) {
            AdvanceFrame();
            _acc -= _timePerFrame;
        }
    }

    private void ResetTo(TimeSpan time) {
        var data = _media.Video.GetFrame(time);
        _texture.UpdateData(data);
    }

    protected override void DisposeImpl() {
        _media.Dispose();
        Texture.Dispose();
    }
}
