using UnityEngine;

public class FlockChildSound : MonoBehaviour
{
    public AudioSource _audio;
    public AudioClip[] _idleSounds;
    public float _idleSoundRandomChance = .05f;

    public AudioClip[] _flightSounds;
    public float _flightSoundRandomChance = .05f;


    public AudioClip[] _scareSounds;
    public float _pitchMin = .85f;
    public float _pitchMax = 1.0f;

    public float _volumeMin = .6f;
    public float _volumeMax = .8f;

    FlockChild _flockChild;
    bool _hasLanded;

    public void Start()
    {
        _flockChild = GetComponent<FlockChild>();
        InvokeRepeating( "PlayRandomSound", 0.1f, 1.0f );
        if ( _scareSounds.Length > 0 )
        {
            InvokeRepeating( "ScareSound", 1.0f, .01f );
        }
    }

    public void PlayRandomSound()
    {
        if ( gameObject.activeInHierarchy && enabled )
        {
            if ( !_audio.isPlaying && _flightSounds.Length > 0 && _flightSoundRandomChance > Random.value &&
                 !_flockChild._landing )
            {
                _hasLanded = false;
                _audio.clip = _flightSounds[Random.Range( 0, _flightSounds.Length )];
                _audio.pitch = Random.Range( _pitchMin, _pitchMax );
                _audio.volume = Random.Range( _volumeMin, _volumeMax );
                _audio.Play();
            }
            else if ( !_audio.isPlaying && _idleSounds.Length > 0 && _idleSoundRandomChance > Random.value &&
                      _flockChild._landing )
            {
                _audio.clip = _idleSounds[Random.Range( 0, _idleSounds.Length )];
                _audio.pitch = Random.Range( _pitchMin, _pitchMax );
                _audio.volume = Random.Range( _volumeMin, _volumeMax );
                _audio.Play();
                _hasLanded = true;
            }
            else if ( _flockChild._landing )
            {
                if ( !_hasLanded )
                {
                    _hasLanded = true;
                    _audio.Stop();
                }
            }
        }
    }

    public void ScareSound()
    {
        if ( gameObject.activeInHierarchy )
        {
            if ( _hasLanded && !_flockChild._landing && _idleSoundRandomChance * 2 > Random.value )
            {
                _audio.clip = _scareSounds[Random.Range( 0, _scareSounds.Length )];
                _audio.volume = Random.Range( _volumeMin, _volumeMax );
                _audio.PlayDelayed( Random.value * .2f );
                _hasLanded = false;
            }
        }
    }
}