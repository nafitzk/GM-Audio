using Discord.WebSocket;
using Discord.Audio;
using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Core
{
    public class DiscordPlayer : IWavePlayer
    {
        // discord shit

        private DiscordSocketClient _client;
        private IAudioClient _voice;
        private AudioOutStream _stream;

        // waveplayer shit
        private WaveBuffer _buffer;

        private volatile PlaybackState _state;
        public PlaybackState PlaybackState => _state;

        private WaveFormat _format;
        public WaveFormat OutputWaveFormat => _format;

        private IWaveProvider _provider;

        public DiscordPlayer()
        {
            _format = new WaveFormat(48000, 2);

            var config = new DiscordSocketConfig();
            config.GatewayIntents = Discord.GatewayIntents.GuildVoiceStates | Discord.GatewayIntents.GuildMembers;
            config.AlwaysDownloadUsers = true;

            _client = new DiscordSocketClient();

            _client.Connected += () =>
            {
                Debug.Print("Discord Connected!");
                return Task.CompletedTask;
            };

            // connect to the desired user's voice channel
            // when discord is ready
            _client.Ready += async () =>
            {
                Debug.Print("Discord Ready!");

                foreach(var guild in  _client.Guilds)
                {
                    foreach(var guildUser in guild.Users)
                    {
                        if(guildUser.Username == user)
                        {
                            var voiceChannel = guildUser.VoiceChannel;
                            if(voiceChannel != null)
                            {
                                _voice = await voiceChannel.ConnectAsync(false);
                                if(_voice != null)
                                {
                                    Debug.Print("Connected to voice!");
                                    _stream = _voice.CreatePCMStream(AudioApplication.Music, bufferMillis:50);
                                }
                                else
                                {
                                    Debug.Print("Error connecting to voice!");
                                }
                            }
                        }
                    }
                }
            };
        }

        public async void Dispose()
        {
            await _voice.StopAsync();
            _voice.Dispose();

            await _client.StopAsync();
            await _client.DisposeAsync();
        }

        public async void Init(IWaveProvider waveProvider)
        {
            _provider = new WaveFloatTo16Provider(waveProvider);

            // 50ms buffer size
            int size = waveProvider.WaveFormat.ConvertLatencyToByteSize(50);
            _buffer = new WaveBuffer(size);
            
            // init discord
            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();
        }

        public void Pause()
        {
            
        }

        public void Play()
        {
            if (PlaybackState == PlaybackState.Stopped)
            {
                _state = PlaybackState.Playing;
                ThreadPool.QueueUserWorkItem(state => DoPlayTask(), null);
            }
        }

        private async void DoPlayTask()
        {
            try
            {
                while (PlaybackState != PlaybackState.Stopped)
                {
                    int b = _provider.Read(_buffer.ByteBuffer, 0, _buffer.MaxSize);
                    await _stream.WriteAsync(_buffer.ByteBuffer, 0, b);
                }
            }
            finally
            {
                await _stream.FlushAsync();
            }
        }

        public void Stop()
        {
            _stream.Dispose();
        }

        public event EventHandler<StoppedEventArgs> PlaybackStopped;
        public float Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
