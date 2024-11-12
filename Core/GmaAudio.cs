using Core.Audio;
using Core.Opus;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gma
{
    public enum PlayMode
    {
        ONCE,
        LOOP
    }

    // an audio-like object.
    [JsonDerivedType(typeof(FileAudio), typeDiscriminator: nameof(FileAudio))]
    [JsonDerivedType(typeof(FileListAudio), typeDiscriminator: nameof(FileListAudio))]
    public abstract class GmaBaseAudio
    {
        public Guid Guid { get; set; }
        public string Name { get; set; } = string.Empty;
        public float Volume { get; set; } = 0.0f;
        public float FadeInTime { get; set; } = 0.0f;
        public float FadeOutTime { get; set; } = 0.0f;
        public PlayMode Mode { get; set; } = PlayMode.ONCE;

        public GmaBaseAudio(string name, float volume, PlayMode mode) 
        {
            Guid = Guid.NewGuid();
            this.Name = name;
            this.Volume = volume;
            this.Mode = mode;
        }

        // returns a way to sample and play this audio
        public abstract ISampleProvider Play();
    }

    // audio that plays a single audio file
    public class FileAudio : GmaBaseAudio
    {
        public string File { get; set; } = string.Empty;

        public FileAudio() : this(string.Empty, string.Empty, 0.5f, PlayMode.ONCE)
        {

        }

        public FileAudio(string file, string name, float volume, PlayMode mode) : base(name, volume, mode)
        {
            this.File = file;
        }

        public override ISampleProvider Play()
        {
            return new SingleFileProvider(File, Mode == PlayMode.LOOP);
        }
    }

    // audio that works with a list of files
    // can specify an interval range between the next in the list
    // can also shuffle
    public class FileListAudio : GmaBaseAudio
    {
        public List<string> Files { get; set; } = new List<string>();

        public bool Shuffle { get; set; } = false;

        public float IntervalMin { get; set; } = 0.0f;
        public float IntervalMax { get; set; } = 0.0f;

        public FileListAudio() : this (string.Empty, 0.5f, PlayMode.ONCE)
        {

        }

        public FileListAudio(string name, float volume, PlayMode mode) : base(name, volume, mode)
        {
            
        }

        public override ISampleProvider Play()
        {
            if(Shuffle) return new RandomFileProvider(Files, (int)(IntervalMin * 1000.0f), (int)(IntervalMax * 1000.0f));

            return new FileListProvider(Files, false);
        }
    }

    // an invdivual piece of a sequence
    public class SequenceNode
    {
        // delay from main sequence starting until this node does
        public float StartTime { get; set; } = 0.0f;

        // the audio that plays for this node
        public GmaBaseAudio Audio { get; set; }

        public SequenceNode(float startTime, GmaBaseAudio audio)
        {
            StartTime = startTime;
            Audio = audio;
        }
    }

    // a series of audios played at particular times
    public class SequenceAudio : GmaBaseAudio
    {
        public List<SequenceNode> Nodes { get; set; } = new List<SequenceNode>();

        public SequenceAudio(string name, float volume, PlayMode mode) : base(name, volume, mode)
        {

        }

        public override ISampleProvider Play()
        {
            throw new NotImplementedException();
        }
    }
}
