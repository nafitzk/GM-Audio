using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gma
{
    // library events
    public delegate void LibraryLoadEvent();
    public delegate void LibrarySceneCreatedEvent(GmaScene scene);
    public delegate void LibrarySceneRemovedEvent(GmaScene scene);
    public delegate void LibraryAudioCreatedEvent(GmaBaseAudio audio);
    public delegate void LibraryAudioRemovedEvent(GmaBaseAudio audio);

    // scene events
    public delegate void SceneUpdatedEvent();
    public delegate void AudioAddedEvent(GmaBaseAudio audio);
    public delegate void AudioRemovedEvent(GmaBaseAudio audio);

    // audio events
    public delegate void AudioUpdatedEvent();

    // mix events
    public delegate void MixTrackCreatedEvent(GmaMixTrack track);
    public delegate void MixTrackRemovedEvent(GmaMixTrack track);

    // mix track events
    public delegate void MixTrackItemRemoved();
}
