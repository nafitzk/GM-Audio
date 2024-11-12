using Core.Opus;
using Discord;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Audio
{
    // a provider that reads from a single audio file
    public class SingleFileProvider : ISampleProvider
    {
        protected string _file;

        protected WaveStream _fileReader;

        protected ISampleProvider _sampler;

        protected bool _loop = false;
        public bool Loop { get => _loop; set => _loop = value; }

        public SingleFileProvider(string file, bool loop)
        {
            _file = file;

            // load the file
            if (file.EndsWith(".opus")) // TODO make this sexier idk
            {
                _fileReader = new OpusFileReader(file);
            }
            else
            {
                _fileReader = new AudioFileReader(file);
            }

            // float file sampler
            _sampler = new SampleChannel(_fileReader, true);

            // resampler is required
            if (_fileReader.WaveFormat.SampleRate != 48000)
            {
                _sampler = new WdlResamplingSampleProvider(_sampler, 48000);
            }

            Loop = loop;
        }

        public SingleFileProvider(string file) : this(file, false) { }

        public WaveFormat WaveFormat => _sampler.WaveFormat;

        public virtual int Read(float[] buffer, int offset, int count)
        {
            int b = _sampler.Read(buffer, offset, count);
            if(b < count && Loop)
            {
                if(_fileReader.CanSeek) // restart stream if seeking is supported
                {
                    _fileReader.Position = 0; 
                }
                else // if file cant seek (opus), just make a new ass stream
                {
                    if (_file.EndsWith(".opus"))
                    {
                        _fileReader = new OpusFileReader(_file);
                    }
                    else
                    {
                        _fileReader = new AudioFileReader(_file);
                    }
                    _sampler = new SampleChannel(_fileReader);
                }

                // fire loop event?

                return _sampler.Read(buffer, b, count-b) + b;
            }

            return b;
        }
    }

    // converts the weird multi channel dice audio into 2 channel
    // takes a channel, makes it stereo
    public class MultiChannelFileProvider : ISampleProvider
    {
        protected ISampleProvider _stream;
        protected MultiplexingSampleProvider _multiplexer;

        public int Channel {get; set;}

        private WaveFormat _format;
        public WaveFormat WaveFormat => _format;

        public MultiChannelFileProvider(int channel, ISampleProvider stream)
        {
            _format = WaveFormat.CreateIeeeFloatWaveFormat(stream.WaveFormat.SampleRate, 2);
            _stream = stream;

            Channel = channel;

            _multiplexer = new MultiplexingSampleProvider(new[] { _stream }, _format.Channels);
            _multiplexer.ConnectInputToOutput(Channel, 0);
            _multiplexer.ConnectInputToOutput(Channel, 1);
        }

        // looping is not supported (should it be?)
        public int Read(float[] buffer, int offset, int count)
        {
            return _multiplexer.Read(buffer, offset, count);
        }
    }

    public class FileListProvider : ISampleProvider
    {
        private List<string> _files = new List<string>();
        private IEnumerator<string> _iterator;
        private bool _shuffle = false;

        private WaveFormat _format; // need our own format, since we cant guarantee all the files have the same format
        public WaveFormat WaveFormat => _format;

        // when fading between files, we need two streams
        private ISampleProvider? _streamA;
        private ISampleProvider? _streamB;

        // for volume fade between tracks
        private FadeInOutSampleProvider? _fadeA;
        private FadeInOutSampleProvider? _fadeB;

        // final mix
        private MixingSampleProvider _mix;

        // if rng is needed, here it is
        private Random _rng = new Random(Guid.NewGuid().GetHashCode());

        private int _channel = 0;
        private int _segmentsRemaining = 10; // number of audio segments until a channel switch

        public FileListProvider(List<string> files, bool shuffle)
        {
            _files = files;
            _iterator = files.GetEnumerator();
            _shuffle = shuffle; // TODO shuffle

            _format = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);

            _mix = new MixingSampleProvider(_format);

            if (_iterator.MoveNext())
            {
                Next(_iterator.Current);
            }
        }

        private void Next(string file)
        {
            _streamA = new SingleFileProvider(file);

            _mix.RemoveAllMixerInputs();

            // battlefield multi channel shit
            if (_streamA.WaveFormat.Channels > 2)
            {
                _streamA = new MultiChannelFileProvider(_channel, _streamA);

                // if enough segments have passed, change random channels
                --_segmentsRemaining;
                //Debug.Print("Segments remaining: {0}", _segmentsRemaining);

                if (_segmentsRemaining == 0) // load up new channel, fade it in
                {
                    _segmentsRemaining = _rng.Next(4,10);

                    int newChannel = _rng.Next(_streamA.WaveFormat.Channels);
                    //Debug.Print("Switching channels! {0} -> {1}", _channel, newChannel);

                    _channel = newChannel;
                    _streamB = new MultiChannelFileProvider(_channel, new SingleFileProvider(file));

                    _fadeA = new FadeInOutSampleProvider(_streamA, false);
                    _fadeB = new FadeInOutSampleProvider(_streamB, true);

                    _fadeA.BeginFadeOut(2000);
                    _fadeB.BeginFadeIn(2000);

                    _mix.AddMixerInput(_fadeA);
                    _mix.AddMixerInput(_fadeB);
                }
                else
                {
                    _mix.AddMixerInput(_streamA);
                }
            }
            else
            {
                _mix.AddMixerInput(_streamA);
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int b = _mix!.Read(buffer, offset, count);

            if(b < count) // the current track has run dry
            {
                if (!_iterator.MoveNext())
                {
                    _iterator.Reset();
                    _iterator.MoveNext();
                }
                Next(_iterator.Current);
                b += _mix!.Read(buffer, b, count - b);
            }

            return b;
        }
    }

    public class RandomFileProvider : ISampleProvider
    {
        private WaveFormat _format;
        public WaveFormat WaveFormat => _format;

        private List<string> _files = new List<string>();

        private MixingSampleProvider _mix;

        private Random _rng = new Random(Guid.NewGuid().GetHashCode());

        private int _intervalMin = -1;
        private int _intervalMax = -1;
        private int _intervalTime = 0;

        public RandomFileProvider(List<string> files, int intervalMinMillis, int intervalMaxMillis)
        {
            _format = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
            _intervalMin = intervalMinMillis / 1000 * _format.SampleRate;
            _intervalMax = intervalMaxMillis / 1000 * _format.SampleRate;
            _files = files;

            _mix = new MixingSampleProvider(_format);

            PlayRandom();
        }

        public RandomFileProvider(List<string> files) : this(files, -1, -1) {  }
        public RandomFileProvider(List<string> files, int intervalMillis) : this(files, intervalMillis, intervalMillis) {  }

        protected void PlayRandom()
        {
            string file = _files[_rng.Next(_files.Count)];

            _mix.AddMixerInput(new SingleFileProvider(file));

            //Debug.Print("random mixer size: {0}", _mix.MixerInputs.Count());

            if(_intervalMin > 0 && _intervalMax > 0)
            {
                if(_intervalMax == _intervalMin)
                {
                    _intervalTime = _intervalMin;
                }
                else
                {
                    _intervalTime = _rng.Next(_intervalMin, _intervalMax);
                }
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int b = _mix.Read(buffer, offset, count);

            if(b < count)
            {
                // we are doing nexts
                if (_intervalMin == 0 && _intervalMax == 0)
                {
                    PlayRandom();
                    b += _mix.Read(buffer, b, count - b);
                }
                else if(_intervalMin > 0 && _intervalMin > 0) // if we are waiting for an interval, fill with silence
                {
                    Array.Clear(buffer, b, count - b);
                    b = count;
                }
            }

            // we are doing intervals
            if (_intervalMin > 0 && _intervalMax > 0)
            {
                _intervalTime -= b / 2;
                if (_intervalTime <= 0)
                {
                    PlayRandom();
                }
            }

            return b;
        }
    }
}
