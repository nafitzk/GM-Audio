using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace Gma
{
    public class GmaLibrary
    {
        // data
        public HashSet<GmaBaseAudio> audioLibrary { get; set; } = new HashSet<GmaBaseAudio> ();
        public HashSet<GmaScene> scenes { get; set; } = new HashSet<GmaScene>();

        // callbacks
        public event LibraryLoadEvent? onLibraryLoad;
        public event LibrarySceneCreatedEvent? OnSceneCreated;
        public event LibraryAudioCreatedEvent? OnAudioCreated;
        public event LibraryAudioRemovedEvent? OnAudioRemoved;

        public GmaLibrary()
        {

        }

        public void Shutdown()
        {
            SaveLibrary();
        }

        // create and populate a new library from the library json file
        public static GmaLibrary LoadLibrary()
        {
            string fileName = "library.gmalib";
            try
            {
                string json = File.ReadAllText(fileName);
                return JsonSerializer.Deserialize<GmaLibrary>(json);
            }
            catch (Exception e)
            {
                Debug.Print("Error loading library: "+e.Message);
                return new GmaLibrary();
            }
            
        }

        // save the library to a json file
        public void SaveLibrary()
        {
            Debug.Print("Saving Library...");
            string fileName = "library.gmalib";
            using FileStream createStream = File.Create(fileName);

            var options = new JsonSerializerOptions
            { 
                WriteIndented = true 
            };
            JsonSerializer.Serialize(createStream, this, options);
            Debug.Print("Done!");
        }

        public void CreateScene(string name)
        {
            GmaScene scene = new GmaScene(name);
            if (scenes.Add(scene)) OnSceneCreated!(scene);
        }

        public void AddAudio(GmaBaseAudio audio)
        {
            if(audioLibrary.Add(audio)) OnAudioCreated!(audio);
        }

        public void AddAudio(GmaScene scene, GmaBaseAudio audio)
        {
            //AddAudio(audio);
            scene.AddAudio(audio);
        }

        public void DeleteAudio(GmaScene scene, GmaBaseAudio audio)
        {
            scene.RemoveAudio(audio);
            if (audioLibrary.Remove(audio))
            {
                OnAudioRemoved!(audio);
            }
        }
    }

    public class GmaScene
    {
        // data
        public Guid guid { get; set; }
        public string name { get; set; } = string.Empty;
        public HashSet<GmaBaseAudio> sceneAudio { get; set; } = new HashSet<GmaBaseAudio>();

        // callbacks
        public event SceneUpdatedEvent? OnSceneUpdated;
        public event AudioAddedEvent? OnAudioAdded;
        public event AudioRemovedEvent? OnAudioRemoved;

        public GmaScene(string name) 
        {
            guid = Guid.NewGuid();
            this.name = name;
        }

        public void AddAudio(GmaBaseAudio audio)
        {
            if(sceneAudio.Add(audio))
                OnAudioAdded!(audio);
        }

        public void RemoveAudio(GmaBaseAudio audio)
        {
            if (sceneAudio.Remove(audio))
            {
                //OnAudioRemoved!(audio);
            }
        }
    }
}
