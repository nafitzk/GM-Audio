using Core;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Core.Opus;
using System.Diagnostics;

namespace Gma
{
    public class GmaMix
    {
        private Dictionary<ISampleProvider, GmaMixTrack> _tracks = new Dictionary<ISampleProvider, GmaMixTrack>();

        private GmaPlayerType _playerType = GmaPlayerType.Default;
        private IWavePlayer _player = new WaveOutEvent();

        private MixingSampleProvider _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(48000, 2)); // 48khz, stereo

        private IWavePlayer _preview = new WaveOutEvent();

        public GmaMix()
        {
            _player.Init(_mixer);

            _mixer.MixerInputEnded += OnMixerInputEnded;
        }

        private void OnMixerInputEnded(object? sender, SampleProviderEventArgs e)
        {
            GmaMixTrack track = _tracks[e.SampleProvider];
            if (_tracks.Remove(e.SampleProvider))
            {
                track.Dispose();
                OnMixTrackRemoved?.Invoke(track);
            }
        }

        public void Dispose()
        {
            _player.Dispose();
        }

        public void Play(GmaBaseAudio audio)
        {
            GmaMixTrack track = new GmaMixTrack(audio);

            _mixer.AddMixerInput(track.Stream);
            if (_player.PlaybackState != PlaybackState.Playing)
            {
                _player.Play();
            }

            _tracks[track.Stream] = track;
            OnMixTrackAdded?.Invoke(track);
        }

        public async void Stop(GmaMixTrack track)
        {
            if (_tracks.ContainsKey(track.Stream))
            {
                if (track.Audio.FadeOutTime > 0f)
                {
                    int t = (int)track.Audio.FadeOutTime * 1000;
                    track.Fade.BeginFadeOut(t);
                    await Task.Delay(t);
                }

                if (_tracks.Remove(track.Stream))
                {
                    _mixer.RemoveMixerInput(track.Stream);
                    track.Dispose();
                    OnMixTrackRemoved?.Invoke(track);
                }
            }
        }

        public void Clear()
        {
            foreach(var track in _tracks.Values)
            {
                Stop(track);
            }
        }

        public void PreviewPlay(GmaBaseAudio audio)
        {
            if (_preview.PlaybackState == PlaybackState.Playing) _preview.Stop();

            GmaMixTrack previewTrack = new GmaMixTrack(audio);

            _preview.Init(previewTrack.Stream);
            _preview.Play();
        }

        public void PreviewStop()
        {
            _preview.Stop();
        }

        public void SetPlayer(GmaPlayerType type)
        {
            if (type == _playerType) return;

            _player.Stop();
            _player.Dispose();

            switch (type)
            {
                case GmaPlayerType.Default:
                    _player = new WaveOutEvent();
                    break;
                case GmaPlayerType.Discord:
                    _player = new DiscordPlayer();
                    break;
            }

            _player.Init(_mixer);
        }

        public event MixTrackCreatedEvent? OnMixTrackAdded;
        public event MixTrackRemovedEvent? OnMixTrackRemoved;
    }

    public class GmaMixTrack
    {
        private readonly GmaBaseAudio _audio;
        public GmaBaseAudio Audio { get { return _audio; } }

        private readonly VolumeSampleProvider _stream;
        public VolumeSampleProvider Stream { get { return _stream; } }

        public FadeInOutSampleProvider Fade { get; }

        private WdlResamplingSampleProvider? _resampler;

        private ISampleProvider _reader;

        public float Volume
        {
            get { return _stream.Volume; }
            set { _stream.Volume = value; }
        }

        public GmaMixTrack(GmaBaseAudio audio)
        {
            _audio = audio;
            _reader = audio.Play();

            Fade = new FadeInOutSampleProvider(_reader, audio.FadeInTime > 0f);
            if (audio.FadeInTime > 0f)
            {
                Fade.BeginFadeIn((int)audio.FadeInTime * 1000);
            }

            if (_reader.WaveFormat.SampleRate != 48000)
            {
                _resampler = new WdlResamplingSampleProvider(Fade, 48000);
                _stream = new VolumeSampleProvider(_resampler);
            }
            else
            {
                _stream = new VolumeSampleProvider(Fade);
            }

            Volume = audio.Volume;
        }

        public void Dispose()
        {
            OnTrackRemoved?.Invoke();
           // _reader?.Dispose();
        }

        public event MixTrackItemRemoved? OnTrackRemoved;
    }
}
