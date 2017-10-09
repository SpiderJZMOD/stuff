using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RuntimeAudioClipLoader;

public class MusicPlayer : MonoBehaviour {

    public static System.Random Rand = new System.Random(System.Guid.NewGuid().GetHashCode());
    
    public static string DirectoryPath = @"D:\Me\Music\!SDX\";

    static List<string> AllowedExtensions = new List<string>();
    static List<string> Playlist = new List<string>();

    AudioSource audioSource;
    public bool Randomise;


    float SongStartTime;

    int PlayIndex = -1;


    public void Read(BinaryReader bin)
    {
        int index = bin.ReadInt32();
        Randomise = bin.ReadBoolean();
        float songTime = bin.ReadSingle();
        float serverTime = bin.ReadSingle();

        int samples = bin.ReadInt32();

        float diff = Time.time - serverTime;
        float newSongTime = Mathf.Max(0, songTime + diff);


        //Debug.Log("Index: " + index);
        //Debug.Log("Random: " + Randomise);
        //Debug.Log("SongStart: " + songTime);
        //Debug.Log("ServerTime: " + serverTime);
        //Debug.Log("Diff: " + diff);
        //Debug.Log("NewSongStart: " + newSongTime);
        //Debug.Log("Time Sample: " + samples);
        
        PlayIndexAtTime(index, newSongTime);

    }
    public void Write(BinaryWriter bin)
    {
        bin.Write(PlayIndex);
        bin.Write(Randomise);

        float currentSongMoment = Time.time - SongStartTime;
        bin.Write(currentSongMoment);
        bin.Write(Time.time);
        bin.Write(audioSource.timeSamples);

    }

    public void FirstTime()
    {
        LoadPropertiesFromXML();
        PlayNext();
    }

    static MusicPlayer()
    {
        AllowedExtensions.Add(".mp3");
        AllowedExtensions.Add(".wav");
        AllowedExtensions.Add(".ogg");
        AllowedExtensions.Add(".aiff");
    }
    void Start()
    {
        audioSource = this.gameObject.GetComponent<AudioSource>();
        //audioSource = this.gameObject.GetComponent<AudioSource>();
        //LoadPlaylist(DirectoryPath, true);
        //PlayNext();
    }


    private void LoadPropertiesFromXML()
    {
        audioSource = this.gameObject.GetComponent<AudioSource>();
        string sRandom = HalHelper.GetBlockPropertyAsString("Radio", "RandomisePlaylist");
        bool.TryParse(sRandom, out Randomise);

        DirectoryPath = HalHelper.GetBlockPropertyAsString("Radio", "MusicDirectory").Replace(@"\\", @"\");
        LoadPlaylist(DirectoryPath, true);
    }


    public void PlayIndexAtTime(int index, float time)
    {
        this.CancelInvoke();

        PlayIndex = index < 0 ? 0 : index;
        if (Playlist.Count == 0)
        {
            audioSource = this.gameObject.GetComponent<AudioSource>();

            LoadPropertiesFromXML();
            if (Playlist.Count == 0)
            {
                return; // nothing to play
            }
        }

        if (PlayIndex > Playlist.Count)
        {
            PlayIndex = 0;
        }


        LoadClipFromFileSystem(Playlist[PlayIndex]);

        if (audioSource.clip == null)
        {
            PlayNext();
            return;
        }


        if (time > audioSource.clip.length)
        {
            Debug.Log("Song expired");
            PlayNext();
            return;
        }

 //       Debug.Log("Time: " + time);
        SongStartTime = Time.time - time;
        audioSource.time = time;
        audioSource.Play();
        this.Invoke("PlayNext", (audioSource.clip.length - time) + 0.5f);
    }
    public void PlayNext()
    {

        this.CancelInvoke();

        if (Playlist.Count == 0)
        {
            Debug.Log("No items in music playlist.");
            return; // nothing to play
        }

        if (Randomise)
        {
            PlayIndex = Rand.Next(0, Playlist.Count);
        }
        else
        {
            PlayIndex++;
        }


        if (PlayIndex > Playlist.Count)
        {
            PlayIndex = 0;
        }

        LoadClipFromFileSystem(Playlist[PlayIndex]);

        if (audioSource.clip == null)
        {
            PlayNext();
            return;
        }

        SongStartTime = Time.time;
        audioSource.Play();
        this.Invoke("PlayNext", audioSource.clip.length + 0.5f);

    }

    private void LoadClipFromFileSystem(string path)
    {

   //     Debug.Log("Trying: " + path);

        bool doStream = false;
        bool loadInBackground = true;

   //     Debug.Log("GetBytes");
        byte[] bytes = System.IO.File.ReadAllBytes(path);
  //      Debug.Log("Load Stream. Bytes: " + bytes.Length);
        Stream stream = new MemoryStream(bytes);
  //      Debug.Log("GetFOrmat. Stream null: " + (stream == null ? "yes" : "no"));
        AudioFormat format = RuntimeAudioClipLoader.Manager.GetAudioFormat(path);
   //     Debug.Log("GetFilename");
        string filename = System.IO.Path.GetFileName(path);
   //     Debug.Log("Filename : " + filename);

        if (format == AudioFormat.unknown) format = AudioFormat.mp3;
//        Debug.Log("Format: " + format.ToString());

        try
        {
          //  Debug.Log("LoadClip. Source null: " + (audioSource == null ? "yes" : "no"));
            if (audioSource == null)
            {
                audioSource = this.gameObject.GetComponent<AudioSource>();
            }

            audioSource.clip = RuntimeAudioClipLoader.Manager.Load(stream, format, filename, doStream, loadInBackground, true);
   //         Debug.Log("Done");
        }
        catch (System.Exception ex)
        {
            audioSource.clip = null;
        }

        //yield return null;
    }

    IEnumerator DownloadClipFromUrl(string url)
    {

        bool doStream = false;
        bool loadInBackground = true;

        // Debug.Log("loading " + url);
        WWW www = new WWW(url);
        yield return www;
        // Debug.Log("loaded " + url + " got bytes:" + www.bytes.Length);
        Stream stream = new MemoryStream(www.bytes);
        AudioFormat format = RuntimeAudioClipLoader.Manager.GetAudioFormat(url);
        if (format == AudioFormat.unknown) format = AudioFormat.mp3;
        audioSource.clip = RuntimeAudioClipLoader.Manager.Load(stream, format, url, doStream, loadInBackground, true);
    }
    private void LoadPlaylist(string dir, bool SearchSubFolders)
    {

        //Might be a good idea to store the playlist in a file rather than all in memory
        Playlist.Clear();

        Debug.Log("Loading playlist from: " + dir);
        LoadDirectory(dir, SearchSubFolders);
    }

    private void LoadDirectory(string dir, bool SearchSubFolders)
    {

        foreach (string file in System.IO.Directory.GetFiles(dir))
        {
            string ext = System.IO.Path.GetExtension(file);

            if (AllowedExtensions.Contains(ext))
            {
                Playlist.Add(file);
            }
        }
        if (SearchSubFolders)
        {
            foreach (string d in System.IO.Directory.GetDirectories(dir))
            {
                LoadDirectory(d, SearchSubFolders);
            }
        }
    }
	


	// Update is called once per frame
	void Update () {
	
	}
}
